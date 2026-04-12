using System.IO;
using UnityEngine;
public interface ISaveableWorldEntity
{
    WorldEntitySaveData Save();
    void Load(WorldEntitySaveData data);
}

[System.Serializable]
public class WorldEntitySaveData
{
    public string type;
    public int id;
    public Vector3 position;

    public string jsonData;
}
public static class VoxelSaveLoadManager
{
    public static void Save(string path, World world)
    {
        VoxelSaveFile save = new();

        foreach (var pair in world.Chunks)
        {
            save.chunks.Add(new ChunkSaveData
            {
                coord = pair.Key,
                voxels = pair.Value.ChunkData.SerializeVoxels()
            });
        }

        var entities = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var e in entities)
        {
            if (e is ISaveableWorldEntity saveable)
            {
                save.entities.Add(saveable.Save());
            }
        }

        File.WriteAllText(path, JsonUtility.ToJson(save, true));
    }

    public static void Load(string path, World world, int skipId = -1)
    {
        string json = File.ReadAllText(path);
        VoxelSaveFile save = JsonUtility.FromJson<VoxelSaveFile>(json);

        world.Clear();

        foreach (var c in save.chunks)
        {
            ChunkRenderer r = world.GetOrCreateChunk(c.coord);
            r.ChunkData.DeserializeVoxels(c.voxels);
            r.RebuildMesh();
        }

        // Load entities
        foreach (var entityData in save.entities)
        {
            GameObject obj = null;
            if (entityData.id == skipId)
                continue; // defeated enemy. do not restore
            switch (entityData.type)
            {
                case "Enemy":
                    obj = Object.Instantiate(world.enemyPrefab);
                    break;
                case "Player":
                    obj = Object.Instantiate(world.playerPrefab);
                    break;
            }

            if (obj == null)
                continue;

            obj.GetComponent<ISaveableWorldEntity>()!.Load(entityData);
        }
    }
}
