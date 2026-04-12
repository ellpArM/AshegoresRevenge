using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "EquippableItemSO", menuName = "Items/EquippableItemSO")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        public string ActionName => "Equip";

        [SerializeField]
        private EquipmentSlot slot;
        public EquipmentSlot Slot => slot;

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            EquipmentSystem weaponSystem = character.GetComponent<EquipmentSystem>();
            if (weaponSystem != null)
            {
                weaponSystem.SetWeapon(this, itemState == null ? DefaultParametersList : itemState);
                return true;
            }
            return false;
        }
    }
}

