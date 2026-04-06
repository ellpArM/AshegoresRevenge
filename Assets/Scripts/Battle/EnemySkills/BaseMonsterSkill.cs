using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseMonsterSkill : MonoBehaviour
{
    public string skillName;
    public string skillDescription;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    protected FightingEntity entity;

    public abstract IEnumerator Execute(FightingEntity target);
    public IEnumerator PostExecute()
    {
        yield return null;
        //yield return SpiritLinkManager.Instance.ResolveAll();
    }

    public virtual List<FightingEntity> SelectTargets(List<FightingEntity> playerCards, List<FightingEntity> enemyCards)
    {
        // Default behavior: pick one random player
        return new List<FightingEntity>
        {
            playerCards[Random.Range(0, playerCards.Count)]
        };
    }

    public virtual IEnumerator ExecuteWithTargets(List<FightingEntity> targets)
    {
        // Default: assume one target
        if (targets == null || targets.Count == 0)
            yield break;

        yield return Execute(targets[0]);
    }

    public virtual void ShowIntentIndicators(List<FightingEntity> targets)
    {
        // Default: highlight targets red
        foreach (var t in targets)
            t?.ShowSelector(SelectionState.Active);
    }

    public virtual void HideIntentIndicators()
    {
        // Default: clear all indicators
        foreach (var card in FindObjectsByType<FightingEntity>(FindObjectsSortMode.None))
            card.HideSelector();
    }

}
