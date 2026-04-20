using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using Inventory.UI;
using Inventory.Model;
using System.Text;
using UnityEngine;
using System.Linq;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] UIInventoryPage inventoryPage;

        [SerializeField] GameObject player;

        [SerializeField] InventorySO inventoryData;

        [SerializeField] List<EquipmentSystem> equipmentSystem = new List<EquipmentSystem>();

        public List<InventoryItem> initialItems = new List<InventoryItem>();

        public EquipmentSystem curEquipment;

        public void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            inventoryData.RemoveActionMenu += RemoveActionMenu;
            foreach(EquipmentSystem sys in equipmentSystem) sys.UpdateInventory += UpdateEquipmentUI;
            
            foreach(InventoryItem item in initialItems)
            {
                if (item.IsEmpty) continue;
                inventoryData.AddItem(item);
            }
        }

        private void UpdateEquipmentUI(EquipmentSlot slot, ItemSO item)
        {
            inventoryPage.UpdateEquipmentUI(slot, item);
        }

        private void RemoveActionMenu()
        {
            inventoryPage.ResetSelection();
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryPage.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryPage.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryPage.InitializeInventoryUI(inventoryData.Size);
            inventoryPage.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryPage.OnSwapItems += HandleSwapItems;
            inventoryPage.OnStartDragging += HandleDragging;
            inventoryPage.OnItemActionRequested += HandleItemActionRequest;

            // subscribe to equipment UI events
            inventoryPage.OnEquipmentDescriptionRequested += HandleEquipmentDescriptionRequest;
            inventoryPage.OnEquipmentActionRequested += HandleEquipmentActionRequest;
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty) return;
            
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                inventoryPage.ShowItemAction(itemIndex);
                inventoryPage.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));

            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryPage.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
            }
        }

        private void DropItem(int itemIndex, int quantity)
        {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryPage.ResetSelection();
            //Put some audio here
        }

        public void PerformAction(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty) return;
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(curEquipment, inventoryItem.itemState);
            }
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty) return;
            inventoryPage.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem invItem = inventoryData.GetItemAt(itemIndex);
            if (invItem.IsEmpty)
            {
                inventoryPage.ResetSelection();
                return;
            }

            ItemSO item = invItem.item;
            string description = PrepareDescription(invItem);
            inventoryPage.UpdateDescription(itemIndex, item.ItemImage, item.name, description);


        }

        // New: handle equipment slot left-click -> show description
        private void HandleEquipmentDescriptionRequest(EquipmentSystem eqSystem, EquipmentSlot slot)
        {
            EquippableItemSO item = eqSystem.GetEquippedItem(slot);
            if (item == null)
            {
                inventoryPage.ResetSelection();
                return;
            }

            // simple description for equipped item (no itemState access here)
            string desc = item.Description;
            inventoryPage.UpdateEquipmentDescription(item.ItemImage, item.Name, desc);
        }

        // New: handle equipment slot right-click -> show actions (Unequip / Drop)
        private void HandleEquipmentActionRequest(EquipmentSystem eqSystem, EquipmentSlot slot, Transform anchor)
        {
            EquippableItemSO item = eqSystem.GetEquippedItem(slot);
            if (item == null) return;

            // position action panel at the clicked equipment UI slot
            inventoryPage.ShowActionAt(anchor);

            // add Unequip action
            inventoryPage.AddAction("Unequip", () =>
            {
                eqSystem.Unequip(slot);
                // clear UI slot immediately so visuals reflect the change
                inventoryPage.ClearEquipmentSlot(slot);
                inventoryPage.ResetSelection();
            });

            // add Drop action: Unequip (which moves item into inventory) then remove the newly added instance
            inventoryPage.AddAction("Drop", () =>
            {
                // remember item reference
                ItemSO itemRef = item;
                // Unequip moves item into inventory
                eqSystem.Unequip(slot);

                // Try to find first inventory slot that contains this item and remove one quantity.
                int foundIndex = FindFirstInventoryIndexOfItem(itemRef);
                if (foundIndex != -1)
                {
                    inventoryData.RemoveItem(foundIndex, 1);
                }

                // clear UI slot immediately
                inventoryPage.ClearEquipmentSlot(slot);
                inventoryPage.ResetSelection();
            });
        }

        // Public: refresh all equipment UI boxes for the given equipment system (used when opening a tab)
        public void RefreshEquipmentTab(EquipmentSystem eq)
        {
            if (eq == null) return;

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                ItemSO item = eq.GetEquippedItem(slot);
                inventoryPage.UpdateEquipmentUI(slot, item);
            }
        }

        // helper to find first inventory index matching an item
        private int FindFirstInventoryIndexOfItem(ItemSO item)
        {
            var state = inventoryData.GetCurrentInventoryState();
            foreach (var kv in state)
            {
                if (kv.Value.item != null && kv.Value.item.ID == item.ID)
                    return kv.Key;
            }
            return -1;
        }

        private string PrepareDescription(InventoryItem inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();
            for (int i = 0; i < inventoryItem.itemState.Count; i++)
            {
                sb.Append($"{inventoryItem.itemState[i].itemParameter.parameterName} " + $": {inventoryItem.itemState[i].value} " + $"/ {inventoryItem.item.DefaultParametersList[i].value}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("OPENED INVENTORY");
                if (inventoryPage.isActiveAndEnabled == false)
                {
                    inventoryPage.Show();
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryPage.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
                    }
                }
                else inventoryPage.Hide();
            }
        }
    }
}