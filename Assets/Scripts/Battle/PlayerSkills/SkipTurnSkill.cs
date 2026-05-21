using System.Collections;
using UnityEngine;

public class SkipTurnSkill : BaseSkill
{
    FightingEntity owner;
    private void Start()
    {
        owner = transform.root.GetComponent<HeroEntity>();
    }
    public override IEnumerator Execute()
    {
        owner.RecoverMana(5);
        BattleManagerNew.Instance.NotifyPlayerSkillUsed();
        yield return null;
    }
}
