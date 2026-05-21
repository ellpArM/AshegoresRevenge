using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BattleManagerNew : MonoBehaviour
{
    public static BattleManagerNew Instance { get; private set; }

    [Header("Game State")]
    public bool playerTurn = true;
    public int actionsThisTurn = 3;
    public int maxActionsPerTurn = 3;
    public int maxCardsInHand = 5;
    public Material skybox;

    [Header("References")]
    [SerializeField] private SkillsHolderUI playerActions;
    public TroopsField playerField;
    [SerializeField] public TroopsField enemyField;
    [SerializeField] private WaveSpawner enemySpawner;
    [SerializeField] private PlayerInputController playerInput;
    [SerializeField] private List<HeroEntity> startingHeroes;
    public AudioSource bgm;
    public AudioClip winSound;
    public AudioClip lossSound;
    public bool IsActionPending(string type) => pendingActionType == type;
    [Header("Stages swap")]
    [HideInInspector] public int waveCounter = 0;
    private int currentStageIndex = 0;
    EncounterData currentRoom;
    public Image blackFade;
    public AudioClip useSound;

    [Header("Monster Scaling")]
    public float hpScaleIncPerWave = 0.025f;
    public float dmgScaleIncPerWave = 0.015f;
    public float hpScaleIncPerStage = 0.25f;
    public float dmgScaleIncPerStage = 0.15f;

    private string pendingActionType = null;

    [Header("Turn Settings")]
    [SerializeField] private float enemyTurnDuration = 2f;

    //[Header("Global Skills")]
    //skill prefab must be instantiated or they can't start CoRoutines
    //public List<BaseSkill> allSkills = new List<BaseSkill>();
    public GameObject skillsContainer;

    private List<ElementType> selectedElements = new List<ElementType>();

    [Header("Skill UI")]
    public TextMeshProUGUI actionsText;
    public TextMeshProUGUI healText;

    public bool PlayerInputEnabled => playerInput.InputEnabled;

    private int currentTurn = 0;
    private HeroEntity selectedHero;
    public List<HeroEntity> PlayerHeroes { get; private set; } = new();
    public FightingEntity SelectedTarget;
    private List<ElementalCardInstance> selectedCards = new List<ElementalCardInstance>();
    private BaseSkill matchedSkill = null;
    [SerializeField] private bool waveActive = false;
    public HeroSelectionData heroSelectionData;
    public FightingEntity[] testEnemies;
    private bool skillWasUsed;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //allSkills = GetComponentsInChildren<BaseSkill>().ToList();
        StartCoroutine(GameStartRoutine());
        //StartCoroutine(WaveMonitorRoutine());
    }

    public List<FightingEntity> GetEnemies()
    {
        return enemyField.GetEntities();
    }

    internal HeroEntity GetHeroOfelement(ElementType element)
    {
        foreach (var hero in PlayerHeroes)
        {
            if (hero.mainElement == element)
                return hero;
        }
        return null;
    }
    private IEnumerator GameStartRoutine()
    {
        yield return StartCoroutine(LoadNextRoomEnvironment());

        // Spawn heroes
        for (int i = 0; i < heroSelectionData.selectedHeroes.Length; i++)
        {
            HeroEntity hero = Instantiate(heroSelectionData.selectedHeroes[i], playerField.transform.position, Quaternion.identity).GetComponent<HeroEntity>();
            hero.name = heroSelectionData.selectedHeroes[i].name;
            hero.troopsField = playerField;
            PlayerHeroes.Add(hero);
            
            // restore later
            //CharacterData data = PlayerDataManager.instance.GetHeroData(hero.Guid);
            //if (data != null)
            //{
            //    data.RefreshStats();
            //    hero.ApplyData(data);

            //}
            //else
            //{
            //    hero.SetData();
            //    hero.ApplyData(PlayerDataManager.instance.GetHeroData(hero.Guid));

            //}

            yield return playerField.AddEntity(hero);

        }
        
        yield return enemySpawner.SpawnWaveCoroutine(testEnemies);
        //yield return enemySpawner.SpawnWaveCoroutine(BattleTransitionManager.instance.CurrentEncounter.enemies);
        waveActive = true;

        yield return StartCoroutine(PlayerTurnSimple()); // trying out simple turn order
    }

    private void AddSpells(List<GameObject> spells)
    {
        foreach (GameObject spell in spells)
        {
            BaseSkill skill = spell.GetComponent<BaseSkill>();
            Instantiate(skill, skillsContainer.transform);
            
        }
    }
    private IEnumerator PlayerTurnSimple()
    {
        currentTurn++;

        if (playerField.AllHeroesDefeated())
        {
            GameOverScreen.instance.Show();
        }

        FightingEntity[] playerUnits = playerField.GetEntities().ToArray();
        foreach (var hero in playerUnits)
        {
            if (hero == null) continue;
            yield return StartCoroutine(hero.ProcessStartOfTurnEffects());
            yield return StartCoroutine(hero.ProcessPassivesTurnStart());
            yield return new WaitForSeconds(0.2f);
        }

        List<FightingEntity> justHeroes = playerField.GetEntities().ToList();
        List<FightingEntity> allPlayerUnits = playerField.GetEntities().ToList();
        List<FightingEntity> enemies = enemyField.GetEntities();
        foreach (var enemy in enemies)
        {
            SkillSelector selector = enemy.GetComponent<SkillSelector>();
            if (selector != null)
                selector.PreselectIntent(allPlayerUnits, enemies);
        }

        justHeroes.RemoveAll(card =>
        {
            HeroEntity hero = card as HeroEntity;
            return hero == null || hero.isDefeated;
        });

        actionsThisTurn = maxActionsPerTurn;
        ResetAllAttacks();

        SetPlayerInput(true);
        //foreach (var hero in justHeroes)
        //    hero.ShowSelector(SelectionState.Active);

        //yield return new WaitUntil(() => playerInput.EndTurnPressed || enemyField.GetEntities().Count == 0);
        //playerInput.EndTurnPressed = false;

        ////if (actionsThisTurn > 0)
        ////    playerDeck.DrawMultiple(actionsThisTurn);
        
        foreach (HeroEntity hero in justHeroes)
        {
            if (hero == null || hero.isDefeated)
                continue;

            hero.ShowSelector(SelectionState.Active);
            SkillsHolderUI.instance.Initialize(hero);
            SkillsHolderUI.instance.Show();

            skillWasUsed = false;

            // Wait until one of hero skills is used
            yield return new WaitUntil(() => skillWasUsed);

            hero.HideSelector();
            yield return new WaitForSeconds(0.15f);
            if (enemyField.GetEntities().Count == 0)
                break;
        }

        SkillsHolderUI.instance.Hide();

        SetPlayerInput(false);
        foreach (var hero in justHeroes)
            hero.HideSelector();

        if (enemyField.GetEntities().Count > 0)
            StartCoroutine(SummonsTurnSimple());
        else
            yield return SimpleWaveTransition();
    }
    public void NotifyPlayerSkillUsed()
    {
        skillWasUsed = true;
    }
    private IEnumerator SummonsTurnSimple()
    {
        Debug.Log("Player minions turn start");
        List<FightingEntity> enemyCards = enemyField.GetEntities();
        List<FightingEntity> summonCards = playerField.GetEntities().ToList();
        summonCards.RemoveAll(card =>
        {
            HeroEntity hero = card as HeroEntity;
            return hero != null;
        });

        //enemyCards = enemyField.GetCards().ToList(); // Rebuild the list after potential removals
        summonCards.RemoveAll(e => e.GetComponent<FreezeEffect>() != null); // make frozen units skip action

        foreach (var summon in summonCards)
            summon.ShowSelector(SelectionState.Inactive);

        foreach (var summon in summonCards)
        {
            summon.ShowSelector(SelectionState.Active);
            enemyCards = enemyField.GetEntities();
            if (summon == null) continue;
            if (enemyCards.Count == 0) break;

            BaseMonsterSkill[] skills = summon.gameObject.GetComponents<BaseMonsterSkill>();

            BaseMonsterSkill selectedSkill = skills[Random.Range(0, skills.Length)];

            FightingEntity target = enemyCards[Random.Range(0, enemyCards.Count)];
            yield return selectedSkill.StartCoroutine(selectedSkill.Execute(target));
            yield return new WaitForSeconds(0.2f);
            summon.HideSelector();
        }
        Debug.Log("Player minions turn end");
        foreach (var summon in summonCards)
            summon.HideSelector();

        if (enemyField.GetEntities().Count > 0)
            StartCoroutine(EnemyTurnSimple());
        else
            yield return SimpleWaveTransition();
    }
    private IEnumerator EnemyTurnSimple()
    {
        List<FightingEntity> enemyCards = enemyField.GetEntities().ToList();
        List<FightingEntity> playerCards = playerField.GetEntities().ToList();
        playerCards.RemoveAll(card =>
        {
            HeroEntity hero = card as HeroEntity;
            return hero != null && hero.isDefeated;
        });

        foreach (var enemy in enemyCards)
        {
            if (enemy == null) continue;

            yield return StartCoroutine(enemy.ProcessStartOfTurnEffects());
            yield return StartCoroutine(enemy.ProcessPassivesTurnStart());
        }

        enemyCards = enemyField.GetEntities().ToList(); // Rebuild the list after potential removals
        enemyCards.RemoveAll(e => e.GetComponent<FreezeEffect>() != null); // make frozen units skip action

        if(enemyCards.Count > 0)
        {
            foreach (var enemyCard in enemyCards)
                enemyCard.ShowSelector(SelectionState.Inactive);

            foreach (var enemyCard in enemyCards)
            {
                if (enemyCard == null)
                    continue;

                enemyCard.ShowSelector(SelectionState.Active);

                SkillSelector selector = enemyCard.GetComponent<SkillSelector>();

                if (selector != null && selector.SelectedSkill != null)
                {
                    // Execute preselected intent
                    yield return selector.StartCoroutine(selector.ExecuteIntent());
                }
                else
                {
                    // Fallback to old behavior
                    BaseMonsterSkill[] skills = enemyCard
                        .GetComponents<BaseMonsterSkill>()
                        .Where(x => x.enabled)
                        .ToArray();

                    if (skills.Length == 0 || playerCards.Count == 0)
                        continue;

                    BaseMonsterSkill selectedSkill =
                        skills[Random.Range(0, skills.Length)];

                    FightingEntity target =
                        playerCards[Random.Range(0, playerCards.Count)];

                    yield return selectedSkill.StartCoroutine(selectedSkill.Execute(target));
                    yield return selectedSkill.StartCoroutine(selectedSkill.PostExecute());
                }

                yield return new WaitForSeconds(0.5f);
                enemyCard.HideSelector();
            }


            yield return new WaitForSeconds(enemyTurnDuration);
            StartCoroutine(PlayerTurnSimple());
        }
        else
            yield return SimpleWaveTransition();
    }
    private IEnumerator SimpleWaveTransition()
    {
        yield return new WaitForSeconds(0.5f);
        waveActive = false;

        foreach (var hero in PlayerHeroes)
        {
            hero.SetData();            
        }

        BattleTransitionManager.instance.ReturnToWorld(true);
    }
    public void SelectTarget(FightingEntity target)
    {
        SelectedTarget = target;
    }

    public void SelectHero(HeroEntity hero)
    {
        selectedHero = hero;
    }
    public HeroEntity GetSelectedHero()
    {
        return selectedHero;
    }
    private void Update()
    {
        actionsText.text = $"actions left: {actionsThisTurn}";
    }

    public void RegisterActionUse(int cost)
    {
        actionsThisTurn -= cost;

        if (actionsThisTurn <= 0 || (PlayerHand.instance.GetCards().Count == 0 && actionsThisTurn <= 0))
        {
            StartCoroutine(EndTurnAfterDelay());
        }
    }
    private IEnumerator EndTurnAfterDelay()
    {
        // Wait a bit to let final skill visuals complete
        yield return new WaitForSeconds(1f);
        playerInput.EndTurnPressed = true;
    }
    private void HealOnWaveClear()
    {
        // experimental... looking for ways to replace green mage in battles.
        if (actionsThisTurn > 0)
        {
            foreach (var hero in PlayerHeroes)
            {
                hero.maxHealth += 1;
                hero.Heal(10);
            }
        }
    }

    private void ResetAllAttacks()
    {
        foreach (var c in playerField.GetEntities())
        {
            if (c != null) c.HasActedThisTurn = false;
        }
    }

    public void SetPlayerInput(bool enabled)
    {
        playerInput.SetInputEnabled(enabled);
    }
    private List<BaseSkill> FindMatchingSkills(List<ElementType> combo)
    {
        List<BaseSkill> result = new List<BaseSkill>();

        foreach (var skill in skillsContainer.GetComponentsInChildren<BaseSkill>())
        {
            if (skill.requiredElements.Count != combo.Count)
                continue;

            List<ElementType> skillElements = new List<ElementType>(skill.requiredElements);
            List<ElementType> comboElements = new List<ElementType>(combo);

            bool match = true;

            foreach (var elem in comboElements)
            {
                if (skillElements.Contains(elem))
                {
                    skillElements.Remove(elem);
                }
                else
                {
                    match = false;
                    break;
                }
            }

            if (match && skillElements.Count == 0)
                result.Add(skill);
        }

        return result;
    }

    public IEnumerator ExecuteSkill(BaseSkill chosenSkill, int cost)
    {
        yield return chosenSkill.Execute();
        yield return chosenSkill.PostExecute();
        RegisterActionUse(cost);
    }

    public IEnumerator EndLevel()
    {
        Debug.Log("EndLevel triggered.");

        SetPlayerInput(false);
        InfoPanel.instance.ShowMessage("Stage Cleared!");

        yield return new WaitForSeconds(1f);

        {
            bgm.Stop();
            yield return new WaitForSeconds(1f);
            EffectsManager.instance.CreateSoundEffect(winSound, Vector3.zero);
            InfoPanel.instance.Hide();
            yield return StartCoroutine(JustFadeOut());
            yield return new WaitForSeconds(4f);
            //SceneController.ToEndScene();
            // return back to Map
        }
    }
    public void RevieveFallenHeroes()
    {
        foreach (var hero in PlayerHeroes)
        {
            if (hero.isDefeated)
                hero.Revive();
        }
    }

    bool movingCamera;
    private IEnumerator MoveCamera(CameraPathPoint[] path)
    {
        //camController.enabled = false;
        //foreach (var point in path)
        //{
        //    Transform target = point.target;
        //    float duration = point.time;

        //    Vector3 startPos = cam.transform.position;
        //    Quaternion startRot = cam.transform.rotation;

        //    float t = 0f;
        //    while (t < duration)
        //    {
        //        t += Time.deltaTime;
        //        float normalized = t / duration;

        //        cam.transform.position = Vector3.Lerp(startPos, target.position, normalized);
        //        cam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, normalized);

        //        yield return null;
        //    }

        //    cam.transform.position = target.position;
        //    cam.transform.rotation = target.rotation;


        //    camController.ResetValues();
        //    //yield return new WaitForSeconds(0.05f);
        //}
        //movingCamera = false;
        yield return null;
    }
    public IEnumerator JustFadeOut()
    {
        float duration = 1f;
        blackFade.gameObject.SetActive(true);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            Color c = blackFade.color;
            c.a = Mathf.Lerp(0f, 1f, normalized);
            blackFade.color = c;
            yield return null;
        }
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 1f);
    }

    public IEnumerator FadeScreen()
    {
        float duration = 1f;
        blackFade.gameObject.SetActive(true);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            Color c = blackFade.color;
            c.a = Mathf.Lerp(0f, 1f, normalized);
            blackFade.color = c;
            yield return null;
        }
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 1f);

        yield return StartCoroutine(LoadNextRoomEnvironment());


        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            Color c = blackFade.color;
            c.a = Mathf.Lerp(1f, 0f, normalized);
            blackFade.color = c;
            yield return null;
        }

        blackFade.gameObject.SetActive(false);
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 0f);
    }

    private IEnumerator MoveHeroesThroughPath(Transform[] pathPoints)
    {
        if (pathPoints == null || pathPoints.Length == 0)
            yield break;

        List<HeroEntity> heroes = PlayerHeroes;

        float travelSpeed = 6f;

        // Move heroes through each point in sequence
        foreach (Transform t in pathPoints)
        {
            bool allReached = false;

            while (!allReached)
            {
                allReached = true;

                foreach (var hero in heroes)
                {
                    if (hero == null) continue;

                    hero.transform.position = Vector3.MoveTowards(
                        hero.transform.position,
                        t.position,
                        travelSpeed * Time.deltaTime
                    );

                    if (Vector3.Distance(hero.transform.position, t.position) > 0.05f)
                        allReached = false;
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator LoadNextRoomEnvironment()
    {

        yield return null;
        //BattleTransitionManager.instance.CurrentEncounter.enemies
        World.instance.Initialize();

        playerField.SetPositions(true);
        enemyField.SetPositions(false);
        //EncounterData roomPrefab = allStages[currentStageIndex];
        //if (currentRoom != null)
        //    Destroy(currentRoom.gameObject);

        //currentRoom = Instantiate(roomPrefab, roomOrigin);
        yield return null;
        //playerField.fieldPositions = currentRoom.playerLocations.ToList();
        //playerField.spawnPoint = currentRoom.playerEnterLocation;
        //enemyField.fieldPositions = currentRoom.enemyLocations.ToList();
        //enemyField.spawnPoint = currentRoom.enemyEnterLocation;
        //exitPathPoints = currentRoom.exitPath;
        //enterCameraPath = currentRoom.enterCameraPath;
        //exitCameraPath = currentRoom.exitCameraPath;
        //RenderSettings.skybox = currentRoom.skybox;
        //enemySpawner.BossPrefab = currentRoom.StageBoss;
        //enemySpawner.enemies = currentRoom.enemies.ToList();

        //playerField.ClearPositions();
        //foreach (var hero in PlayerHeroes)
        //{
        //    hero.transform.position = currentRoom.playerEnterLocation.position;
        //    yield return playerField.ReasignPositions(hero);
        //}

        //yield return new WaitForSeconds(1f);
    }
}
