using System.Collections;
using UnityEngine;

public class MightPassive : PassiveSkill
{
    public int duration = 0;
    void Start()
    {
        owner = GetComponent<FightingEntity>();
        if (owner != null)
            owner.AddPassive(this);
        if (duration == 0)
            duration = 2;
    }
    void Update()
    {

    }
    public override IEnumerator OnTurnStart()
    {
        duration--;
        if (duration <= 0)
        {
            owner.RemovePassive(this);
            Destroy(this);
        }
        yield return null;
    }
    public override int ModifyAttack(int value)
    {
        return Mathf.RoundToInt(value * 1.5f);
    }
    public override string GetDescription(int spellPower)
    {
        return "Might";
    }
}
