using System.Collections;
using UnityEngine;

public class FreezeEffect : StatusEffect
{
    //public int speedPenalty;
    private int originalSpeed;
    public override void Initialize(FightingEntity targetUnit, StatusEffect origin, int power)
    {
        base.Initialize(targetUnit, origin, power);
        EffectsManager.instance.CreateFloatingText(target.transform.position, "Frozen", Color.black);
    }
    public override string GetDescription()
    {
        return $"Frozen\n turns left: {duration}";
    }
    public override IEnumerator OnTurnStartCoroutine()
    {
        // no effect here skip 
        duration--;
        if (duration <= 0)
        {
            OnExpire();
            Destroy(this);
        }
        yield return null;
    }
    public override void Reapply(StatusEffect newEffect, int power)
    {
        // do nothing freeze effects don't stack
    }

    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
