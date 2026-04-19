using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "SpellbookSO", menuName = "Items/SpellbookSO")]
    public class SpellbookSO : EquippableItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private EquipmentSlot slot = EquipmentSlot.Spellbook;

        public List<GameObject> spells;
    }
}
