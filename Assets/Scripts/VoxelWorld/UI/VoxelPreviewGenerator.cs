using UnityEngine;

public static class VoxelPreviewGenerator
{
    public static Texture2D Generate(
        VoxelAtlas atlas,
        Vector2Int[] faces,
        int previewSize = 256)
    {
        Texture2D atlasTex = atlas.Texture;
        int tilesX = atlas.gridSize;
        int tilesY = atlas.gridSize;

        Texture2D output = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
        Clear(output);

        int tileWidth = atlasTex.width / tilesX;
        int tileHeight = atlasTex.height / tilesY;

        Rect topRect = GetTileRect(faces[0], tileWidth, tileHeight, tilesY);
        Rect leftRect = GetTileRect(faces[2], tileWidth, tileHeight, tilesY);
        Rect rightRect = GetTileRect(faces[3], tileWidth, tileHeight, tilesY);

        int half = previewSize / 2;
        int quarter = previewSize / 4;

        // Draw top diamond
        DrawDiamond(output, atlasTex, topRect, new Vector2(half, half + quarter), quarter);

        // Draw left face
        DrawParallelogramLeft(output, atlasTex, leftRect, new Vector2(half - quarter, half), quarter);

        // Draw right face
        DrawParallelogramRight(output, atlasTex, rightRect, new Vector2(half, half), quarter);

        output.Apply();
        return output;
    }

    static Rect GetTileRect(Vector2Int tile, int tileWidth, int tileHeight, int tilesY)
    {
        // invert Y because textures start bottom-left
        //int invertedY = tilesY - 1 - tile.y;

        return new Rect(
            tile.x * tileWidth,
            tile.y * tileHeight,
            tileWidth,
            tileHeight
        );
    }

    static void Clear(Texture2D tex)
    {
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, clear);
    }

    // =========================
    //   FACE RENDER METHODS
    // =========================

    static void DrawDiamond(Texture2D target, Texture2D atlas, Rect srcRect, Vector2 center, int size)
    {
        Vector2 top = new Vector2(center.x, center.y + size);
        Vector2 right = new Vector2(center.x + size, center.y);
        Vector2 bottom = new Vector2(center.x, center.y - size);
        Vector2 left = new Vector2(center.x - size, center.y);

        DrawQuad(target, atlas, srcRect, top, right, bottom, left);
    }

    static void DrawParallelogramLeft(Texture2D target, Texture2D atlas, Rect srcRect, Vector2 topCorner, int size)
    {
        Vector2 topLeft = topCorner;
        Vector2 topRight = topCorner + new Vector2(0, -size);
        Vector2 bottomRight = topCorner + new Vector2(size, -2 * size);
        Vector2 bottomLeft = topCorner + new Vector2(size, -size);

        DrawQuad(target, atlas, srcRect, topLeft, topRight, bottomRight, bottomLeft);
    }

    static void DrawParallelogramRight(Texture2D target, Texture2D atlas, Rect srcRect, Vector2 topCorner, int size)
    {
        Vector2 topLeft = topCorner;
        Vector2 topRight = topCorner + new Vector2(size, -size);
        Vector2 bottomRight = topCorner + new Vector2(size, -2 * size);
        Vector2 bottomLeft = topCorner + new Vector2(0, -size);

        DrawQuad(target, atlas, srcRect, topLeft, topRight, bottomRight, bottomLeft);
    }

    static Color Sample(Texture2D atlas, Rect rect, float u, float v)
    {
        int px = Mathf.Clamp(
            Mathf.RoundToInt(rect.x + u * rect.width),
            0,
            atlas.width - 1);

        int py = Mathf.Clamp(
            Mathf.RoundToInt(rect.y + v * rect.height),
            0,
            atlas.height - 1);

        return atlas.GetPixel(px, py);
    }
    static void DrawQuad(Texture2D target, Texture2D atlas, Rect srcRect, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        int minX = Mathf.RoundToInt(Mathf.Min(v0.x, v1.x, v2.x, v3.x));
        int maxX = Mathf.RoundToInt(Mathf.Max(v0.x, v1.x, v2.x, v3.x));
        int minY = Mathf.RoundToInt(Mathf.Min(v0.y, v1.y, v2.y, v3.y));
        int maxY = Mathf.RoundToInt(Mathf.Max(v0.y, v1.y, v2.y, v3.y));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x, y);

                if (PointInQuad(p, v0, v1, v2, v3, out float u, out float v))
                {
                    Color c = Sample(atlas, srcRect, u, v);
                    target.SetPixel(x, y, c);
                }
            }
        }
    }
    static bool PointInQuad(Vector2 p, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, out float u, out float v)
    {
        u = v = 0;

        Vector2 a = v0;
        Vector2 b = v1;
        Vector2 c = v2;
        Vector2 d = v3;

        // Convert quad into two triangles and test
        if (PointInTriangle(p, a, d, c, out u, out v))
            return true;

        if (PointInTriangle(p, b, c, a, out u, out v)) // good
            return true;

        return false;
    }
    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c, out float u, out float v)
    {
        u = v = 0;

        Vector2 v0 = b - a;
        Vector2 v1 = c - a;
        Vector2 v2 = p - a;

        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;
        if (denom == 0) return false;

        float invDenom = 1 / denom;
        float alpha = (d11 * d20 - d01 * d21) * invDenom;
        float beta = (d00 * d21 - d01 * d20) * invDenom;

        if (alpha >= 0 && beta >= 0 && (alpha + beta) <= 1)
        {
            u = alpha;
            v = beta;
            return true;
        }

        return false;
    }

}
