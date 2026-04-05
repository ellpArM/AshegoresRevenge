using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using Inventory.UI;
using Inventory.Model;
using System.Text;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] UIInventoryPage inventoryPage;

        [SerializeField] GameObject player;

        [SerializeField] InventorySO inventoryData;

        public List<InventoryItem> initialItems = new List<InventoryItem>();

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
            foreach(InventoryItem item in initialItems)
            {
                if (item.IsEmpty) continue;
                inventoryData.AddItem(item);
            }
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
                itemAction.PerformAction(player, inventoryItem.itemState);
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