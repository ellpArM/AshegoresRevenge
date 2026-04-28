using System;
using System.Collections.Generic;

[Serializable]
public class HeroEquipmentSaveData
{
    public string heroID;

    public List<EquipmentSaveData> equipment;
}