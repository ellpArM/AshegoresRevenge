using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RedMageShield : BaseSkill
{
    [Header("Skill Visuals")]
    public GameObject effectPrefab;
    //public ElementIconLibrary elementsLib;

    [Header("Passive Skill to Grant")]
    public PassiveSkill passiveToGrant;   // Prefab or component reference
    public ElementType mainElement;
    public AudioClip effectSound;

    public override IEnumerator Execute()
    {
        yield return BattleManagerNew.Instance.StartCoroutine(ApplyPassivesRoutine());
        BattleManagerNew.Instance.NotifyPlayerSkillUsed();
    }
    public override string UpdatedDescription()
    {
        HeroEntity hero = BattleManagerNew.Instance.GetHeroOfelement(mainElement);
        return description.Replace("<passive_info>", passiveToGrant.GetDescription(hero.spellPower));
    }

    private IEnumerator ApplyPassivesRoutine()
    {
        BattleManagerNew.Instance.SetPlayerInput(false);

        InfoPanel.instance.ShowMessage("Empowering...");

        InfoPanel.instance.Hide();

        yield return new WaitForSeconds(0.1f);

        // Apply passive to all allied heroes
        List<HeroEntity> heroes = BattleManagerNew.Instance.PlayerHeroes;
        HeroEntity mainHero = BattleManagerNew.Instance.GetHeroOfelement(mainElement);

        foreach (var hero in heroes)
        {
            if (hero == null) continue;

            // Remove old version if present
            PassiveSkill existing = hero.GetComponent(passiveToGrant.GetType()) as PassiveSkill;
            if (existing != null)
            {
                GameObject.Destroy(existing);
            }

            // Add new passive
            PassiveSkill newPassive = hero.gameObject.AddComponent(passiveToGrant.GetType()) as PassiveSkill;

            // Optional: copy over values from the assigned template
            CopyPassiveValues(passiveToGrant, newPassive);

            // Optional spellPower scaling
            newPassive.InitializeFromCaster(mainHero);
        }
        if (effectSound != null)
            EffectsManager.instance.CreateSoundEffect(effectSound, mergePoint);

        BattleManagerNew.Instance.SetPlayerInput(true);
    }

    private void CopyPassiveValues(PassiveSkill source, PassiveSkill target)
    {
        // Copies public fields so PassiveSkill prefabs can hold data
        var type = source.GetType();
        var fields = type.GetFields();

        foreach (var f in fields)
        {
            f.SetValue(target, f.GetValue(source));
        }
    }
}
