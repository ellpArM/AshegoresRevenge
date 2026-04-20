using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace Inventory.UI
{
    public class UIInventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text quantityTxt;
        [SerializeField] private Image borderImage;

        public event Action<UIInventoryItem> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClick;

        private bool empty = true;

        public void Awake()
        {
            ResetData();
            Deselect();
        }
        public void ResetData()
        {
            // guard against destroyed/missing UI components
            if (itemImage != null)
            {
                // UnityEngine.Object equality handles destroyed objects in editor/runtime
                if (itemImage.gameObject != null)
                    itemImage.gameObject.SetActive(false);
            }

            if (quantityTxt != null)
                quantityTxt.text = "0";

            empty = true;
        }
        public void Deselect()
        {
            if (borderImage != null)
                borderImage.enabled = false;
        }
        public void SetData(Sprite sprite, int quantity)
        {
            if (sprite == null)
            {
                // ensure we don't access destroyed objects
                if (itemImage != null && itemImage.gameObject != null)
                    itemImage.gameObject.SetActive(false);

                if (quantityTxt != null)
                    quantityTxt.text = "0";

                empty = true;
                return;
            }

            if (itemImage != null && itemImage.gameObject != null)
            {
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = sprite;
                itemImage.preserveAspect = true;
            }

            if (quantityTxt != null)
                quantityTxt.text = quantity.ToString();

            empty = false;
        }

        public void Select()
        {
            if (borderImage != null)
                borderImage.enabled = true;
        }

        public void OnPointerClick(PointerEventData pointerData)
        {
            if (pointerData.button == PointerEventData.InputButton.Right)
            {
                OnRightMouseBtnClick?.Invoke(this);
            }
            else
            {
                OnItemClicked?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (empty)
                return;
            OnItemBeginDrag?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnItemEndDrag?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            OnItemDroppedOn?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {

        }
    }
}