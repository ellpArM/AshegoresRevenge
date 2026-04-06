using UnityEngine;
using System.Collections.Generic;
public struct FaceData
{
    public VoxelDefinition voxel;
    public VoxelAtlas atlas;
}
public static class ChunkMeshBuilder
{
    private class SubMeshData
    {
        public List<Vector3> vertices = new();
        public List<int> triangles = new();
        public List<Vector2> uvs = new();
    }

    public static Mesh BuildMesh(
        Chunk chunk,
        VoxelDatabase database,
        out List<VoxelAtlas> usedAtlases
    )
    {
        Dictionary<VoxelAtlas, SubMeshData> submeshes = new();

        for (int x = 0; x < Chunk.Size; x++)
            for (int y = 0; y < Chunk.Size; y++)
                for (int z = 0; z < Chunk.Size; z++)
                {
                    int voxelId = chunk.GetVoxel(x, y, z).VoxelId;
                    if (voxelId == 0) continue;

                    VoxelDefinition voxel = database.GetVoxel(voxelId);
                    //voxel.atlas.Initialize();

                    for (int face = 0; face < 6; face++)
                    {
                        if (!IsFaceVisible(chunk, database, x, y, z, (VoxelFace)face))
                            continue;

                        if (!submeshes.TryGetValue(voxel.atlas, out var sm))
                        {
                            sm = new SubMeshData();
                            submeshes[voxel.atlas] = sm;
                        }

                        AddFace(sm, voxel, face, x, y, z);
                    }
                }

        Mesh mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        usedAtlases = new List<VoxelAtlas>();
        mesh.subMeshCount = submeshes.Count;

        var allVertices = new List<Vector3>();
        var allUVs = new List<Vector2>();
        var submeshTriangles = new List<List<int>>();

        int vertexOffset = 0;

        foreach (var pair in submeshes)
        {
            var sm = pair.Value;
            usedAtlases.Add(pair.Key);

            allVertices.AddRange(sm.vertices);
            allUVs.AddRange(sm.uvs);

            submeshTriangles.Add(
                Offset(sm.triangles, vertexOffset)
            );

            vertexOffset += sm.vertices.Count;
        }

        mesh.SetVertices(allVertices);
        mesh.SetUVs(0, allUVs);

        for (int i = 0; i < submeshTriangles.Count; i++)
            mesh.SetTriangles(submeshTriangles[i], i);

        mesh.RecalculateNormals();
        return mesh;
    }

    private static List<int> Offset(List<int> src, int offset)
    {
        var dst = new List<int>(src.Count);
        foreach (int i in src) dst.Add(i + offset);
        return dst;
    }

    private static void AddFace(
        SubMeshData sm,
        VoxelDefinition voxel,
        int face,
        int x, int y, int z
    )
    {
        int v = sm.vertices.Count;

        sm.vertices.AddRange(VoxelGeometry.GetFaceVertices((VoxelFace)face, x, y, z));
        sm.triangles.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
        sm.uvs.AddRange(voxel.atlas.GetFaceUVs(voxel.faceTiles[face]));
    }
    static bool IsFaceVisible(Chunk chunk, VoxelDatabase database, int x, int y, int z, VoxelFace face)
    {
        Vector3Int offset = VoxelGeometry.GetFaceOffset(face);

        // Convert local voxel position to world voxel position
        Vector3Int worldPos = chunk.Coord * Chunk.Size
                              + new Vector3Int(x, y, z);

        Vector3Int neighborWorldPos = worldPos + offset;

        Voxel current = World.instance.GetVoxel(worldPos);
        Voxel neighbor = World.instance.GetVoxel(neighborWorldPos);

        // Air always renders face
        if (neighbor.VoxelId == 0)
            return true;

        VoxelDefinition currentDef = database.GetVoxel(current.VoxelId);
        VoxelDefinition neighborDef = database.GetVoxel(neighbor.VoxelId);

        bool currentOpaque = IsOpaque(currentDef);
        bool neighborOpaque = IsOpaque(neighborDef);

        if (currentOpaque && neighborOpaque)
            return false;

        if (!currentOpaque && !neighborOpaque)
            return false;

        return true;
    }
    static bool IsOpaque(VoxelDefinition def)
    {
        if (def == null)
            return false;

        VoxelAtlas atlas = def.atlas;
        if (atlas == null)
            return false;

        return def.isSolid && atlas.transparency >= 1f;
    }

}
