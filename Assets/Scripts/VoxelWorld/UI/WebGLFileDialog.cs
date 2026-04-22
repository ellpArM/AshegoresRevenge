using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLFileDialog
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenFileDialog();

    [DllImport("__Internal")]
    private static extern void SaveFileDialog(string filename, string data);
#endif

    public static void OpenFile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenFileDialog();
#endif
    }

    public static void SaveFile(string filename, string data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveFileDialog(filename, data);
#endif
    }
}