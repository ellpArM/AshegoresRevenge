using UnityEngine;
using UnityEngine.EventSystems;

public class EntitiesEditorController : MonoBehaviour
{
    [Header("Dependencies")]
    public Camera playerCamera;
    public EntitiesEditorDatabase database;
    public LayerMask placementMask;

    [Header("Settings")]
    public float maxDistance = 100f;

    private EntityDefinition selectedEntity;

    public void SetSelectedEntity(int index)
    {
        selectedEntity = database.GetByIndex(index);
    }

    private void Update()
    {
        // Prevent clicking through UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftControl))
                TryRemoveEntity();
            else
                TryPlaceEntity();
        }
    }

    private void TryPlaceEntity()
    {
        if (selectedEntity == null) return;

        if (!Physics.Raycast(playerCamera.transform.position,
                             playerCamera.transform.forward,
                             out RaycastHit hit,
                             maxDistance,
                             placementMask))
            return;

        // IMPORTANT: shift inside the hit voxel
        Vector3 adjusted = hit.point - hit.normal * 0.01f;

        // Now this is reliable for all faces
        Vector3Int voxel = Vector3Int.FloorToInt(adjusted);

        // Move to adjacent voxel (placement on surface)
        Vector3Int offset = Vector3Int.RoundToInt(hit.normal);
        Vector3Int targetVoxel = voxel + offset;

        // Center of voxel
        Vector3 position = targetVoxel + Vector3.one * 0.5f;

        Quaternion rotation = Quaternion.identity;

        if (selectedEntity.alignToSurfaceNormal)
            rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        GameObject obj = Instantiate(selectedEntity.prefab, position, rotation);

        var placed = obj.GetComponent<PlacedEntity>();
        if (placed == null)
            placed = obj.AddComponent<PlacedEntity>();

        placed.Initialize(selectedEntity);
    }

    private void TryRemoveEntity()
    {
        if (!Physics.Raycast(playerCamera.transform.position,
                             playerCamera.transform.forward,
                             out RaycastHit hit,
                             maxDistance))
            return;

        var entity = hit.collider.GetComponentInParent<PlacedEntity>();

        if (entity != null)
        {
            Destroy(entity.gameObject);
        }
    }
}