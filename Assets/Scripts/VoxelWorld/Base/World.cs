using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public interface IMapGenerator
{
    void GenerateMap();
}
public class World : MonoBehaviour
{
    public static World instance;
    private Dictionary<Vector3Int, ChunkRenderer> chunks = new();
    public Dictionary<Vector3Int, ChunkRenderer> Chunks => chunks;
    public VoxelDatabase voxelDatabase;
    public VoxelDatabase VoxelDatabase => voxelDatabase;
    public Material chunkMaterial;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;

    public Material baseVoxelMaterial;
    private Dictionary<VoxelAtlas, Material> materialCache = new();

    private VoxelAtlas atlas;
    private VoxelDefinition activeVoxel;
    private HashSet<Vector3Int> dirtyChunks = new();
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        voxelDatabase.Initialize();

        atlas = new VoxelAtlas();
        //atlas.Build(voxelDatabase);

        chunkMaterial.mainTexture = atlas.Texture;

        //CreateChunk(Vector3Int.zero);
        activeVoxel = voxelDatabase.GetVoxel(1);

        IMapGenerator mg = GetComponent<IMapGenerator>();
        if (mg != null)
            mg?.GenerateMap();
        else
            GenerateDefaultPlane();
    }
    void GenerateDefaultPlane()
    {
        for (int i = -8; i < 8; i++) 
            for (int j = -8; j < 8; j++)
                SetVoxel(new Vector3Int(i, 0, j), 2);
    }

    void CreateChunk(Vector3Int coord)
    {
        Chunk chunk = new Chunk(coord, 0);

        GameObject go = new GameObject($"Chunk {coord}");
        go.transform.position = coord * Chunk.Size;

        ChunkRenderer renderer = go.AddComponent<ChunkRenderer>();
        renderer.Initialize(chunk, this);

        chunks[coord] = renderer;
    }
    public void SetVoxel(Vector3Int worldVoxelPos, int voxelId)
    {
        BeginVoxelBatch();
        SetVoxelInternal(worldVoxelPos, voxelId);
        EndVoxelBatch();
    }
    void SetVoxelInternal(Vector3Int worldVoxelPos, int voxelId)
    {
        Vector3Int chunkCoord = WorldToChunkCoord(worldVoxelPos);
        Vector3Int localPos = WorldToLocalVoxel(worldVoxelPos);

        ChunkRenderer renderer = GetOrCreateChunk(chunkCoord);

        renderer.ChunkData.SetVoxel(
            localPos.x,
            localPos.y,
            localPos.z,
            voxelId
        );

        dirtyChunks.Add(chunkCoord);

        // Neighbor chunks may also need rebuild
        if (localPos.x == 0) dirtyChunks.Add(chunkCoord + Vector3Int.left);
        if (localPos.x == Chunk.Size - 1) dirtyChunks.Add(chunkCoord + Vector3Int.right);
        if (localPos.y == 0) dirtyChunks.Add(chunkCoord + Vector3Int.down);
        if (localPos.y == Chunk.Size - 1) dirtyChunks.Add(chunkCoord + Vector3Int.up);
        if (localPos.z == 0) dirtyChunks.Add(chunkCoord + Vector3Int.back);
        if (localPos.z == Chunk.Size - 1) dirtyChunks.Add(chunkCoord + Vector3Int.forward);
    }
    public void BeginVoxelBatch()
    {
        dirtyChunks.Clear();
    }
    public void SetVoxelBatched(Vector3Int worldVoxelPos, int voxelId)
    {
        SetVoxelInternal(worldVoxelPos, voxelId);
    }
    public void EndVoxelBatch()
    {
        foreach (var coord in dirtyChunks)
        {
            ChunkRenderer chunk =  GetChunk(coord);
            if (chunk)
                chunk.RebuildMesh();
        }

        dirtyChunks.Clear();
    }
    public void SetVoxel(ChunkRenderer sourceRenderer, Vector3Int localPos, int type)
    {
        Vector3Int worldVoxel =
            sourceRenderer.ChunkData.Coord * Chunk.Size + localPos;

        Vector3Int targetChunkCoord = WorldToChunkCoord(worldVoxel);
        Vector3Int targetLocalPos = WorldToLocalVoxel(worldVoxel);

        ChunkRenderer targetRenderer =
            GetOrCreateChunk(targetChunkCoord);

        targetRenderer.ChunkData.SetVoxel(
            targetLocalPos.x,
            targetLocalPos.y,
            targetLocalPos.z,
            type
        );

        targetRenderer.RebuildMesh();

        RebuildNeighborsIfNeeded(
            targetChunkCoord,
            targetLocalPos
        );
    }

    internal Voxel GetVoxel(Vector3Int worldPos)
    {
        Vector3Int chunkCoord = WorldToChunkCoord(worldPos);
        Vector3Int local = WorldToLocalVoxel(worldPos);
        return GetOrCreateChunk(chunkCoord).ChunkData.GetVoxel(local);
    }

    public void SetVoxelSilent(Vector3Int worldPos, int voxelId)
    {
        Vector3Int chunkCoord = WorldToChunkCoord(worldPos);
        Vector3Int local = WorldToLocalVoxel(worldPos);

        var renderer = GetOrCreateChunk(chunkCoord);
        renderer.ChunkData.SetVoxel(local.x, local.y, local.z, voxelId);
        dirtyChunks.Add(chunkCoord);
        //MarkChunkDirty(chunkCoord, local);
    }
    public void ApplyAction(VoxelAction action, bool undo)
    {
        BeginVoxelBatch();

        foreach (var c in action.Changes)
        {
            SetVoxelSilent(
                c.worldPos,
                undo ? c.beforeId : c.afterId
            );
        }

        EndVoxelBatch(); // rebuild dirty chunks once
    }
    public ChunkRenderer GetOrCreateChunk(Vector3Int coord)
    {
        if (chunks.TryGetValue(coord, out ChunkRenderer existing))
            return existing;

        Chunk chunk = new Chunk(coord, 0);

        GameObject go = new GameObject($"Chunk {coord}");
        go.transform.position = coord * Chunk.Size;
        go.layer = LayerMask.NameToLayer("Terrain");

        ChunkRenderer renderer = go.AddComponent<ChunkRenderer>();
        renderer.Initialize(chunk, this);

        chunks.Add(coord, renderer);
        return renderer;
    }
    ChunkRenderer GetChunk(Vector3Int coord)
    {
        if (chunks.TryGetValue(coord, out ChunkRenderer existing))
            return existing;
        return null;
    }
    void RebuildNeighborsIfNeeded(Vector3Int chunkCoord, Vector3Int localPos)
    {
        if (localPos.x == 0) TryRebuild(chunkCoord + Vector3Int.left);
        if (localPos.x == Chunk.Size - 1) TryRebuild(chunkCoord + Vector3Int.right);
        if (localPos.y == 0) TryRebuild(chunkCoord + Vector3Int.down);
        if (localPos.y == Chunk.Size - 1) TryRebuild(chunkCoord + Vector3Int.up);
        if (localPos.z == 0) TryRebuild(chunkCoord + Vector3Int.back);
        if (localPos.z == Chunk.Size - 1) TryRebuild(chunkCoord + Vector3Int.forward);
    }

    void TryRebuild(Vector3Int coord)
    {
        if (chunks.TryGetValue(coord, out ChunkRenderer r))
            r.RebuildMesh();
    }
    Vector3Int WorldToChunkCoord(Vector3Int worldVoxel)
    {
        return new Vector3Int(
            Mathf.FloorToInt((float)worldVoxel.x / Chunk.Size),
            Mathf.FloorToInt((float)worldVoxel.y / Chunk.Size),
            Mathf.FloorToInt((float)worldVoxel.z / Chunk.Size)
        );
    }
    Vector3Int WorldToLocalVoxel(Vector3Int worldVoxel)
    {
        int Mod(int a, int m) => (a % m + m) % m;

        return new Vector3Int(
            Mod(worldVoxel.x, Chunk.Size),
            Mod(worldVoxel.y, Chunk.Size),
            Mod(worldVoxel.z, Chunk.Size)
        );
    }
    public int GetVoxelId(Vector3Int pos)
    {
        ChunkRenderer chunk = GetOrCreateChunk(WorldToChunkCoord(pos));
        Vector3Int localPos = WorldToLocalVoxel(pos);
        int id = chunk.ChunkData.GetVoxel(localPos.x, localPos.y, localPos.z).VoxelId;
        return id;
    }
    public void SetAtlas(Texture2D texture, int gridSize)
    {
        //atlas = new VoxelAtlas();
        //atlas.Initialize(texture, gridSize);

        //chunkMaterial.mainTexture = atlas.AtlasTexture;

        //foreach (var chunk in chunks.Values)
        //    chunk.RebuildMesh();
    }
    public void RegisterVoxel(VoxelDefinition voxel)
    {
        voxelDatabase.voxelDefinitions.Add(voxel);
        voxelDatabase.Initialize();

        foreach (var chunk in chunks.Values)
            chunk.RebuildMesh();
    }

    public void SetActiveVoxel(VoxelDefinition voxel)
    {
        activeVoxel = voxel;
    }

    public int GetActiveVoxelId()
    {
        if (activeVoxel != null)
            return voxelDatabase.GetId(activeVoxel);
        else
            return 0;
    }
    public Material GetMaterialForAtlas(VoxelAtlas atlas)
    {
        if (!materialCache.TryGetValue(atlas, out var mat))
        {
            //atlas.Initialize();
            mat = new Material(baseVoxelMaterial);
            mat.name = atlas.atlasName;
            mat.mainTexture = atlas.Texture;
            ApplyTransparencySettings(mat, atlas.transparency);
            materialCache[atlas] = mat;
        }
        return mat;
    }
    public void Clear()
    {
        foreach (var pair in chunks)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        chunks.Clear();
    }
    private void ApplyTransparencySettings(Material mat, float transparency)
    {
        transparency = Mathf.Clamp01(transparency);

        Color baseColor = mat.color;
        baseColor.a = transparency;
        mat.color = baseColor;

        if (transparency < 1f)
        {
            // Try URP first
            if (mat.HasProperty("_Surface"))
            {
                // URP Lit shader
                mat.SetFloat("_Surface", 1); // 0=Opaque, 1=Transparent
                mat.SetFloat("_Blend", 0);   // Alpha blending
                mat.SetFloat("_AlphaClip", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                // Built-in Standard fallback
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }
        else
        {
            // Opaque
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 0);
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                mat.SetFloat("_Mode", 0);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHABLEND_ON");
            }

            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }
    }
}
