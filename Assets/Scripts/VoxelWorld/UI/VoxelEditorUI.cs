using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VoxelEditorUI : MonoBehaviour
{
    [Header("References")]
    public VoxelDatabase database;

    [Header("UI")]
    public TMP_Dropdown voxelDropdown;
    public TMP_Dropdown atlasDropdown;

    public RawImage atlasPreviewImage;
    public RawImage voxelPreviewImage;

    public Button addButton;
    public Button updateButton;
    public Button removeButton;
    public Button closeButton;
    public Button setFacesButton;

    public TMP_InputField voxelNameInput;
    public Toggle solidToggle;

    private VoxelDefinition selectedVoxel;
    private VoxelAtlas selectedAtlas;

    private bool isSelectingFaces = false;
    private int faceSelectionIndex = 0;

    private Vector2Int[] pendingFaceTiles = new Vector2Int[6];
    private int voxelIdx = 0;
    private int atlasIdx = 0;

    private void Start()
    {
        addButton.onClick.AddListener(AddVoxel);
        updateButton.onClick.AddListener(UpdateVoxel);
        removeButton.onClick.AddListener(RemoveVoxel);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        setFacesButton.onClick.AddListener(StartFaceSelection);

        atlasDropdown.onValueChanged.AddListener(OnAtlasChanged);
        voxelDropdown.onValueChanged.AddListener(OnVoxelChanged);
        gameObject.SetActive(false);
    }
    private void OnBecameVisible()
    {
    }
    private void OnEnable()
    {
        PopulateVoxelDropdown();
        PopulateAtlasDropdown();
        OnAtlasChanged(atlasIdx);
        OnVoxelChanged(voxelIdx);
    }

    #region Dropdown Population

    void PopulateVoxelDropdown()
    {
        voxelDropdown.ClearOptions();
        List<string> options = new();

        foreach (var voxel in database.voxelDefinitions)
            options.Add(voxel.voxelName);

        voxelDropdown.AddOptions(options);
    }

    void PopulateAtlasDropdown()
    {
        atlasDropdown.ClearOptions();
        List<string> options = new();

        foreach (var atlas in database.Atlases)
            options.Add(atlas.atlasName);

        atlasDropdown.AddOptions(options);
    }

    #endregion

    #region Voxel CRUD

    void AddVoxel()
    {
        VoxelDefinition newVoxel = new VoxelDefinition();
        newVoxel.voxelName = voxelNameInput.text;
        newVoxel.isSolid = solidToggle.isOn;
        newVoxel.atlas = selectedAtlas;
        newVoxel.faceTiles = new Vector2Int[6];

        database.voxelDefinitions.Add(newVoxel);
        PopulateVoxelDropdown();
    }

    void UpdateVoxel()
    {
        if (selectedVoxel == null) return;

        selectedVoxel.voxelName = voxelNameInput.text;
        selectedVoxel.isSolid = solidToggle.isOn;
        selectedVoxel.atlas = selectedAtlas;
        selectedVoxel.faceTiles = pendingFaceTiles;

        PopulateVoxelDropdown();
        GeneratePreview();
    }

    void RemoveVoxel()
    {
        if (selectedVoxel == null) return;

        database.voxelDefinitions.Remove(selectedVoxel);
        selectedVoxel = null;
        PopulateVoxelDropdown();
    }

    #endregion

    #region Selection

    void OnVoxelChanged(int index)
    {
        if (index < 0 || index >= database.voxelDefinitions.Count) return;
        voxelIdx = index;

        selectedVoxel = database.voxelDefinitions[index];
        voxelNameInput.text = selectedVoxel.voxelName;
        solidToggle.isOn = selectedVoxel.isSolid;
        selectedAtlas = selectedVoxel.atlas;

        pendingFaceTiles = selectedVoxel.faceTiles;
        GeneratePreview();
    }

    void OnAtlasChanged(int index)
    {
        if (index < 0 || index >= database.Atlases.Count) return;
        atlasIdx = index;

        selectedAtlas = database.Atlases[index];
        atlasPreviewImage.texture = selectedAtlas.Texture;
    }

    #endregion

    #region Face Selection

    void StartFaceSelection()
    {
        if (selectedAtlas == null) return;

        isSelectingFaces = true;
        faceSelectionIndex = 0;

        Debug.Log("Select 6 tiles for faces (Top, Bottom, Left, Right, Front, Back)");
    }

    public void OnAtlasClicked(Vector2 localUV)
    {
        if (!isSelectingFaces) return;

        int tileX = Mathf.FloorToInt(localUV.x * selectedAtlas.gridSize);
        int tileY = Mathf.FloorToInt(localUV.y * selectedAtlas.gridSize);

        pendingFaceTiles[faceSelectionIndex] = new Vector2Int(tileX, tileY);

        faceSelectionIndex++;

        if (faceSelectionIndex >= 6)
        {
            isSelectingFaces = false;
            Debug.Log("Face selection complete.");
            GeneratePreview();
        }
    }

    #endregion

    #region Preview Generation

    void GeneratePreview()
    {
        if (selectedAtlas == null) return;

        Texture2D previewTexture = VoxelPreviewGenerator.Generate(
            selectedAtlas,
            pendingFaceTiles
        );

        voxelPreviewImage.texture = previewTexture;
    }

    #endregion
}
