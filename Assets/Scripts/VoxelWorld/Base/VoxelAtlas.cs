using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "Voxel/Voxel Atlas")]
public class VoxelAtlas : ScriptableObject
{
    public int id;
    public string atlasName;
    public string texturePath;
    public int gridSize = 16;
    [Range(0f, 1f)]
    public float transparency = 1f;

    //[System.NonSerialized]
    public Texture2D texture;

    public Texture2D Texture => texture;

    public void Initialize()
    {
        if (texture != null) return;

        //if (!File.Exists(texturePath))
        //{
        //    Debug.LogError($"Atlas texture not found: {texturePath}");
        //    return;
        //}

        //byte[] data = File.ReadAllBytes(texturePath);
        //texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        //texture.LoadImage(data);
        //texture.name = Path.GetFileName(texturePath);
        //texture.filterMode = FilterMode.Point;
        //texture.wrapMode = TextureWrapMode.Clamp;
    }

    public Vector2[] GetFaceUVs(Vector2Int tile)
    {
        float t = 1f / gridSize;
        float u = tile.x * t;
        float v = tile.y * t;

        return new[]
        {
            new Vector2(u, v),
            new Vector2(u + t, v),
            new Vector2(u + t, v + t),
            new Vector2(u, v + t)
        };
    }
}
