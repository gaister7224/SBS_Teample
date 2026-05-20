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

    [SerializeField] private Transform pSprite;
    private Animator ani;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerProfile = GetComponent<PlayerProfile>();
        ani = pSprite.gameObject.GetComponentInChildren<Animator>();
        inventory = GameObject.Find("InventorySystem").GetComponent<InventoryMain>();
    }

    //private void Update()
    //{
    //    float xOffset = movement.x * playerProfile.moveSpeed * Time.deltaTime;
    //    float yOffset = movement.y * playerProfile.moveSpeed * Time.deltaTime;

    //    if (!HitWall())
    //    {
    //        transform.localPosition += new Vector3(xOffset, 0f, yOffset);
    //    }
    //}

    private void FixedUpdate()
    {
        Vector3 moveVelocity = new Vector3(movement.x, 0f, movement.y) * playerProfile.moveSpeed;
        rb.linearVelocity = moveVelocity;
    }

    public bool HitWall()
    {
        Debug.DrawRay(transform.position, new Vector3(movement.x * rayDistance, 0f, movement.y * rayDistance), Color.red);

        if (Physics.Raycast(transform.position, new Vector3(movement.x, 0f, movement.y), out hit, rayDistance))
        {
            if (hit.transform.CompareTag("Wall"))
            {
                return true;
            }
        }

        return false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();

        if (inventory.currentUI == UIType.None)
        {
            if ((movement.x != 0 || movement.y != 0) && playerProfile.currentState != PlayerSituation.Attack)
            {
                if (movement.x > 0)
                    pSprite.localScale = new Vector3(1f, 1f, 1f);
                else
                    pSprite.localScale = new Vector3(-1f, 1f, 1f);

                ani.SetBool("isWalk", true);
            }
            else if ((movement.x == 0 && movement.y == 0) && playerProfile.currentState != PlayerSituation.Attack)
            {
                ani.SetBool("isWalk", false);
            }
        }
    }
}
