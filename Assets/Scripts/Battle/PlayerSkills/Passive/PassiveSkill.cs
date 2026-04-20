using System;
using System.Collections;
using UnityEngine;

public abstract class PassiveSkill : MonoBehaviour
{
    protected FightingEntity owner;
    [TextArea(4,4)]
    public string castDescription;
    private void Start()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        owner = GetComponent<FightingEntity>();
    }
    public virtual string GetDescription()
    {
        return "";
    }

    public virtual void OnApply() { }

    public virtual int ModifyAttack(int value) => value;
    public virtual int ModifyDefense(int value) => value;
    public virtual IEnumerator OnTurnStart() { yield break;  }

    public virtual void OnReceiveDamage(ref int dmg, ElementType dmgType, FightingEntity owner) { }

    public virtual string GetDescription(int spellPower)
    {
        return "";
    }

    public virtual void InitializeFromCaster(HeroEntity mainHero) { }
}
