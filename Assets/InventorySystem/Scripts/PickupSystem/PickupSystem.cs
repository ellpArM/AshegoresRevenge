using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using Inventory.UI;
using Inventory.Model;

public class PickupSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ITEM PICKUP");
        Item item = other.GetComponent<Item>();
        
        if (item != null)
        {
            int remainder = inventoryData.AddItem(item.InventoryItem, item.Quantity);
            if (remainder == 0)
                item.DestroyItem();
            else
                item.Quantity = remainder;
        }
    }

}
