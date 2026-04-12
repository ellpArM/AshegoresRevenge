using UnityEngine;
using UnityEngine.UI;

public class VoxelSaveLoadUI : MonoBehaviour
{
    public Button saveButton;
    public Button loadButton;
    public Button exportObjButton;
    public Button openVoxelEditorButton;

    public GameObject voxelEditorWindow;
    public World world;
    public VoxelDatabase database;

    void Start()
    {
        saveButton.onClick.AddListener(OnSave);
        loadButton.onClick.AddListener(OnLoad);      
    }

    void OnSave()
    {
        string path = FileDialogUtil.SaveFile("VoxelWorld");
        if (!string.IsNullOrEmpty(path))
            VoxelSaveLoadManager.Save(path, world);
    }

    void OnLoad()
    {
        string path = FileDialogUtil.OpenFile();
        if (!string.IsNullOrEmpty(path))
            VoxelSaveLoadManager.Load(path, world);
        VoxelPaletteUI.instance.RefreshUI();
    }
}
