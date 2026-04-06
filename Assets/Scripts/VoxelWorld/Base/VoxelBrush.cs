using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoxelBrush
{
    public Vector3Int size = Vector3Int.one;
    public int voxelId = 1;

    // Bias even-sized brushes toward positive axes
    public Vector3Int GetPivotOffset()
    {
        return new Vector3Int(
            (size.x - 1) / 2,
            (size.y - 1) / 2,
            (size.z - 1) / 2
        );
    }

    public IEnumerable<Vector3Int> GetOffsets()
    {
        Vector3Int pivot = GetPivotOffset();

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                for (int z = 0; z < size.z; z++)
                    yield return new Vector3Int(x, y, z) - pivot;
    }
}
