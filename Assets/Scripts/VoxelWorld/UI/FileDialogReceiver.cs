using System.IO;
using UnityEngine;

public class FileDialogReceiver : MonoBehaviour
{
    public void OnFileLoaded(string json)
    {
        Debug.Log("Loaded JSON: " + json);

        VoxelSaveLoadManager.LoadJson(json, World.instance);
        VoxelPaletteUI.instance.RefreshUI();
    }
}