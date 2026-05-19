using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private PlayerProfile playerProfile;
    private InventoryMain inventory;

    private Vector2 movement;
    private RaycastHit hit;
    private float rayDistance = 2;

    private Rigidbody rb;
    private InputAction moveAction;

    [SerializeField] private Transform pSprite;
    private Animator ani;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerProfile = GetComponent<PlayerProfile>();

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            moveAction = playerInput.actions.FindAction("Move");
    }

    void Start()
    {
        if (pSprite != null)
            ani = pSprite.GetComponentInChildren<Animator>();

        var inventoryObject = GameObject.Find("InventorySystem");
        if (inventoryObject != null)
            inventory = inventoryObject.GetComponent<InventoryMain>();
    }

    void OnEnable()
    {
        moveAction?.Enable();
        rb?.WakeUp();
    }

    void FixedUpdate()
    {
        if (rb == null || playerProfile == null)
            return;

        if (!GameplayInputUtility.IsGameplayInputAllowed(inventory))
        {
            movement = Vector2.zero;
            StopHorizontalMotion();
            return;
        }

        movement = ReadMovementInput();

        if (movement.sqrMagnitude > 0.01f)
            playerProfile.EnsureCanMove();

        ApplyMovement();

        UpdateWalkAnimation();
    }

    void ApplyMovement()
    {
        var speed = playerProfile.moveSpeed;
        if (movement.sqrMagnitude < 0.01f || speed < 0.01f)
        {
            StopHorizontalMotion();
            return;
        }

        var direction = new Vector3(movement.x, 0f, movement.y);
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        var delta = direction * (speed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + delta);
        rb.WakeUp();

        // 물리 엔진이 XZ 속도를 덮어쓰지 않도록 수평 속도만 제거
        StopHorizontalMotion();
    }

    void StopHorizontalMotion()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    Vector2 ReadMovementInput()
    {
        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (input.sqrMagnitude < 0.01f)
            input = ReadKeyboardWasd();

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        return input;
    }

    static Vector2 ReadKeyboardWasd()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector2.zero;

        float x = 0f;
        float y = 0f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;

        return new Vector2(x, y);
    }

    void UpdateWalkAnimation()
    {
        if (playerProfile == null || pSprite == null || ani == null)
            return;

        if (inventory != null && inventory.currentUI != UIType.None)
            return;

        if ((movement.x != 0f || movement.y != 0f) && playerProfile.currentState != PlayerSituation.Attack)
        {
            if (movement.x > 0f)
                pSprite.localScale = new Vector3(1f, 1f, 1f);
            else if (movement.x < 0f)
                pSprite.localScale = new Vector3(-1f, 1f, 1f);

            ani.SetBool("isWalk", true);
        }
        else if (movement.sqrMagnitude < 0.01f && playerProfile.currentState != PlayerSituation.Attack)
        {
            ani.SetBool("isWalk", false);
        }
    }

    public bool HitWall()
    {
        Debug.DrawRay(transform.position, new Vector3(movement.x * rayDistance, 0f, movement.y * rayDistance), Color.red);

        if (Physics.Raycast(transform.position, new Vector3(movement.x, 0f, movement.y), out hit, rayDistance))
        {
            if (hit.transform.CompareTag("Wall"))
                return true;
        }

        return false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!GameplayInputUtility.IsGameplayInputAllowed(inventory))
        {
            movement = Vector2.zero;
            return;
        }

        movement = context.ReadValue<Vector2>();
        if (movement.sqrMagnitude < 0.01f)
            movement = ReadKeyboardWasd();
    }
}
