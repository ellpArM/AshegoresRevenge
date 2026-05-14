using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RedMageFireBall : BaseSkill
{
    [Header("Projectile Timing")]
    public GameObject projectilePrefab;
    public float travelDuration = 0.4f;
    public float impactDelay = 0.1f;
    public int baseDamage = 5;
    public ElementType damageType;
    public GameObject onHitEffect;
    [Header("Status Effect")]
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply
    public AudioClip soundLaunch;
    public AudioClip soundHit;
    private List<PassiveSkill> passiveSkills;
    public override IEnumerator Execute()
    {
        passiveSkills = GetComponents<PassiveSkill>().ToList();
        yield return BattleManagerNew.Instance.StartCoroutine(WaitForTargetAndAttack());
        BattleManagerNew.Instance.NotifyPlayerSkillUsed();
    }
    public void HighLightUnits(List<FightingEntity> enemies)
    {
        foreach (var e in enemies)
        {
            if (e != null)
                e.ShowSelector(SelectionState.Red);
        }
    }
    public void HideHighlight(List<FightingEntity> enemies)
    {
        foreach (var e in enemies)
        {
            if (e != null)
                e.HideSelector();
        }
    }
    public override string UpdatedDescription()
    {
        HeroEntity hero = BattleManagerNew.Instance.GetHeroOfelement(damageType);
        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }
    public int GetAttackPower()
    {
        int final = baseDamage;

        foreach (var passive in passiveSkills)
            final = passive.ModifyAttack(final);

        return final;
    }

    private IEnumerator WaitForTargetAndAttack()
    {
        BattleManagerNew.Instance.SetPlayerInput(false);
        List<FightingEntity> enemies = BattleManagerNew.Instance.enemyField.GetEntities();

        HighLightUnits(enemies);

        BattleManagerNew.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select enemy as target...");

        yield return new WaitUntil(() => BattleManagerNew.Instance.SelectedTarget != null);

        HideHighlight(enemies);
        InfoPanel.instance.Hide();
        var target = BattleManagerNew.Instance.SelectedTarget;
        BattleManagerNew.Instance.SelectTarget(null);

        yield return BattleManagerNew.Instance.StartCoroutine(PerformAttackVisuals(target));

        HeroEntity hero = BattleManagerNew.Instance.GetHeroOfelement(damageType);
        int damageDealth = target.TakeDamage(Mathf.RoundToInt(GetAttackPower() * (hero.spellPower / 100f)), damageType);
        if (statusEffect != null && damageDealth > 0)
        {
            int roll = Random.Range(0, 100);
            if (roll < chanceToProc)
            {
                target.AddStatusEffect(statusEffect, hero.spellPower);
            }
        }

        if (onHitEffect)
            Instantiate(onHitEffect, target.transform.position, Quaternion.identity);

        if (soundHit)
            EffectsManager.instance.CreateSoundEffect(soundHit, transform.position);

        yield return StartCoroutine(target.ResolveDeathIfNeeded());
        BattleManagerNew.Instance.SetPlayerInput(true);
    }
    private IEnumerator PerformAttackVisuals(FightingEntity target)
    {
        if (projectilePrefab != null)
        {
            GameObject formedProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (soundLaunch)
                EffectsManager.instance.CreateSoundEffect(soundLaunch, transform.position);

            yield return MoveProjectile(formedProjectile, target.transform.position, travelDuration);

            // Impact delay before damage application
            yield return new WaitForSeconds(impactDelay);

            Destroy(formedProjectile);
        }
        else
        {
            Debug.LogError("Formed projectile prefab missing in AttackSkill!");
        }
    }
}
