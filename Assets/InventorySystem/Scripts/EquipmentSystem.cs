using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlot
{
    Wand,
    Spellbook,
    Hat,
    Robes,
    Amulet
}

[CreateAssetMenu(fileName = "EquipmentSystem", menuName = "Scriptable Objects/EquipmentSystem")]
public class EquipmentSystem : ScriptableObject
{
    [SerializeField]
    private InventorySO inventoryData;

    private Dictionary<EquipmentSlot, EquippableItemSO> equippedItems
        = new();

    private Dictionary<EquipmentSlot, List<ItemParameter>> itemStates
        = new();

    [SerializeField]
    private List<ItemParameter> parametersToModify;

    public event Action<EquipmentSlot, EquippableItemSO> UpdateInventory;

    // Runtime-only reference to the owning hero's card sprite (set at runtime by the hero)
    // Not serialized: updated when the hero spawns / initializes
    public Sprite OwnerCardSprite { get; private set; }

    public void SetOwnerCardSprite(Sprite s)
    {
        OwnerCardSprite = s;
    }

    public void SetWeapon(EquippableItemSO item, List<ItemParameter> itemState)
    {
        EquipmentSlot slot = item.Slot;

        if (equippedItems.TryGetValue(slot, out var existingItem))
        {
            inventoryData.AddItem(existingItem, 1, itemStates[slot]);
        }

        equippedItems[slot] = item;
        itemStates[slot] = new List<ItemParameter>(itemState);
        

        //ModifyParameters(slot);
        UpdateInventory?.Invoke(slot, item);
    }

    private void ModifyParameters(EquipmentSlot slot)
    {
        var currentState = itemStates[slot];

        foreach (var parameter in parametersToModify)
        {
            if (currentState.Contains(parameter))
            {
                int index = currentState.IndexOf(parameter);

                float newValue =
                    currentState[index].value
                    + parameter.value;

                currentState[index] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }

    public EquippableItemSO GetEquippedItem(EquipmentSlot slot)
    {
        equippedItems.TryGetValue(
            slot,
            out var item
        );

        return item;
    }

    public void Unequip(EquipmentSlot slot)
    {
        if (equippedItems.TryGetValue(slot, out var item))
        {
            inventoryData.AddItem(
                item,
                1,
                itemStates[slot]
            );

            equippedItems.Remove(slot);
            itemStates.Remove(slot);
        }
    }

    public Dictionary<EquipmentSlot, EquippableItemSO> GetEquipment()
    {
        return equippedItems;
    }
}
