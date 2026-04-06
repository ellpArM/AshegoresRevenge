using System.Collections.Generic;
using UnityEngine;

public static class VoxelPreviewMeshBuilder
{
    static readonly Vector3Int[] FaceOffsets =
    {
        Vector3Int.forward,
        Vector3Int.back,
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    private class SubMeshData
    {
        public List<Vector3> vertices = new();
        public List<int> triangles = new();
        public List<Vector2> uvs = new();
    }
    public static Mesh Build(
        IEnumerable<Vector3Int> voxels,
        int voxelId,
        VoxelDatabase database
    )
    {
        HashSet<Vector3Int> localVoxels = new(voxels);
        SubMeshData sm = new();
        VoxelDefinition def = database.GetVoxel(voxelId);

        foreach (var pos in voxels)
        {
            for (int f = 0; f < 6; f++)
            {
                Vector3Int neighbor = pos + FaceOffsets[f];
                if (localVoxels.Contains(neighbor))
                    continue; // internal face, skip

                AddFace(sm, def, (VoxelFace)f, pos);
            }

            //for (int f = 0; f < 6; f++)
            //    AddFace(sm, def, (VoxelFace)f, pos);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(sm.vertices);
        mesh.SetUVs(0, sm.uvs);
        mesh.SetTriangles(sm.triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    static void AddFace(
        SubMeshData sm,
        VoxelDefinition def,
        VoxelFace face,
        Vector3Int pos
    )
    {
        int start = sm.vertices.Count;

        foreach (var v in VoxelGeometry.GetFaceVertices(face, pos.x, pos.y, pos.z))
            sm.vertices.Add(v);

        sm.uvs.AddRange(def.atlas.GetFaceUVs(def.faceTiles[(int)face]));
        //sm.uvs.AddRange(def.atlas.GetFaceUVs(def.faceTiles[face]));

        sm.triangles.Add(start + 0);
        sm.triangles.Add(start + 1);
        sm.triangles.Add(start + 2);
        sm.triangles.Add(start + 0);
        sm.triangles.Add(start + 2);
        sm.triangles.Add(start + 3);
    }
}
