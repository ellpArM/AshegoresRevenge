using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public class VoxelPlayerMover : MonoBehaviour, ISaveableWorldEntity
{
    public float moveSpeed = 3f;

    [Header("Path Visual")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineHeightOffset = 0.1f;

    private Coroutine moveRoutine;
    SelectionCircle selectionCircle;
    LayerMask layerMask; 
    
    private void Start()
    {
        FindAnyObjectByType<VoxelCameraController>().target = transform;
        selectionCircle = GetComponentInChildren<SelectionCircle>();
        selectionCircle.Show(SelectionState.Green);
        layerMask = LayerMask.GetMask("Terrain");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMove();
        }
    }

    void TryMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100, layerMask))
            return;

        Vector3Int clickedVoxel = Vector3Int.FloorToInt(hit.point - hit.normal * 0.1f);

        Vector3Int target = clickedVoxel + Vector3Int.up;
        Vector3Int current = new Vector3Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            Mathf.FloorToInt(transform.position.z)
        );

        List<Vector3Int> path = VoxelPathfinder.FindPath(current, target);

        if (path == null || path.Count == 0)
        {
            ClearPathLine();
            return;
        }

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(FollowPath(path));

        DrawPathLine(path);

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(FollowPath(path));
    }
    void DrawPathLine(List<Vector3Int> path)
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = new Vector3(
                path[i].x + 0.5f,
                path[i].y + lineHeightOffset,
                path[i].z + 0.5f
            );

            lineRenderer.SetPosition(i, pos);
        }

        lineRenderer.enabled = true;

        Material mat = lineRenderer.material;

        if (mat.HasProperty("_tiling"))
        {
            mat.SetVector("_tiling", new Vector2(path.Count, 1));
        }
    }
    void ClearPathLine()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }

    IEnumerator FollowPath(List<Vector3Int> path)
    {
        foreach (Vector3Int step in path)
        {
            Vector3 target = new Vector3( step.x + 0.5f, step.y, step.z + 0.5f);

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards( transform.position, target, moveSpeed * Time.deltaTime);

                yield return null;
            }
        }
        ClearPathLine();
    }
    public WorldEntitySaveData Save()
    {
        return new WorldEntitySaveData
        {
            type = "Player",
            id = 0,
            position = transform.position,
            jsonData = ""
        };
    }

    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;
    }
    public void StopMovement()
    {
        StopAllCoroutines();
    }
}