using UnityEngine;

public class EnemyWorldEntity : MonoBehaviour, ISaveableWorldEntity
{
    [System.Serializable]
    class EnemyExtraData
    {
        public string[] enemyIds;
    }
    public EntitiesDatabase entitiesDB;
    public FightingEntity[] enemies;
    public int enemyId;

    [Header("Visuals")]
    public float spacing = 0.4f;
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    bool triggered = false;
    SelectionCircle selectionCircle;

    private void Start()
    {
        if(enemies.Length == 0)
        {
            enemies = new FightingEntity[Random.Range(0, 3) + 1];
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i] = entitiesDB.GetRandom().GetComponent<FightingEntity>();
            }
            GenerateVisuals();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("enemy encounter");
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        BattleTransitionManager.instance.StartBattle(this, other.transform);
    }
    public WorldEntitySaveData Save()
    {
        string[] ids = new string[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            ids[i] = enemies[i].Guid;
        }

        return new WorldEntitySaveData
        {
            type = "Enemy",
            id = enemyId,
            position = transform.position,
            jsonData = JsonUtility.ToJson(new EnemyExtraData
            {
                enemyIds = ids
            })
        };
    }
    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;
        enemyId = data.id;

        var extra = JsonUtility.FromJson<EnemyExtraData>(data.jsonData);

        enemies = new FightingEntity[extra.enemyIds.Length];

        for (int i = 0; i < extra.enemyIds.Length; i++)
        {
            enemies[i] = EntitiesDatabaseBootstrap.instance.Get(extra.enemyIds[i]).GetComponent<FightingEntity>();
        }
        GenerateVisuals();
    }
    void GenerateVisuals()
    {
        if (enemies == null || enemies.Length == 0)
            return;

        int count = enemies.Length;

        for (int i = 0; i < count; i++)
        {
            var sprite = ExtractSprite(enemies[i]);

            if (sprite == null)
                continue;

            GameObject visual = new GameObject($"EnemyVisual_{i}");
            visual.transform.SetParent(transform);

            Vector3 localPos;

            if (count == 1)
            {
                // center
                localPos = offset;
            }
            else
            {
                // distribute in circle
                float radius = 1f;
                float angle = (Mathf.PI * 2f / count) * i;

                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                localPos = offset + new Vector3(x, 0, z);
            }

            visual.transform.localPosition = localPos;

            var sr = visual.AddComponent<SpriteRenderer>();
            visual.AddComponent<BillboardToCamera>();
            sr.sprite = sprite;
        }
    }

    Sprite ExtractSprite(FightingEntity entityPrefab)
    {
        if (entityPrefab == null)
            return null;

        // IMPORTANT: use prefab root, not instance
        var animator = entityPrefab.GetComponentInChildren<Animator>();

        if (animator == null)
            return null;

        var sr = animator.GetComponent<SpriteRenderer>();

        if (sr == null)
            sr = animator.GetComponentInChildren<SpriteRenderer>();

        return sr != null ? sr.sprite : null;
    }
}