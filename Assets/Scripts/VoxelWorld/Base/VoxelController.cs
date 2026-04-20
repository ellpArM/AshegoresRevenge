using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoxelController : MonoBehaviour
{
    public Camera cam;
    public World world;
    public float maxRayDistance = 100f;
    public int placeType = 1;

    public VoxelBrush brush;
    public Vector3Int BrushSize => brush.size;
    public VoxelPlacementPreview preview;
    public VoxelDatabase voxelDatabase;
    private VoxelUndoManager undoManager = new();
    public bool CanUndo => undoManager.CanUndo;
    public bool CanRedo => undoManager.CanRedo;
    void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrl && Input.GetKeyDown(KeyCode.Y))
            undoManager.PopRedo();

        if (ctrl && Input.GetKeyDown(KeyCode.Z))
            Undo();

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            preview.Clear();
            return;
        }

        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            preview.Clear();
            return;
        }

        if (!TryGetVoxelHit(out Vector3Int hitPos, out Vector3Int placePos))
        {
            preview.Clear();
            return;
        }

        if (EditorCameraController.IsCameraManipulating)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("pressed z again");
            undoManager.PopUndo();
        }

        bool removing = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        Vector3Int origin = removing ? hitPos : placePos;

        preview.Build(origin, brush, voxelDatabase, removing);

        if (Input.GetMouseButtonDown(0) && !removing)
        {
            PlaceBrush(placePos);
        }
        else if (Input.GetMouseButtonDown(0) && removing)
        {
            RemoveBrush(hitPos);
        }


    }
    public void Undo()
    {
        if (!undoManager.CanUndo)
            return;

        var action = undoManager.PopUndo();
        world.ApplyAction(action, undo: true);
    }

    public void Redo()
    {
        if (!undoManager.CanRedo)
            return;

        var action = undoManager.PopRedo();
        world.ApplyAction(action, undo: false);
    }

    internal void SetActiveVoxel(VoxelDefinition voxel)
    {        
        brush.voxelId = voxelDatabase.GetId(voxel);
    }
    void PlaceBrush(Vector3Int origin)
    {
        VoxelAction currentAction = new();
        world.BeginVoxelBatch();
        foreach (var offset in brush.GetOffsets())
        {
            Vector3Int pos = origin + offset;
            currentAction.AddChange(pos, world.GetVoxelId(pos), brush.voxelId);
            world.SetVoxelBatched(pos, brush.voxelId);
        }
        world.EndVoxelBatch();
        undoManager.Push(currentAction);
    }
    void RemoveBrush(Vector3Int origin)
    {
        VoxelAction currentAction = new();
        world.BeginVoxelBatch();
        foreach (var offset in brush.GetOffsets())
        {
            Vector3Int pos = origin + offset;
            currentAction.AddChange(pos, world.GetVoxelId(pos), 0);
            world.SetVoxelBatched(pos, 0);
        }
        world.EndVoxelBatch();
        undoManager.Push(currentAction);
    }
    bool TryGetVoxelHit(out Vector3Int hitVoxel, out Vector3Int placeVoxel)
    {
        hitVoxel = placeVoxel = default;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return false;

        Vector3 p = hit.point - hit.normal * 0.001f;

        hitVoxel = Vector3Int.FloorToInt(p);
        placeVoxel = hitVoxel + Vector3Int.RoundToInt(hit.normal);

        return true;
    }

    bool RaycastVoxel(out ChunkRenderer chunkRenderer, out Vector3Int voxelPos, bool placing)
    {
        chunkRenderer = null;
        voxelPos = Vector3Int.zero;

        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition),out RaycastHit hit, maxRayDistance)) return false;

        chunkRenderer = hit.collider.GetComponent<ChunkRenderer>();
        if (chunkRenderer == null) return false;

        Vector3 adjustedPoint = placing ? hit.point + hit.normal * 0.5f : hit.point - hit.normal * 0.5f;
        //Vector3 local = adjustedPoint - chunkRenderer.transform.position;
        //voxelPos = Vector3Int.FloorToInt(local);

        voxelPos = Vector3Int.FloorToInt(adjustedPoint);
        return true;
    }
    public void SetBrushSizeX(int value)
    {
        brush.size.x = Mathf.Max(1, value);
    }

    public void SetBrushSizeY(int value)
    {
        brush.size.y = Mathf.Max(1, value);
    }

    public void SetBrushSizeZ(int value)
    {
        brush.size.z = Mathf.Max(1, value);
    }
}
