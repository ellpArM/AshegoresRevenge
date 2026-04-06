using System.IO;
using UnityEngine;

public static class VoxelSaveLoadManager
{
    public static void Save(string path, World world, VoxelDatabase database)
    {
        VoxelSaveFile save = new();

        // Atlases
        foreach (var atlas in database.Atlases)
        {
            save.atlases.Add(new AtlasSaveData
            {
                id = 1,//atlas.id,
                name = atlas.name,
                texturePath = atlas.texturePath,
                gridSize = atlas.gridSize,
                tilesX = atlas.gridSize,
                tilesY = atlas.gridSize
            });
        }

        // Voxels
        foreach (var v in database.voxelDefinitions)
        {
            save.voxels.Add(new VoxelDefSaveData
            {
                id = 1,// v.id,
                name = v.name,
                isSolid = v.isSolid,
                atlasId = 1,//v.atlas.id,
                faceTiles = v.faceTiles,
                //south = v.GetFaceUV(VoxelFace.South),
                //east = v.GetFaceUV(VoxelFace.East),
                //west = v.GetFaceUV(VoxelFace.West),
                //top = v.GetFaceUV(VoxelFace.Top),
                //bottom = v.GetFaceUV(VoxelFace.Bottom)
            });
        }

        // Chunks
        foreach (var pair in world.Chunks)
        {
            save.chunks.Add(new ChunkSaveData
            {
                coord = pair.Key,
                voxels = pair.Value.ChunkData.SerializeVoxels()
            });
        }

        File.WriteAllText(path, JsonUtility.ToJson(save, true));
    }

    public static void Load(string path, World world, VoxelDatabase database)
    {
        string json = File.ReadAllText(path);
        VoxelSaveFile save = JsonUtility.FromJson<VoxelSaveFile>(json);

        world.Clear();

        database.Clear();

        // Atlases
        foreach (var a in save.atlases)
            database.CreateAtlasFromSave(a);

        // Voxels
        foreach (var v in save.voxels)
            database.CreateVoxelFromSave(v);

        database.RebuildIndex();
        // Chunks
        foreach (var c in save.chunks)
        {
            ChunkRenderer r = world.GetOrCreateChunk(c.coord);
            r.ChunkData.DeserializeVoxels(c.voxels);
            r.RebuildMesh();
        }
    }
}
