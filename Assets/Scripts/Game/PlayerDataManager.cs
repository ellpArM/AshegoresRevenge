using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;


public enum StatType
{
    MaxHP,
    SpellPower,
    Defense,
    Strength,
    Speed
}

[System.Serializable]
public class StatModifier
{
    public StatType stat;
    public int value;
}

public class CharacterData
{
    public Dictionary<StatType, int> stats = new Dictionary<StatType, int>();

    public int baseMaxHP;
    public int baseSpellPower;
    public int maxHP;
    public int currentHP;
    public int spellPower;

    public EquipmentSystem equipmentSystem;
    public List<GameObject> spells = new();

    public void RefreshStats()
    {
        maxHP = baseMaxHP;
        spellPower = baseSpellPower;

        Dictionary<EquipmentSlot, EquippableItemSO> equippedItems = equipmentSystem.GetEquipment();
        foreach (var item in equippedItems.Values)
        {
            if (item == null)
                continue;

            foreach (var mod in item.statModifiers)
            {
                switch (mod.stat)
                {
                    case StatType.MaxHP:
                        maxHP += mod.value;
                        break;

                    case StatType.SpellPower:
                        spellPower += mod.value;
                        break;
                }
            }

            if (item is SpellbookSO spellbook && spellbook.spells != null)
            {
                spells.AddRange(spellbook.spells);
            }
        }

        currentHP = Mathf.Min(currentHP, maxHP);
    }
}

[System.Serializable]
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;
    public Dictionary<string, CharacterData> party = new();
    public CharacterData AddToParty(HeroEntity hero)
    {
        party.Add(hero.Guid, new CharacterData() { maxHP = hero.MaxHealth, 
            baseMaxHP = hero.MaxHealth, 
            currentHP = hero.maxHealth, 
            spellPower = hero.spellPower, 
            baseSpellPower = hero.spellPower, 
            equipmentSystem = hero.equipmentSystem});
        hero.equipmentSystem.SetOwnerCardSprite(hero.GetCardVisual());
        return party[hero.Guid];
    }
    public CharacterData GetHeroData(string guid)
    {
        return party[guid];
    }
}
