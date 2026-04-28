using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine;

namespace Inventory.UI
{
    public class UIInventoryDescription : MonoBehaviour
    {
        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text description;

        [SerializeField] UIInventoryPage page;

        public void Awake()
        {
            ResetDescription();
        }

        public void ResetDescription()
        {
            // Use UnityEngine.Object null checks so destroyed native objects evaluate to null safely
            if (itemImage != null)
            {
                if (itemImage.gameObject != null)
                    itemImage.gameObject.SetActive(false);
            }

            if (title != null)
                title.text = "";

            if (description != null)
                description.text = "";
            page.ClearSpellIcons();
        }

        public void SetDescription(Sprite sprite, string itemName, string itemDescription)
        {
            
            if (itemImage != null && itemImage.gameObject != null)
            {
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = sprite;
            }

            if (title != null)
                title.text = itemName ?? "";

            if (description != null)
                description.text = itemDescription ?? "";
        }
    }
}