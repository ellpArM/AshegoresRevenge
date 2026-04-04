using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField] UIInventoryItem itemPrefab;

    [SerializeField] RectTransform contentPanel;
    [SerializeField] UIInventoryDescription itemDescription;
    [SerializeField] MouseFollower mouseFollower;

    List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem> ();

    private int currentlyDraggedItemIndex = -1;

    public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
    public event Action<int, int> OnSwapItems;

    private void Awake()
    {
        Hide();
        mouseFollower.Toggle(false);
        itemDescription.ResetDescription();
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
    }

    private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
    {

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
        CreateDraggedItem(index);
    }

    private void CreateDraggedItem(int index)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(index == 0 ? image : image2, quantity);
    }

    private void HandleItemSelection(UIInventoryItem inventoryItemUI)
    {
        itemDescription.SetDescription(image, title, description);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        itemDescription.ResetDescription();

        listOfUIItems[0].SetData(image, quantity);
        listOfUIItems[1].SetData(image2, quantity);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
