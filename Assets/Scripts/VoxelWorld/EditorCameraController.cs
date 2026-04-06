using UnityEngine;

public class EditorCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 10f;
    public float orbitSpeed = 3f;

    [Header("Limits")]
    public float minDistance = 1.5f;
    public float maxDistance = 100f;

    [Header("Focus")]
    public Vector3 focusPoint = Vector3.zero;

    public static bool IsCameraManipulating { get; private set; }

    float distance;
    float yaw;
    float pitch;
    void Start()
    {
        // Use current camera transform as the initial state
        Vector3 offset = transform.position - focusPoint;
        distance = offset.magnitude;

        Vector3 euler = transform.rotation.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        // Clamp pitch to avoid inverted startup states
        if (pitch > 180f)
            pitch -= 360f;

        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    void Update()
    {
        IsCameraManipulating = false;

        HandleKeyboardMovement();
        HandleMouseWheelZoom();
        HandleAltMouseControls();

        UpdateTransform();
    }

    // ========================= Movement =========================

    void HandleKeyboardMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h == 0 && v == 0) return;

        Vector3 right = transform.right;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        Vector3 move = (right * h + forward * v) * moveSpeed * Time.deltaTime;

        focusPoint += move;
    }

    // ========================= Zoom =========================

    void HandleMouseWheelZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        distance -= scroll * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    // ========================= Alt Controls =========================

    void HandleAltMouseControls()
    {
        if (!Input.GetKey(KeyCode.LeftAlt))
            return;

        IsCameraManipulating = true;

        // Orbit
        if (Input.GetMouseButton(0))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed;
            pitch -= Input.GetAxis("Mouse Y") * orbitSpeed;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }

        // Zoom (Alt + RMB drag)
        if (Input.GetMouseButton(1))
        {
            float delta = Input.GetAxis("Mouse Y");
            distance += delta * zoomSpeed * 0.5f * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    // ========================= Apply =========================

    void UpdateTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = focusPoint + offset;
        transform.rotation = rotation;
    }

    // ========================= Public API =========================

    public void SetFocus(Vector3 point)
    {
        focusPoint = point;
    }
}
