using UnityEngine;
using System.Collections.Generic;

public class PropWorldEntity : MonoBehaviour, ISaveableWorldEntity
{
    [Header("Blocking Settings")]
    public bool blocksPath = true;

    [Tooltip("Offsets in voxel space relative to this object")]
    public List<Vector3Int> occupiedOffsets = new();

    private List<Vector3Int> occupiedTiles = new();
    private World world;

    private void Start()
    {
        Register();
    }

    public void Register()
    {
        if (!blocksPath)
            return;

        world = World.instance;

        if (world == null)
        {
            Debug.LogError("PropWorldEntity: World not found.");
            return;
        }

        CalculateOccupiedTiles();
        ApplyToWorld();
    }

    private void CalculateOccupiedTiles()
    {
        occupiedTiles.Clear();

        Vector3 basePosition = transform.position;

        // Align to voxel grid (center-based like your system)
        Vector3Int baseVoxel = Vector3Int.FloorToInt(basePosition);

        foreach (var offset in occupiedOffsets)
        {
            // Apply rotation (important if props can rotate)
            Vector3 rotated = transform.rotation * offset;
            Vector3Int rotatedOffset = Vector3Int.RoundToInt(rotated);

            Vector3Int tile = baseVoxel + rotatedOffset;
            occupiedTiles.Add(tile);
        }
    }

    private void ApplyToWorld()
    {
        foreach (var tile in occupiedTiles)
        {
            world.OccupiedTiles.Add(tile);
        }
    }

    private void OnDestroy()
    {
        if (world == null)
            return;

        foreach (var tile in occupiedTiles)
        {
            world.OccupiedTiles.Remove(tile);
        }
    }

    public WorldEntitySaveData Save()
    {
        return new WorldEntitySaveData
        {
            type = name.Replace("(Clone)", "").Trim(),
            position = transform.position,
            jsonData = JsonUtility.ToJson(this)
        };
    }

    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;

        JsonUtility.FromJsonOverwrite(data.jsonData, this);
    }
}