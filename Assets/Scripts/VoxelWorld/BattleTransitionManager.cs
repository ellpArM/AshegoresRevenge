using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTransitionManager : MonoBehaviour
{
    public static BattleTransitionManager Instance;

    public EncounterData CurrentEncounter;

    private string returnScene;
    private string savePath;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PlayerDataManager.instance = GetComponent<PlayerDataManager>();
        DontDestroyOnLoad(gameObject);
    }
    public string GetPath()
    {
        return savePath;
    }
    public void StartBattle(EnemyWorldEntity enemy, Transform player)
    {
        // Stop player movement
        var mover = player.GetComponent<VoxelPlayerMover>();
        if (mover != null)
            mover.StopMovement();

        savePath = Application.persistentDataPath + "/worldsave.json";

        VoxelSaveLoadManager.Save(savePath, World.instance);

        returnScene = SceneManager.GetActiveScene().name;


        CurrentEncounter.enemyId = enemy.enemyId;
        CurrentEncounter.enemies = enemy.enemies;

        // Load battle
        SceneManager.LoadScene("BattleField");
    }

    public void ReturnToWorld(bool playerWon)
    {
        SceneManager.LoadScene(returnScene);

        SceneManager.sceneLoaded += OnWorldLoaded;

        void OnWorldLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnWorldLoaded;

            // Restore world
            VoxelSaveLoadManager.Load(savePath, World.instance, CurrentEncounter.enemyId);

            if (playerWon)
            {
                RemoveDefeatedEnemy();
            }

            Cleanup();
        }
    }

    void RemoveDefeatedEnemy()
    {
        EnemyWorldEntity[] enemies = FindObjectsByType<EnemyWorldEntity>(FindObjectsSortMode.None);

        foreach (var e in enemies)
        {
            if (e.enemyId == CurrentEncounter.enemyId)
            {
                Destroy(e.gameObject);
                break;
            }
        }
    }

    void Cleanup()
    {
        //if (CurrentEncounter != null)
        //    Destroy(CurrentEncounter.gameObject);

        //CurrentEncounter = null;
    }
}