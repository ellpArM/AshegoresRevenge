using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VoxelSaveLoadUI : MonoBehaviour
{
    public Button saveButton;
    public Button loadButton;
    public Button exportObjButton;
    public Button openVoxelEditorButton;
    public Button exitButton;

    public GameObject voxelEditorWindow;
    public World world;
    public VoxelDatabase database;

    void Start()
    {
        saveButton.onClick.AddListener(OnSave);
        loadButton.onClick.AddListener(OnLoad);
        exitButton.onClick.AddListener(Exit);      
    }

    void OnSave()
    {
        string data = VoxelSaveLoadManager.JsonData(world);
        FileDialogUtil.SaveFile("VoxelWorld", data);
        //if (!string.IsNullOrEmpty(path))
        //    VoxelSaveLoadManager.Save(path, world);
    }

    void OnLoad()
    {
        FileDialogUtil.OpenFile();
    }
    public void Exit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
