using System;
using System.Collections.Generic;
using Inventory.Model;

[Serializable]
public class InventoryItemSaveData
{
    public int slotIndex;

    public string itemID;

    public int quantity;

    public List<ItemParameter> itemState;
}