using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

[CreateAssetMenu(menuName = "Database/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    private Dictionary<string, ItemSO> lookup;

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized)
            return;

        lookup = new Dictionary<string, ItemSO>();

        ItemSO[] items =
            Resources.LoadAll<ItemSO>("Items");

        foreach (var item in items)
        {
            if (lookup.ContainsKey(item.PersistentID))
            {
                Debug.LogError(
                    $"Duplicate PersistentID: {item.PersistentID}"
                );
                continue;
            }

            lookup[item.PersistentID] = item;
        }

        initialized = true;

        Debug.Log(
            $"ItemDatabase loaded {lookup.Count} items"
        );
    }

    public ItemSO GetItem(string id)
    {
        if (!initialized)
            Initialize();

        if (lookup.TryGetValue(id, out var item))
            return item;

        Debug.LogError($"Item not found: {id}");
        return null;
    }
}