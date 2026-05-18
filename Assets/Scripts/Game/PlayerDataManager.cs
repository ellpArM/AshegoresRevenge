using Inventory.Model;
using System;
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
    public int level;
    public int experience;
    public int skillPoints;

    public EquipmentSystem equipmentSystem;
    public List<GameObject> spells = new();
    public List<BaseSkill> equippedSkills = new();
    public SkillTreeSO skillTree;

    // New: runtime unlocked node ids for this character (persisted in memory / save system can persist as needed)
    public HashSet<string> unlockedNodeIds = new();

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

    public void AddExperience(int amount)
    {
        experience += amount;

        // Calculate next level requirement relative to current level
        int nextLevelExp = (level + 1) * 100;

        // Handle multiple level-ups if experience is large
        while (experience >= nextLevelExp)
        {
            experience -= nextLevelExp;
            level++;
            skillPoints += 1;

            // Auto-unlock tier branches according to new level thresholds
            if (skillTree != null)
                skillTree.HandleLevelUnlocks(this, level);

            // Recompute next level requirement for the new level
            nextLevelExp = (level + 1) * 100;
        }

        // Implement level up effects etc.
    }

    public bool EquipSkill(BaseSkill skill)
    {
        if (equippedSkills.Count <= 5)
        {
            equippedSkills.Add(skill);
            return true;
        }
        return false;
    }

    public void UnequipSkill(BaseSkill baseSkill)
    {
        if (equippedSkills.Contains(baseSkill))
        {
            equippedSkills.Remove(baseSkill);
        }
    }
}

[System.Serializable]
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;
    public Dictionary<string, CharacterData> party = new();

    public CharacterData AddToParty(HeroEntity hero)
    {
        var data = new CharacterData()
        {
            maxHP = hero.MaxHealth,
            baseMaxHP = hero.MaxHealth,
            currentHP = hero.maxHealth,
            spellPower = hero.spellPower,
            baseSpellPower = hero.spellPower,
            equipmentSystem = hero.equipmentSystem,
            level = 0,
            experience = 0,
            skillPoints = 0,
            equippedSkills = hero.AvailableSkills,
            skillTree = hero.skillTree
        };

        Debug.Log(hero.AvailableSkills.Count);
        // Initialize unlocked node ids from the skill tree's default unlocked flags (e.g. base nodes).
        if (hero.skillTree != null && hero.skillTree.nodes != null)
        {
            foreach (var node in hero.skillTree.nodes)
            {
                if (node.unlocked)
                    data.unlockedNodeIds.Add(node.id);
            }
        }

        party.Add(hero.Guid, data);
        hero.equipmentSystem.SetOwnerCardSprite(hero.GetCardVisual());
        hero.skillTree.heroSprite = hero.GetCardVisual();
        return party[hero.Guid];
    }
    public CharacterData GetHeroData(string guid)
    {
        return party[guid];
    }
}
