using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public List<InventoryItemSaveData> inventory;

    public List<EquipmentSaveData> equipment;
}