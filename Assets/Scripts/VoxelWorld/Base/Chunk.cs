using System;
using UnityEngine;

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
    public int[] SerializeVoxels()
    {
        int size = Size;
        int[] data = new int[size * size * size];

        int i = 0;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    data[i++] = voxels[x, y, z].VoxelId;

        return data;
    }
    public void DeserializeVoxels(int[] data)
    {
        int size = Size;

        if (data.Length != size * size * size)
        {
            Debug.LogError("Voxel data size mismatch during deserialization.");
            return;
        }

        int i = 0;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    voxels[x, y, z] = new Voxel { VoxelId = data[i++] };
    }
}
