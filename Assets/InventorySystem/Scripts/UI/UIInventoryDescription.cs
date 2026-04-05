using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace Inventory.UI
{
    public class UIInventoryDescription : MonoBehaviour
    {
        [SerializeField] Image itemImage;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text description;

        public void Awake()
        {
            ResetDescription();
        }

        public void ResetDescription()
        {
            itemImage.gameObject.SetActive(false);
            title.text = "";
            description.text = "";
        }

        public void SetDescription(Sprite sprite, string itemName, string itemDescription)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            title.text = itemName;
            description.text = itemDescription;
        }
    }
}