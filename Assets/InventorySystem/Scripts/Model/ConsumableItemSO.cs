using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using Inventory.UI;
using Inventory.Model;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "ConsumableItemSO", menuName = "Items/ConsumableItemSO")]
    public class ConsumableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField] private List<ModifierData> modifiersData = new List<ModifierData>();
        [SerializeField]
        public string ActionName => "Consume";
        [field: SerializeField]
        public AudioClip actionSFX {get; private set;}

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            foreach (ModifierData data in modifiersData)
            {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;
        }
    }

    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX  { get; }
        bool PerformAction(GameObject character, List<ItemParameter> itemState);
    }

    [Serializable]

    public class ModifierData
    {
        public CharacterStatModifierSO statModifier;
        public float value;
    }
}

