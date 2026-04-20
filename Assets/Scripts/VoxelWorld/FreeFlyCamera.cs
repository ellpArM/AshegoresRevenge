using UnityEngine;

public class FreeFlyCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.5f;
    public float maxLookAngle = 85f;

    [Header("Controls")]
    public KeyCode toggleCursorKey = KeyCode.E;
    public KeyCode toggleMovementModeKey = KeyCode.Q;

    private float yaw;
    private float pitch;

    private bool cursorLocked = true;

    private bool horizontalMovementOnly = true;

    void Start()
    {
        SetCursorState(true);
        LookAtPosition(Vector3.zero);
    }

    void Update()
    {
        HandleCursorToggle();
        HandleMovementModeToggle();
        HandleMouseLook();
        HandleMovement();
    }
    void LookAtPosition(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        pitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(toggleCursorKey))
        {
            cursorLocked = !cursorLocked;
            SetCursorState(cursorLocked);
        }
    }

    void HandleMovementModeToggle()
    {
        if (Input.GetKeyDown(toggleMovementModeKey))
        {
            //horizontalMovementOnly = !horizontalMovementOnly;
        }
    }

    void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void HandleMouseLook()
    {
        if (!cursorLocked)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        Vector3 move;

        if (horizontalMovementOnly)
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            move = (forward * vertical + right * horizontal);
        }
        else
        {
            move = transform.TransformDirection(new Vector3(horizontal, 0f, vertical));
        }

        if (Input.GetKey(KeyCode.Space))
            move += Vector3.up;

        if (Input.GetKey(KeyCode.LeftShift))
            move += Vector3.down;

        transform.position += move * moveSpeed * Time.deltaTime;
    }
}