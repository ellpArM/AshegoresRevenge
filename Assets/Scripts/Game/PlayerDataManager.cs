using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public int maxHP;
    public int currentHP;
    public int spellPower;

    //public List<EquipmentData> equipment;
}
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;
    public Dictionary<string, CharacterData> party = new();
    public CharacterData AddToParty(HeroEntity hero)
    {
        party.Add(hero.Guid, new CharacterData() { maxHP = hero.MaxHealth, currentHP = hero.maxHealth, spellPower = hero.spellPower});
        return party[hero.Guid];
    }
    public CharacterData GetHeroData(string guid)
    {
        return party[guid];
    }
}
