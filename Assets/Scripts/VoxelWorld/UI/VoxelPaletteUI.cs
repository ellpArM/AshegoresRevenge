using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class VoxelPaletteUI : MonoBehaviour
{
    public static VoxelPaletteUI instance;
    public World world;

    public Transform voxelListRoot;
    public Button voxelButtonPrefab;

    private VoxelDatabase Database => world.VoxelDatabase;

    [Header("Brush Size Sliders")]
    public Slider brushXSlider;
    public Slider brushYSlider;
    public Slider brushZSlider;

    [Header("Brush Size Labels")]
    public TMP_Text brushXValueText;
    public TMP_Text brushYValueText;
    public TMP_Text brushZValueText;

    [Header("Undo / Redo Buttons")]
    public Button undoButton;
    public Button redoButton;

    [Header("References")]
    public VoxelController voxelController;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializeSliders();
        HookUIEvents();
        RefreshUndoRedoButtons();
        RefreshUI();
    }
    void HookUIEvents()
    {
        brushXSlider.onValueChanged.AddListener(v =>
        {
            voxelController.SetBrushSizeX((int)v);
            UpdateBrushLabels();
        });

        brushYSlider.onValueChanged.AddListener(v =>
        {
            voxelController.SetBrushSizeY((int)v);
            UpdateBrushLabels();
        });

        brushZSlider.onValueChanged.AddListener(v =>
        {
            voxelController.SetBrushSizeZ((int)v);
            UpdateBrushLabels();
        });

        undoButton.onClick.AddListener(() =>
        {
            voxelController.Undo();
            RefreshUndoRedoButtons();
        });

        redoButton.onClick.AddListener(() =>
        {
            voxelController.Redo();
            RefreshUndoRedoButtons();
        });
    }
    private void Update()
    {
        RefreshUndoRedoButtons();
    }

    void UpdateBrushLabels()
    {
        brushXValueText.text = brushXSlider.value.ToString();
        brushYValueText.text = brushYSlider.value.ToString();
        brushZValueText.text = brushZSlider.value.ToString();
    }

    void RefreshUndoRedoButtons()
    {
        undoButton.interactable = voxelController.CanUndo;
        redoButton.interactable = voxelController.CanRedo;
    }
    public void CreateVoxel()
    {
        var voxel = ScriptableObject.CreateInstance<VoxelDefinition>();
        voxel.voxelName = "New Voxel";

        for (int i = 0; i < 6; i++)
            voxel.faceTiles[i] = Vector2Int.zero;

        Database.AddVoxel(voxel);
        //world.RegisterVoxel(voxel);
        RefreshUI();
    }
    void InitializeSliders()
    {
        // Assumes controller already has valid values
        brushXSlider.wholeNumbers = true;
        brushYSlider.wholeNumbers = true;
        brushZSlider.wholeNumbers = true;

        brushXSlider.value = voxelController.BrushSize.x;
        brushYSlider.value = voxelController.BrushSize.y;
        brushZSlider.value = voxelController.BrushSize.z;

        UpdateBrushLabels();
    }
    public void RefreshUI()
    {
        foreach (Transform c in voxelListRoot)
            Destroy(c.gameObject);

        foreach (var voxel in Database.voxelDefinitions)
        {
            Button b = Instantiate(voxelButtonPrefab, voxelListRoot);
            b.GetComponentInChildren<TextMeshProUGUI>().text = voxel.voxelName;
            b.onClick.AddListener(() => SelectVoxel(voxel));
        }
    }

    void SelectVoxel(VoxelDefinition voxel)
    {
        world.SetActiveVoxel(voxel);
        FindFirstObjectByType<VoxelController>().SetActiveVoxel(voxel);
    }
}
