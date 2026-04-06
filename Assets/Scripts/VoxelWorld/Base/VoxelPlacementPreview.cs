using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelPlacementPreview : MonoBehaviour
{
    public Material placeMaterial;
    public Material removeMaterial;

    MeshFilter filter;
    MeshRenderer renderer;

    void Awake()
    {
        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
    }

    public void Clear()
    {
        filter.mesh = null;
    }
    public bool HasMesh()
    {
        return filter.mesh.vertexCount > 0;
    }

    public void Build(
        Vector3Int origin,
        VoxelBrush brush,
        VoxelDatabase database,
        bool removing
    )
    {
        renderer.material = removing ? removeMaterial : placeMaterial;

        List<Vector3Int> voxels = new();
        foreach (var offset in brush.GetOffsets())
            voxels.Add(origin + offset);

        filter.mesh = VoxelPreviewMeshBuilder.Build(
            voxels,
            brush.voxelId,
            database
        );
    }
}
