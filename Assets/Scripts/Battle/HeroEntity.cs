using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HeroEntity : FightingEntity
{
    public ElementType mainElement;
    public int spellPower = 100;
    
    public bool isDefeated = false;

    [Header("References")]
    public EquipmentSystem equipmentSystem;

    public override void Initialize()
    {
        base.Initialize();

        try
        {
            var cardSprite = GetCardVisual();
            if (cardSprite != null && equipmentSystem != null)
            {
                equipmentSystem.SetOwnerCardSprite(cardSprite);
            }
        }
        catch
        {

        }

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
    public void ApplyData(CharacterData data)
    {
        maxHealth = data.maxHP;
        SetHealth(data.currentHP);
        spellPower = data.spellPower;
    }
    public void SetData()
    {
        CharacterData data = PlayerDataManager.instance.GetHeroData(Guid);
        if (data == null)
            data = PlayerDataManager.instance.AddToParty(this);
        data.maxHP = maxHealth;
        data.currentHP = currentHealth;
        data.spellPower = spellPower;
    }
}
