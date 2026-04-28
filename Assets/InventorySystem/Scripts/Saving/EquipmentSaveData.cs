using System;
using System.Collections.Generic;
using Inventory.Model;

[Serializable]
public class EquipmentSaveData
{
    public EquipmentSlot slot;

    public string itemID;

    public List<ItemParameter> itemState;
}