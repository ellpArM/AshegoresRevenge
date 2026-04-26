using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorOnlyVisual : MonoBehaviour
{
    [SerializeField] private string editorSceneName = "MapEditor";

    private void Awake()
    {
        bool isEditorScene = SceneManager.GetActiveScene().name == editorSceneName;

        if (!isEditorScene)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }
    }
}