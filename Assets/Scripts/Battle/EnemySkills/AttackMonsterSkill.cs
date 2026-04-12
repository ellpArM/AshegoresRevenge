using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AttackMonsterSkill : BaseMonsterSkill
{
    [Header("Base Settings")]
    public int damage;
    public ElementType element = ElementType.Physical;
    public bool isProjectileAttack;
    public int baseAccuracy = 95;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;     // Assign in Inspector
    public float projectileSpeed = 12f;
    private bool castEventTriggered = false;
    public Transform projectileSpawnPoint;

    [Header("Status effect Settings")]
    public StatusEffect statusEffect;   // The effect to apply
    [Range(0, 100)]
    public int chanceToProc = 0;        // % chance to apply
    private List<PassiveSkill> passiveSkills;
    public AudioClip originSound;
    public AudioClip hitSound;

    [Header("Intent Indicator")]
    [SerializeField] private GameObject intentBeamPrefab;
    private GameObject intentBeamInstance;

    public override IEnumerator Execute(FightingEntity target)
    {
        passiveSkills = GetComponents<PassiveSkill>().ToList();
        entity = this.GetComponent<FightingEntity>();
        if (isProjectileAttack)
        {
            yield return StartCoroutine(PerformProjectileAttack(target));
        }
        else
        {
            yield return StartCoroutine(PerformPhysicalAttack(target));
        }
    }
    public int GetAttackPower()
    {
        int final = damage;

        foreach (var passive in passiveSkills)
            final = passive.ModifyAttack(final);

        return final;
    }

    public IEnumerator PerformPhysicalAttack(FightingEntity target)
    {

        if (target == null) yield break;

        animator.SetTrigger("Action");

        BattleManager.Instance.SelectHero(null);

        Vector3 originalPosition = transform.position;

        // Move toward target (0.25s)
        yield return MoveToPosition(target.transform.position, 0.25f);
        int accuracy = 100;
        //LowerAccuracyStatus accStatus = GetComponent<LowerAccuracyStatus>();
        //if (accStatus != null)
        //    accuracy -= accStatus.accuracyPenalty;
        // Deal damage
        target.TakeDamage(Mathf.RoundToInt(GetAttackPower() * entity.attackPower * 0.01f), element, accuracy);
        if (hitSound)
            EffectsManager.instance.CreateSoundEffect(hitSound, Vector3.zero);
        if (statusEffect != null)
        {
            int roll = Random.Range(0, 100);
            if (roll < chanceToProc)
            {
                target.AddStatusEffect(statusEffect, target.attackPower);
            }
        }

        yield return MoveToPosition(originalPosition, 0.25f);
        yield return StartCoroutine(target.ResolveDeathIfNeeded());
    }
    public IEnumerator PerformProjectileAttack(FightingEntity target)
    {
        if (target == null) yield break;
        castEventTriggered = false;

        // Play casting animation
        animator.SetTrigger("StartCast");

        // Wait for animation event OR fallback timeout
        float maxWait = 1.35f;   // fallback
        float timer = 0f;

        while (!castEventTriggered && timer < maxWait)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Spawn projectile
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            if (originSound)
                EffectsManager.instance.CreateSoundEffect(originSound, Vector3.zero);

            Vector3 start = projectileSpawnPoint.position;
            Vector3 end = target.transform.position;

            float travelTime = Vector3.Distance(start, end) / projectileSpeed;
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / travelTime;
                proj.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            // Hit target
            Destroy(proj);

            int accuracy = baseAccuracy;
            //LowerAccuracyStatus accStatus = GetComponent<LowerAccuracyStatus>();
            //if (accStatus != null)
            //    accuracy -= accStatus.accuracyPenalty;

            target.TakeDamage(Mathf.RoundToInt(GetAttackPower() * entity.attackPower * 0.01f), element, accuracy);
            if (hitSound)
                EffectsManager.instance.CreateSoundEffect(hitSound, Vector3.zero);

            // Proc status effects
            if (statusEffect != null)
            {
                int roll = Random.Range(0, 100);
                if (roll < chanceToProc)
                {
                    target.AddStatusEffect(statusEffect, entity.attackPower);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(target.ResolveDeathIfNeeded());
    }

    private IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }
    public void OnProjectileCastEvent()
    {
        castEventTriggered = true;
    }
    public override void ShowIntentIndicators(List<FightingEntity> targets)
    {
        if (targets == null || targets.Count == 0)
            return;

        if (intentBeamInstance != null || !intentBeamPrefab)
            return;

        FightingEntity target = targets[0];
        if (target == null)
            return;

        intentBeamInstance = Instantiate(
            intentBeamPrefab,
            transform.position,
            Quaternion.identity
        );

        BeamController beam = intentBeamInstance.GetComponent<BeamController>();
        if (beam != null)
        {
            beam.PositionBeam(
                transform.position,
                target.transform.position
            );
        }

        float length = Vector3.Distance(transform.position, target.transform.position);
        beam.SetArrowPoperties(Color.red, length);
    }

    public override void HideIntentIndicators()
    {
        if (intentBeamInstance != null)
        {
            Destroy(intentBeamInstance);
            intentBeamInstance = null;
        }
    }
}
