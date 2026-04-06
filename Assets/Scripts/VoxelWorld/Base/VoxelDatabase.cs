using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/Voxel Database")]
public class VoxelDatabase : ScriptableObject
{
    public VoxelDefinition airVoxel;
    public List<VoxelAtlas> Atlases = new();
    public List<VoxelDefinition> voxelDefinitions = new();

    private Dictionary<VoxelDefinition, int> voxelToId;
    private Dictionary<int, VoxelDefinition> idToVoxel;

    public void Initialize()
    {
        voxelToId = new Dictionary<VoxelDefinition, int>();
        idToVoxel = new Dictionary<int, VoxelDefinition>();

        RebuildIndex();
        foreach (var atlas in Atlases)
        {
            atlas.Initialize();
        }
    }
    public void RebuildIndex()
    {
        voxelToId.Clear();
        idToVoxel.Clear();

        int id = 0;

        voxelToId[airVoxel] = id;
        idToVoxel[id] = airVoxel;
        id++;

        foreach (var voxel in voxelDefinitions)
        {
            voxelToId[voxel] = id;
            idToVoxel[id] = voxel;
            id++;
        }

    }
    public void AddVoxel(VoxelDefinition voxel)
    {
        voxelDefinitions.Add(voxel);
        Rebuild();
    }
    public void Rebuild()
    {
        voxelToId.Clear();
        for (int i = 0; i < voxelDefinitions.Count; i++)
            voxelToId[voxelDefinitions[i]] = i;
    }
    public void Clear()
    {
        Atlases.Clear();
        voxelDefinitions.Clear();
    }
    public void CreateAtlasFromSave(AtlasSaveData data)
    {
        VoxelAtlas atlas = ScriptableObject.CreateInstance<VoxelAtlas>();

        atlas.id = data.id;
        atlas.atlasName = data.name;
        atlas.texturePath = data.texturePath;
        atlas.gridSize = data.gridSize;
        //atlas.tilesX = data.tilesX;
        //atlas.tilesY = data.tilesY;

        atlas.Initialize();

        Atlases.Add(atlas);
    }
    public void CreateVoxelFromSave(VoxelDefSaveData data)
    {
        VoxelDefinition def = new VoxelDefinition();

        //def.id = data.id;
        def.name = data.name;
        def.isSolid = data.isSolid;
        def.faceTiles = data.faceTiles;

        def.atlas = Atlases.Find(a => a.id == data.atlasId);

        if (def.atlas == null)
            Debug.LogError($"Voxel atlas {data.atlasId} not found for voxel {def.name}");

        voxelDefinitions.Add(def);
    }

    public int GetId(VoxelDefinition voxel)
        => voxelToId[voxel];

    public VoxelDefinition GetVoxel(int id)
        => idToVoxel[id];
}
