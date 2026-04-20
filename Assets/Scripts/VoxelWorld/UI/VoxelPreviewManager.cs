using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelPreviewManager : MonoBehaviour
{
    public static VoxelPreviewManager instance;

    [Header("Scene References")]
    public Camera previewCamera;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Material material;

    [Header("Settings")]
    public int textureSize = 128;

    private Queue<System.Action> renderQueue = new();
    private bool isRendering = false;

    void Awake()
    {
        instance = this;
    }

    public void RequestPreview(VoxelDefinition voxel, System.Action<Texture2D> callback)
    {
        renderQueue.Enqueue(() => StartCoroutine(RenderVoxel(voxel, callback)));

        if (!isRendering)
            ProcessQueue();
    }

    void ProcessQueue()
    {
        if (renderQueue.Count == 0)
        {
            isRendering = false;
            return;
        }

        isRendering = true;
        renderQueue.Dequeue().Invoke();
    }

    IEnumerator RenderVoxel(VoxelDefinition voxel, System.Action<Texture2D> callback)
    {
        // Build mesh
        BuildMesh(voxel);

        // Create temporary RenderTexture
        RenderTexture rt = new RenderTexture(textureSize, textureSize, 16);
        previewCamera.targetTexture = rt;

        previewCamera.Render();

        // Read pixels into Texture2D
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        tex.Apply();

        // Cleanup
        previewCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();

        callback?.Invoke(tex);

        yield return null;

        ProcessQueue();
    }

    void BuildMesh(VoxelDefinition voxel)
    {
        Mesh mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        AddFace(vertices, triangles, uvs, voxel, VoxelFace.Top);
        AddFace(vertices, triangles, uvs, voxel, VoxelFace.North);
        AddFace(vertices, triangles, uvs, voxel, VoxelFace.East);

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        //mat = new Material(baseVoxelMaterial);
        //mat.name = atlas.atlasName;
        //mat.mainTexture = atlas.Texture;
        meshRenderer.material = new Material(new Material(material));
        //meshRenderer.material.tex;
        meshRenderer.material.mainTexture = voxel.atlas.texture;
    }

    void AddFace(List<Vector3> verts, List<int> tris, List<Vector2> uvs, VoxelDefinition voxel, VoxelFace face)
    {
        int v = verts.Count;

        verts.AddRange(VoxelGeometry.GetFaceVertices(face, 0, 0, 0));
        tris.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
        uvs.AddRange(voxel.atlas.GetFaceUVs(voxel.faceTiles[(int)face]));
    }
}