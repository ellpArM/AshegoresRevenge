using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VoxelButtonUI : MonoBehaviour
{
    public RawImage previewImage;
    public GameObject selectionFrame;

    public void Setup(VoxelDefinition voxel, int hotkey, System.Action<VoxelDefinition> onClick)
    {
        previewImage.texture = null;
        VoxelPreviewManager.instance.RequestPreview(voxel, (tex) =>
        {
            previewImage.texture = tex;
        });

    }
    public void SetSelected(bool selected)
    {
        if (selectionFrame != null)
            selectionFrame.SetActive(selected);
    }
}