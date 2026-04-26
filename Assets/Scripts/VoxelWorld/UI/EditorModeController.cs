using UnityEngine;

public class EditorModeController : MonoBehaviour
{
    public enum EditorMode
    {
        Voxels,
        Entities
    }

    public EditorMode currentMode;

    [Header("References")]
    public VoxelController voxelController;
    public EntitiesEditorController entitiesController;
    public GameObject voxelPalette;
    public GameObject entitiesPalette;
    private void Start()
    {
        UpdateModes();
    }

    public void SetVoxelMode()
    {
        currentMode = EditorMode.Voxels;
        UpdateModes();
    }

    public void SetEntitiesMode()
    {
        currentMode = EditorMode.Entities;
        UpdateModes();
    }

    private void UpdateModes()
    {
        voxelController.enabled = (currentMode == EditorMode.Voxels);
        voxelPalette.SetActive(currentMode == EditorMode.Voxels);

        entitiesController.enabled = (currentMode == EditorMode.Entities);
        entitiesPalette.SetActive(currentMode == EditorMode.Entities);
    }
}