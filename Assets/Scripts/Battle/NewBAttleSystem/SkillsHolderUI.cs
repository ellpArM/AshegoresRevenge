using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsHolderUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image characterPortrait;

    [Header("Skill Slots")]
    private readonly List<SkillSlotUI> spawnedSlots = new();

    [Header("Root Object")]
    [SerializeField] private GameObject rootObject;

    private HeroEntity currentHero;
    public static SkillsHolderUI instance;
    public SkillSlotUI skillSlotPrefab;
    public Transform skillsContainer;

    private void Awake()
    {
        instance = this;
    }
    public void Initialize(HeroEntity hero)
    {
        currentHero = hero;

        if (currentHero == null)
        {
            Hide();
            return;
        }

        InitializePortrait();
        InitializeSkills();
    }

    private void InitializePortrait()
    {
        if (characterPortrait == null)
            return;

        //characterPortrait.sprite = currentHero.Portrait;
        //characterPortrait.enabled = currentHero.Portrait != null;
    }

    private void InitializeSkills()
    {
        ClearSkillSlots();

        List<BaseSkill> skills = currentHero.AvailableSkills;

        foreach (BaseSkill skill in skills)
        {
            if (skill == null)
                continue;

            SkillSlotUI slot = Instantiate(
                skillSlotPrefab,
                skillsContainer
            );
            slot.transform.localPosition = new Vector3(spawnedSlots.Count * 80,0,0);

            slot.Initialize(skill);

            spawnedSlots.Add(slot);
        }
    }
    private void ClearSkillSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
            {
                Destroy(spawnedSlots[i].gameObject);
            }
        }

        spawnedSlots.Clear();
    }

    public void Show()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}