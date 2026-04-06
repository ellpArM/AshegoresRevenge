using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelSaveFile
{
    public int version = 1;
    public List<AtlasSaveData> atlases = new();
    public List<VoxelDefSaveData> voxels = new();
    public List<ChunkSaveData> chunks = new();
}

[Serializable]
public class AtlasSaveData
{
    public int id;
    public string name;
    public string texturePath;
    public int gridSize;
    public int tilesX;
    public int tilesY;
}

[Serializable]
public class VoxelDefSaveData
{
    public int id;
    public string name;
    public bool isSolid;
    public int atlasId;

    public Vector2Int[] faceTiles;
    //public Vector2Int north;
    //public Vector2Int south;
    //public Vector2Int east;
    //public Vector2Int west;
    //public Vector2Int top;
    //public Vector2Int bottom;
}

[Serializable]
public class ChunkSaveData
{
    public Vector3Int coord;
    public int[] voxels;
}
