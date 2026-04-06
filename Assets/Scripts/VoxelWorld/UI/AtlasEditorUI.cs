using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AtlasEditorUI : MonoBehaviour
{
    public World world;

    public RawImage atlasPreview;
    public TMP_InputField gridSizeInput;

    private Texture2D loadedTexture;

    public void LoadTexture(Texture2D texture)
    {
        loadedTexture = texture;
        atlasPreview.texture = texture;
    }

    public void ApplyAtlas()
    {
        if (loadedTexture == null) return;

        int gridSize = int.Parse(gridSizeInput.text);

        world.SetAtlas(loadedTexture, gridSize);
    }
}
