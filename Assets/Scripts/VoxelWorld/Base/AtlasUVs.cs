using UnityEngine;

public static class AtlasUVs
{
    public static Vector2[] GetUVs(int tileIndex, int atlasSize)
    {
        float tileSize = 1f / atlasSize;

        int y = tileIndex / atlasSize;
        int x = tileIndex % atlasSize;

        float u = x * tileSize;
        float v = y * tileSize;

        return new[]
        {
            new Vector2(u, v),
            new Vector2(u + tileSize, v),
            new Vector2(u + tileSize, v + tileSize),
            new Vector2(u, v + tileSize)
        };
    }
}
