using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VoxelRun
{
    public int id;
    public int count;
}
public class Chunk
{
    public const int Size = 32;
    public readonly Vector3Int Coord;

    private Voxel[,,] voxels;

    public Chunk(Vector3Int coord, int airId = 0)
    {
        Coord = coord;
        voxels = new Voxel[Size, Size, Size];

        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
                for (int z = 0; z < Size; z++)
                    voxels[x, y, z] = new Voxel(airId);
    }

    public Voxel GetVoxel(Vector3Int pos)
    {
        return GetVoxel(pos.x, pos.y, pos.z);
    }
    public Voxel GetVoxel(int x, int y, int z)
    {
        if (!InBounds(x, y, z))
            return default;
        return voxels[x, y, z];
    }

    public void SetVoxel(int x, int y, int z, int voxelId)
    {
        if (!InBounds(x, y, z)) return;
        voxels[x, y, z] = new Voxel(voxelId);
    }

    bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < Size &&
               y >= 0 && y < Size &&
               z >= 0 && z < Size;
    }
    public VoxelRun[] SerializeVoxels()
    {
        int size = Size;
        List<VoxelRun> runs = new List<VoxelRun>();

        int currentId = voxels[0, 0, 0].VoxelId;
        int count = 0;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    int id = voxels[x, y, z].VoxelId;

                    if (id == currentId)
                    {
                        count++;
                    }
                    else
                    {
                        runs.Add(new VoxelRun { id = currentId, count = count });
                        currentId = id;
                        count = 1;
                    }
                }
            }
        }

        // add last run
        runs.Add(new VoxelRun { id = currentId, count = count });

        return runs.ToArray();
    }
    public void DeserializeVoxels(VoxelRun[] runs)
    {
        int size = Size;

        int x = 0, y = 0, z = 0;

        foreach (var run in runs)
        {
            for (int i = 0; i < run.count; i++)
            {
                voxels[x, y, z] = new Voxel(run.id);

                // advance index
                z++;
                if (z >= size)
                {
                    z = 0;
                    y++;
                    if (y >= size)
                    {
                        y = 0;
                        x++;
                    }
                }
            }
        }
    }
}
