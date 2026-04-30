using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventory;

    [SerializeField]
    private List<EquipmentSystem> equipmentSystems;

    [SerializeField]
    private ItemDatabase itemDatabase;

    private void Awake()
    {
        itemDatabase.Initialize();
    }

    private void Start()
    {
        inventory.Initialize();
    }

    //TEMP LOAD KEYS
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SaveGame();

        if (Input.GetKeyDown(KeyCode.F9))
            LoadGame();
    }

    public void SaveGame()
    {
        PlayerSaveData data = new PlayerSaveData();

        data.inventory = inventory.GetSaveData();

        data.heroEquipment = new List<HeroEquipmentSaveData>();

        foreach (var system in equipmentSystems)
        {
            data.heroEquipment.Add(
                new HeroEquipmentSaveData
                {
                    heroID = system.HeroID,
                    equipment = system.GetSaveData()
                }
            );
        }

        SaveSystem.Save(data);
    }

    public void LoadGame()
    {
        var data = SaveSystem.Load();

        if (data == null)
            return;

        inventory.LoadData(
            data.inventory,
            itemDatabase
        );

        foreach (var heroData in data.heroEquipment)
        {
            var system =
                equipmentSystems.Find(
                    s => s.HeroID == heroData.heroID
                );

            if (system != null)
            {
                system.LoadData(
                    heroData.equipment,
                    itemDatabase
                );
            }
        }
    }
}