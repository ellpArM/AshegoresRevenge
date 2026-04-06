using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    private Chunk chunk;
    public Chunk ChunkData => chunk;
    private World world;

    public void Initialize(Chunk chunk, World world)
    {
        this.chunk = chunk;
        this.world = world;
        RebuildMesh();
    }

    public void RebuildMesh()
    {
        Mesh mesh = ChunkMeshBuilder.BuildMesh(
            chunk,
            world.VoxelDatabase,
            out List<VoxelAtlas> atlases
        );

        var materials = new Material[atlases.Count];
        for (int i = 0; i < atlases.Count; i++)
            materials[i] = world.GetMaterialForAtlas(atlases[i]);

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials = materials;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
