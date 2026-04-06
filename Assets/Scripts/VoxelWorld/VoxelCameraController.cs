using UnityEngine;

public class VoxelCameraController : MonoBehaviour
{
    public Transform target;

    [Header("Distance")]
    public float distance = 10f;
    public float height = 6f;
    public float angle = 35f;

    [Header("Rotation")]
    public float rotationSpeed = 90f;
    public float smoothSpeed = 8f;

    [Header("Mouse Edge Rotation")]
    public float edgeSize = 30f;

    private float currentYaw = 45f;

    void LateUpdate()
    {
        if (target == null)
            return;

        HandleRotation();
        UpdatePosition();
    }

    void HandleRotation()
    {
        float rotateInput = 0f;

        if (Input.GetKey(KeyCode.A))
            rotateInput -= 1f;

        if (Input.GetKey(KeyCode.D))
            rotateInput += 1f;

        if (Input.mousePosition.x <= edgeSize)
            rotateInput -= 1f;

        if (Input.mousePosition.x >= Screen.width - edgeSize)
            rotateInput += 1f;

        currentYaw += rotateInput * rotationSpeed * Time.deltaTime;
    }

    void UpdatePosition()
    {
        Quaternion rotation =
            Quaternion.Euler(angle, currentYaw, 0);

        Vector3 offset =
            rotation * new Vector3(0, 0, -distance);

        Vector3 desiredPosition =
            target.position + Vector3.up * height + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1f);
    }
}