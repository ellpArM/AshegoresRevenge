using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "SpellbookSO", menuName = "Items/SpellbookSO")]
    public class SpellbookSO : EquippableItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private EquipmentSlot slot = EquipmentSlot.Spellbook;



        public bool PerformAction(EquipmentSystem weaponSystem, List<ItemParameter> itemState = null)
        {
            if (weaponSystem != null)
            {
                weaponSystem.SetWeapon(this, itemState == null ? DefaultParametersList : itemState);
                return true;
            }
            return false;
        }
    }
}
