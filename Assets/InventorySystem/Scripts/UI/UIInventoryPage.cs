using Inventory.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField] UIInventoryItem itemPrefab;

        [SerializeField] RectTransform contentPanel;
        [SerializeField] UIInventoryDescription itemDescription;
        [SerializeField] MouseFollower mouseFollower;
        [SerializeField] ItemActionPanel actionPanel;
        [SerializeField] InventoryController inventoryController;

        [SerializeField] GameObject tabs;
        [SerializeField] GameObject information;

        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
        public event Action<int, int> OnSwapItems;

        // new events for equipment UI interactions
        public event Action<EquipmentSystem, EquipmentSlot> OnEquipmentDescriptionRequested;
        public event Action<EquipmentSystem, EquipmentSlot, Transform> OnEquipmentActionRequested;

        [SerializeField] List<UIInventoryItem> equipmentUI1;
        [SerializeField] List<UIInventoryItem> equipmentUI2;
        [SerializeField] List<UIInventoryItem> equipmentUI3;
        [SerializeField] EquipmentSystem pg1;
        [SerializeField] EquipmentSystem pg2;
        [SerializeField] EquipmentSystem pg3;
        [SerializeField] Image equipUI1Image;
        [SerializeField] Image equipUI2Image;
        [SerializeField] Image equipUI3Image;
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
            EquipmentSystem curEquip = inventoryController.curEquipment;
            Debug.Log(curEquip);

            int idx = equipmentSlots[slot];

            // If item is null, clear the UI slot
            if (item == null)
            {
                if (curEquip == pg1 && equipmentUI1 != null && idx < equipmentUI1.Count) equipmentUI1[idx].ResetData();
                else if (curEquip == pg2 && equipmentUI2 != null && idx < equipmentUI2.Count) equipmentUI2[idx].ResetData();
                else if (curEquip == pg3 && equipmentUI3 != null && idx < equipmentUI3.Count) equipmentUI3[idx].ResetData();
                return;
            }

            if (curEquip == pg1 && equipmentUI1 != null && idx < equipmentUI1.Count) equipmentUI1[idx].SetData(item.ItemImage, 0);
            else if (curEquip == pg2 && equipmentUI2 != null && idx < equipmentUI2.Count) equipmentUI2[idx].SetData(item.ItemImage, 0);
            else if (curEquip == pg3 && equipmentUI3 != null && idx < equipmentUI3.Count) equipmentUI3[idx].SetData(item.ItemImage, 0);
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

            // Wire equipment UI lists so they behave like inventory items
            SetupEquipmentUI(equipmentUI1, pg1);
            SetupEquipmentUI(equipmentUI2, pg2);
            SetupEquipmentUI(equipmentUI3, pg3);
        }

        public void RefreshEquipmentBackground()
        {
            EquipmentSystem curEquip = inventoryController.curEquipment;
            if (curEquip == pg1 && equipmentUI1 != null) equipUI1Image.sprite = curEquip.OwnerCardSprite;
            else if (curEquip == pg2 && equipmentUI2 != null) equipUI2Image.sprite = curEquip.OwnerCardSprite;
            else if (curEquip == pg3 && equipmentUI3 != null) equipUI3Image.sprite = curEquip.OwnerCardSprite;
        }

        private void SetupEquipmentUI(List<UIInventoryItem> uiList, EquipmentSystem eqSystem)
        {
            if (uiList == null || uiList.Count == 0 || eqSystem == null) return;

            // attach handlers for each equipment UI slot
            foreach (var uiItem in uiList)
            {
                // capture local reference to avoid closure issues
                UIInventoryItem localItem = uiItem;
                localItem.OnItemClicked += (it) => HandleEquipmentSelection(localItem, uiList, eqSystem);
                localItem.OnRightMouseBtnClick += (it) => HandleEquipmentShowActions(localItem, uiList, eqSystem);
            }
        }

        private void SwitchTab(GameObject tab, GameObject button, EquipmentSystem eS)
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

            // set current equipment system on controller
            if (eS != null)
                inventoryController.curEquipment = eS;

            // request a full UI refresh for the just-opened equipment tab
            inventoryController.RefreshEquipmentTab(eS);
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

        // New: show action panel anchored to an arbitrary transform (used for equipment slots)
        public void ShowActionAt(Transform anchorTransform)
        {
            if (anchorTransform == null) return;
            actionPanel.Toggle(true);
            actionPanel.transform.position = anchorTransform.position;
        }

        // New: used by InventoryController to set description for equipment slots
        public void UpdateEquipmentDescription(Sprite itemImage, string name, string description)
        {
            itemDescription.SetDescription(itemImage, name, description);
        }

        // Public helper to clear an equipment UI slot (used when unequipping/dropping)
        public void ClearEquipmentSlot(EquipmentSlot slot)
        {
            int idx = equipmentSlots[slot];
            EquipmentSystem curEquip = inventoryController.curEquipment;

            if (curEquip == pg1 && equipmentUI1 != null && idx < equipmentUI1.Count) equipmentUI1[idx].ResetData();
            else if (curEquip == pg2 && equipmentUI2 != null && idx < equipmentUI2.Count) equipmentUI2[idx].ResetData();
            else if (curEquip == pg3 && equipmentUI3 != null && idx < equipmentUI3.Count) equipmentUI3[idx].ResetData();
        }

        // equipment click handler: show description via event then update selection visuals
        private void HandleEquipmentSelection(UIInventoryItem inventoryItemUI, List<UIInventoryItem> uiList, EquipmentSystem eqSystem)
        {
            int index = uiList.IndexOf(inventoryItemUI);
            if (index == -1) return;

            // find slot enum from index mapping
            EquipmentSlot slot = equipmentSlots.First(kv => kv.Value == index).Key;

            // notify controller to show description for this equipment slot
            OnEquipmentDescriptionRequested?.Invoke(eqSystem, slot);

            // visual selection
            DeselectAllItems();
            inventoryItemUI.Select();
        }

        // equipment right-click handler: request action menu (unequip/drop) and provide anchor transform
        private void HandleEquipmentShowActions(UIInventoryItem inventoryItemUI, List<UIInventoryItem> uiList, EquipmentSystem eqSystem)
        {
            int index = uiList.IndexOf(inventoryItemUI);
            if (index == -1) return;

            EquipmentSlot slot = equipmentSlots.First(kv => kv.Value == index).Key;

            // ask controller to populate actions for this equipment slot
            OnEquipmentActionRequested?.Invoke(eqSystem, slot, inventoryItemUI.transform);
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                // UIInventoryItem overrides Unity's == so destroyed objects evaluate to null
                if (item != null)
                    item.Deselect();
            }

            // also deselect equipment UI items
            void DeselectList(List<UIInventoryItem> list)
            {
                if (list == null) return;
                foreach (var it in list)
                    if (it != null)
                        it.Deselect();
            }
            DeselectList(equipmentUI1);
            DeselectList(equipmentUI2);
            DeselectList(equipmentUI3);

            // Guard calls to actionPanel so we don't call into a destroyed UI component
            if (actionPanel != null)
                actionPanel.Toggle(false);
        }

        public void Hide()
        {
            if (actionPanel != null)
                actionPanel.Toggle(false);

            // Guard gameObject access too (rare, but defensive)
            if (this != null && gameObject != null)
                gameObject.SetActive(false);

            ResetDraggedItem();
        }
    }
}