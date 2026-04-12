using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelPlayerMover : MonoBehaviour, ISaveableWorldEntity
{
    public float moveSpeed = 3f;

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
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(FollowPath(path));
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