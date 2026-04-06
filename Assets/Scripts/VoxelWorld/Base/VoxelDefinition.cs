using UnityEngine;
public enum VoxelFace
{
    North,
    South,
    East,
    West,
    Top,
    Bottom
}

[CreateAssetMenu(menuName = "Voxel/Voxel Definition")]
public class VoxelDefinition : ScriptableObject
{
    public string voxelName;
    public bool isSolid = true;
    public VoxelAtlas atlas;

    [Tooltip("Atlas tile coordinates per face (North, South, East, West, Top, Bottom)")]
    public Vector2Int[] faceTiles = new Vector2Int[6];
}
