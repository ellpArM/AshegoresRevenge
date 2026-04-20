using System.IO;
using UnityEngine;

public class ArenaLoader : MonoBehaviour, IMapGenerator
{
    [Header("Arena Size")]
    //public int width = 20;
    //public int depth = 20;
    //public int height = 0;
    public string mapFile;

    [Header("Voxel")]
    public int groundVoxelId = 1; // stone or floor

    public void GenerateMap()
    {
        if (World.instance == null)
            return;

        World.instance.BeginVoxelBatch();

        GenerateArena();

        World.instance.EndVoxelBatch();
    }

    void GenerateArena()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("MapData/" + mapFile);

        VoxelSaveLoadManager.LoadJson(jsonFile.text, World.instance);
        //int halfW = width / 2;
        //int halfD = depth / 2;

        //for (int x = -halfW; x < halfW; x++)
        //{
        //    for (int z = -halfD; z < halfD; z++)
        //    {
        //        World.instance.SetVoxelBatched(
        //            new Vector3Int(x, height, z),
        //            groundVoxelId
        //        );
        //    }
        //}
    }
}