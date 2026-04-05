using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input from keyboard
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down

        // Create movement vector
        movement = new Vector2(horizontal, vertical);
    }

    void FixedUpdate()
    {
        // Move the player using physics
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}