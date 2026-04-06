using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HeroEntity : FightingEntity
{
    public ElementType mainElement;
    public int spellPower = 100;
    
    public bool isDefeated = false;
    public override void Initialize()
    {
        base.Initialize();
        UpdateVisuals();
    }
    public override void UpdateVisuals()
    {
        base.UpdateVisuals();
    }
    protected override void HandleLeftClick()
    {
        BattleManager.Instance.SelectTarget(this);
    }
    public override void SelectAsAttacker()
    {
        base.SelectAsAttacker();
        BattleManager.Instance.SelectHero(this);
        Debug.Log("Hero Selected");
    }
    protected override IEnumerator HandleDestruction()
    {
        base.HandleDestruction();
        BattleManager.Instance.SetPlayerInput(false);

        yield return new WaitForSeconds(0.5f);

        PlayerHand.instance.RemoveCardsOfType(mainElement);
        BattleManager.Instance.RemoveElementFromDeck(mainElement);

        isDefeated = true;
        animator.Play("DefeatFall");
        BattleManager.Instance.SetPlayerInput(true);
    }

    public void Revive()
    {
        animator.Play("Idle");
        isDefeated = false;
        currentHealth = maxHealth / 2;
        BattleManager.Instance.AddElementToDeck(mainElement);
    }
}
