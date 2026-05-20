using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    private Vector2 movement;
    private float moveSpeed = 5.5f;

    private RaycastHit hit;
    private float rayDistance = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        float xOffset = movement.x * moveSpeed * Time.deltaTime;
        float yOffset = movement.y * moveSpeed * Time.deltaTime;

        if (!HitWall())
        {
            transform.localPosition += new Vector3(xOffset, 0f, yOffset);
        }
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
    }
}
