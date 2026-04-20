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

    [SerializeField] private int maxVisible = 10;

    private List<VoxelDefinition> visibleVoxels = new();
    private List<VoxelButtonUI> buttons = new();

    private int selectedIndex = -1;
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
        HandleHotkeys();
        HandleScrollInput();
    }
    void HandleHotkeys()
    {
        for (int i = 0; i < visibleVoxels.Count; i++)
        {
            KeyCode key = (i == 9) ? KeyCode.Alpha0 : KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(key))
            {
                //SelectVoxel(visibleVoxels[i]);
                SelectIndex(i);
            }
        }
    }
    void HandleScrollInput()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (scroll > 0)
        {
            SelectIndex(selectedIndex + 1);
        }
        else if (scroll < 0)
        {
            SelectIndex(selectedIndex - 1);
        }
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

        visibleVoxels.Clear();
        buttons.Clear();

        int count = Mathf.Min(maxVisible, Database.voxelDefinitions.Count);

        for (int i = 0; i < count; i++)
        {
            var voxel = Database.voxelDefinitions[i];
            visibleVoxels.Add(voxel);

            var obj = Instantiate(voxelButtonPrefab.gameObject, voxelListRoot);
            var buttonUI = obj.GetComponent<VoxelButtonUI>();
            buttonUI.GetComponent<Button>().onClick.AddListener(() => SelectVoxel(voxel));

            buttonUI.Setup(voxel, i, SelectVoxel);

            buttons.Add(buttonUI);
        }
        if (visibleVoxels.Count > 0)
        {
            SelectIndex(0);
        }
    }
    void SelectIndex(int index)
    {
        if (visibleVoxels.Count == 0)
            return;

        // Loop index
        if (index < 0)
            index = visibleVoxels.Count - 1;
        else if (index >= visibleVoxels.Count)
            index = 0;

        selectedIndex = index;

        var selectedVoxel = visibleVoxels[index];

        // Update visuals
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetSelected(i == selectedIndex);
        }

        // Apply selection
        SelectVoxel(selectedVoxel);
    }

    void SelectVoxel(VoxelDefinition voxel)
    {
        world.SetActiveVoxel(voxel);
        FindFirstObjectByType<VoxelController>().SetActiveVoxel(voxel);

        int index = visibleVoxels.IndexOf(voxel);
        if (index != -1 && index != selectedIndex)
        {
            SelectIndex(index);
            return;
        }
    }
}
