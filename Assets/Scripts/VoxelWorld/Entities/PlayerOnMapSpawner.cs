using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerOnMapSpawner : MonoBehaviour, ISaveableWorldEntity
{
    public GameObject playerPrefabOverride;
    [SerializeField] private string editorSceneName = "WorldEditor";

    private bool hasSpawned = false;

    private void Start()
    {
        bool isEditorScene = SceneManager.GetActiveScene().name == editorSceneName;
        if (!isEditorScene)
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        if (hasSpawned)
            return;

        World world = FindFirstObjectByType<World>();

        if (world == null)
        {
            Debug.LogError("PlayerOnMapSpawner: World not found.");
            return;
        }

        GameObject prefab = playerPrefabOverride != null
            ? playerPrefabOverride
            : world.playerPrefab;

        if (prefab == null)
        {
            Debug.LogError("PlayerOnMapSpawner: No player prefab assigned.");
            return;
        }
        Instantiate(prefab, transform.position, Quaternion.identity);
        hasSpawned = true;
    }

    public WorldEntitySaveData Save()
    {
        return new WorldEntitySaveData
        {
            type = name.Replace("(Clone)", "").Trim(),
            position = transform.position,
            //jsonData = JsonUtility.ToJson(this)
        };
    }

    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;

        //JsonUtility.FromJsonOverwrite(data.jsonData, this);
    }
}