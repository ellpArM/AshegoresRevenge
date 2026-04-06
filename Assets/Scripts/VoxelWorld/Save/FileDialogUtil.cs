using UnityEngine;
using System.IO;

public static class FileDialogUtil
{
    public static string SaveFile(string defaultName)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.SaveFilePanel(
            "Save Voxel World",
            "",
            defaultName,
            "json"
        );
#else
        return Path.Combine(Application.persistentDataPath, defaultName + ".json");
#endif
    }

    public static string OpenFile()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.OpenFilePanel(
            "Load Voxel World",
            "",
            "json"
        );
#else
        return "";
#endif
    }
}
