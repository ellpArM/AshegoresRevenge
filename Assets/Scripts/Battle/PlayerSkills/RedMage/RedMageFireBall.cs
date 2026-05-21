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

    [Header("Main Damage")]
    public int baseDamage = 5;
    public int levelDamageBonus = 3;
    public int rageBonus = 3;
    public int levelRageBons;
    public ElementType damageType;

    [Header("Area Damage")]
    public float splashRadius = 1.5f;
    public int splashDamage = 2;

    [Header("Knockback")]
    public float pushDistance = 1f;
    public float pushDuration = 0.2f;

    [Header("Effects")]
    public GameObject onHitEffect;

    [Header("Status Effect")]
    public StatusEffect statusEffect;

    [Range(0, 100)]
    public int chanceToProc = 0;

    public AudioClip soundLaunch;
    public AudioClip soundHit;

    private List<PassiveSkill> passiveSkills;
    HeroEntity hero;
    private void Start()
    {
        hero = transform.root.GetComponent<HeroEntity>();
    }

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

        return description.Replace("<damage>", Mathf.RoundToInt(baseDamage * (hero.spellPower / 100f)).ToString());
    }

    public int GetAttackPower()
    {
        int final = baseDamage + ((level - 1) * levelDamageBonus);

        RagePassive rage = hero.GetComponent<RagePassive>();
        if (rage != null)
        {
            final += rage.RemoveAllRage() * (rageBonus + ((level - 1) * levelRageBons));
        }

        foreach (var passive in passiveSkills)
            final = passive.ModifyAttack(final);

        return final;
    }

    private IEnumerator WaitForTargetAndAttack()
    {
        BattleManagerNew.Instance.SetPlayerInput(false);
        List<FightingEntity> enemies = BattleManagerNew.Instance.enemyField.GetEntities().ToList();
        HighLightUnits(enemies);
        BattleManagerNew.Instance.SelectedTarget = null;

        InfoPanel.instance.ShowMessage("Select enemy as target...");

        yield return new WaitUntil(() => BattleManagerNew.Instance.SelectedTarget != null);

        HideHighlight(enemies);
        InfoPanel.instance.Hide();
        FightingEntity target = BattleManagerNew.Instance.SelectedTarget;

        BattleManagerNew.Instance.SelectTarget(null);
        yield return BattleManagerNew.Instance.StartCoroutine(PerformAttackVisuals(target));
        HeroEntity hero = BattleManagerNew.Instance.GetHeroOfelement(damageType);
        int finalDamage = Mathf.RoundToInt(GetAttackPower() * (hero.spellPower / 100f));
        int damageDealt = target.TakeDamage(finalDamage, damageType);

        if (statusEffect != null && damageDealt > 0)
        {
            int roll = Random.Range(0, 100);

            if (roll < chanceToProc)
            {
                target.AddStatusEffect(statusEffect, hero.spellPower);
            }
        }

        Vector3 impactPosition = target.transform.position;
        ApplySplashDamage(impactPosition, target, enemies, hero);
        yield return StartCoroutine(PushTarget(target));


        if (onHitEffect)
        {
            Instantiate(onHitEffect, target.transform.position, Quaternion.identity);
        }

        if (soundHit)
        {
            EffectsManager.instance.CreateSoundEffect(soundHit, transform.position);
        }

        // Resolve deaths
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                yield return StartCoroutine(enemy.ResolveDeathIfNeeded());
        }

        BattleManagerNew.Instance.SetPlayerInput(true);
    }
    private void ApplySplashDamage(Vector3 impactPosition, FightingEntity mainTarget, List<FightingEntity> enemies,HeroEntity hero)
    {
        foreach (FightingEntity enemy in enemies)
        {
            if (enemy == null)
                continue;

            // Main target already receives direct hit
            if (enemy == mainTarget)
                continue;

            float distance = Vector3.Distance(
                impactPosition,
                enemy.transform.position
            );

            if (distance <= splashRadius)
            {
                int finalSplashDamage = Mathf.RoundToInt(splashDamage * (hero.spellPower / 100f));

                enemy.TakeDamage(finalSplashDamage, damageType);
            }
        }
    }

    private IEnumerator PushTarget(FightingEntity target)
    {
        Vector3 startPosition = target.transform.position;
        Vector3 pushDirection =  (target.transform.position - transform.position).normalized;
        Vector3 endPosition = startPosition + pushDirection * pushDistance;

        float elapsed = 0f;

        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pushDuration);
            target.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        target.transform.position = endPosition;
    }

    private IEnumerator PerformAttackVisuals(FightingEntity target)
    {
        if (projectilePrefab != null)
        {
            GameObject formedProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            if (soundLaunch)
            {
                EffectsManager.instance.CreateSoundEffect( soundLaunch, transform.position);
            }

            yield return MoveProjectile( formedProjectile, target.transform.position, travelDuration);
            yield return new WaitForSeconds(impactDelay);

            Destroy(formedProjectile);
        }
        else
        {
            Debug.LogError(
                "Formed projectile prefab missing in AttackSkill!"
            );
        }
    }
}