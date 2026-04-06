using UnityEngine;

public static class VoxelGeometry
{
    // Cube face vertex offsets (quad, clockwise)
    public static Vector3[] GetFaceVertices(
        VoxelFace face,
        int x,
        int y,
        int z
    )
    {
        Vector3 p = new Vector3(x, y, z);

        return face switch
        {
            VoxelFace.North => new[]
            {
                p + new Vector3(0, 0, 1),
                p + new Vector3(1, 0, 1),
                p + new Vector3(1, 1, 1),
                p + new Vector3(0, 1, 1),
            },

            VoxelFace.South => new[]
            {
                p + new Vector3(1, 0, 0),
                p + new Vector3(0, 0, 0),
                p + new Vector3(0, 1, 0),
                p + new Vector3(1, 1, 0),
            },

            VoxelFace.East => new[]
            {
                p + new Vector3(1, 0, 1),
                p + new Vector3(1, 0, 0),
                p + new Vector3(1, 1, 0),
                p + new Vector3(1, 1, 1),
            },

            VoxelFace.West => new[]
            {
                p + new Vector3(0, 0, 0),
                p + new Vector3(0, 0, 1),
                p + new Vector3(0, 1, 1),
                p + new Vector3(0, 1, 0),
            },

            VoxelFace.Top => new[]
            {
                p + new Vector3(0, 1, 1),
                p + new Vector3(1, 1, 1),
                p + new Vector3(1, 1, 0),
                p + new Vector3(0, 1, 0),
            },

            VoxelFace.Bottom => new[]
            {
                p + new Vector3(0, 0, 0),
                p + new Vector3(1, 0, 0),
                p + new Vector3(1, 0, 1),
                p + new Vector3(0, 0, 1),
            },

            _ => null
        };
    }

    public static Vector3Int GetFaceOffset(VoxelFace face)
    {
        return face switch
        {
            VoxelFace.North => new Vector3Int(0, 0, 1),
            VoxelFace.South => new Vector3Int(0, 0, -1),
            VoxelFace.East => new Vector3Int(1, 0, 0),
            VoxelFace.West => new Vector3Int(-1, 0, 0),
            VoxelFace.Top => new Vector3Int(0, 1, 0),
            VoxelFace.Bottom => new Vector3Int(0, -1, 0),
            _ => Vector3Int.zero
        };
    }
}
