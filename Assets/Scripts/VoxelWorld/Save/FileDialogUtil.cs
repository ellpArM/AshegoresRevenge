using UnityEngine;

public static class FileDialogUtil
{
    public static void SaveFile(string defaultName, string data)
    {
#if UNITY_EDITOR
        var path = UnityEditor.EditorUtility.SaveFilePanel(
            "Save Voxel World", "", defaultName, "json"
        );
        if (!string.IsNullOrEmpty(path))
            System.IO.File.WriteAllText(path, data);

#elif UNITY_STANDALONE_WIN
        var path = SFB.StandaloneFileBrowser.SaveFilePanel(
            "Save Voxel World", "", defaultName, "json"
        );
        if (!string.IsNullOrEmpty(path))
            System.IO.File.WriteAllText(path, data);

#elif UNITY_WEBGL
        WebGLFileDialog.SaveFile(defaultName + ".json", data);
#endif
    }

    public static void OpenFile()
    {
#if UNITY_EDITOR
        var path = UnityEditor.EditorUtility.OpenFilePanel(
            "Load Voxel World", "", "json"
        );
        if (!string.IsNullOrEmpty(path))
        {
            var data = System.IO.File.ReadAllText(path);
            Debug.Log(data);
        }

#elif UNITY_STANDALONE_WIN
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel(
            "Load Voxel World", "", "json", false
        );
        if (paths.Length > 0)
        {
            var data = System.IO.File.ReadAllText(paths[0]);
            Debug.Log(data);
        }

#elif UNITY_WEBGL
        WebGLFileDialog.OpenFile(); // result comes via callback
#endif
    }
}