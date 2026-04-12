using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Inventory.Model;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField] UIInventoryItem itemPrefab;

        [SerializeField] RectTransform contentPanel;
        [SerializeField] UIInventoryDescription itemDescription;
        [SerializeField] MouseFollower mouseFollower;
        [SerializeField] ItemActionPanel actionPanel;

        [SerializeField] GameObject tabs;
        [SerializeField] GameObject information;

        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
        public event Action<int, int> OnSwapItems;

        [SerializeField] List<UIInventoryItem> equipmentUI;
        private Dictionary<EquipmentSlot, int> equipmentSlots = new Dictionary<EquipmentSlot, int>();
        [SerializeField] EquipmentSystem equipmentSystem;

        private void Awake()
        {
            equipmentSlots[EquipmentSlot.Wand] = 0;
            equipmentSlots[EquipmentSlot.Spellbook] = 1;
            equipmentSlots[EquipmentSlot.Hat] = 2;
            equipmentSlots[EquipmentSlot.Robes] = 3;
            equipmentSlots[EquipmentSlot.Amulet] = 4;
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }

        public void UpdateEquipmentUI(EquipmentSlot slot, ItemSO item)
        {
            
            equipmentUI[equipmentSlots[slot]].SetData(item.ItemImage, 0);
        }

        public void InitializeInventoryUI(int inventorySize)
        {
            listOfUIItems.Clear();
            foreach (Transform child in contentPanel.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < inventorySize; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(contentPanel);
                uiItem.transform.localScale = new Vector3(1, 1, 1);
                listOfUIItems.Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDroppedOn += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            }
            foreach (Transform child in tabs.transform)
            {
                UITabs c = child.GetComponent<UITabs>();
                c.OnTabClicked += SwitchTab;
            }
        }

        private void SwitchTab(GameObject tab, GameObject button)
        {
            foreach (Transform child in tabs.transform)
            {
                ChangeBrightness(child, 1.00f);
            }
            foreach (Transform child in information.transform)
            {
                child.gameObject.SetActive(false);
            }
            tab.SetActive(true);
            ChangeBrightness(button.transform, 0.65f);
        }

        private void ChangeBrightness(Transform obj,float val)
        {
            Image im = obj.GetComponent<Image>();
            if (im == null) return;
            Color c = im.color;
            float h, s, v;
            Color.RGBToHSV(c, out h, out s, out v);
            v = val;
            c = Color.HSVToRGB(h, s, v);
            im.color = c;
        }

        internal void ResetAllItems()
        {
            foreach(var item in listOfUIItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }

        internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
        {
            itemDescription.SetDescription(itemImage, name, description);
            DeselectAllItems();
            listOfUIItems[itemIndex].Select();
        }

        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            ResetDraggedItem();
        }

        private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        public void UpdateData(int itemIndex, Sprite sprite, int quantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(sprite, quantity);
            }
        }


        private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1) return;
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1) return;
            OnDescriptionRequested?.Invoke(index);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            itemDescription.ResetDescription();

            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }

        public void AddAction(string actionName, Action performAction)
        {
            actionPanel.AddButton(actionName, performAction);
        }

        public void ShowItemAction(int itemIndex)
        {
            actionPanel.Toggle(true);
            actionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
            actionPanel.Toggle(false);
        }

        public void Hide()
        {
            actionPanel.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();
        }
    }
}