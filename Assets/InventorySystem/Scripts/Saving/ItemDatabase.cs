using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

[CreateAssetMenu(menuName = "Database/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    private Dictionary<string, ItemSO> lookup;

    private bool initialized = false;


    void Awake()
    {
        var items = Resources.LoadAll<ItemSO>("Items");

        Debug.Log("ItemDatabase loaded: " + items.Length);

        foreach (var item in items)
        {
            Debug.Log(
                item.name +
                " | ID: " + item.PersistentID
            );
        }
    }

    // Ensure runtime-only fields are reset when scriptable object is enabled (domain reload / scene load)
    private void OnEnable()
    {
        // Don't rely on the serialized 'initialized' across domain reloads,
        // force reinitialization of runtime structures.
        initialized = false;
        lookup = null;
    }

    public void Initialize()
    {
        if (initialized && lookup != null)
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
        // Ensure lookup is initialized even if 'initialized' got serialized as true
        if (!initialized || lookup == null)
            Initialize();

        if (lookup != null && lookup.TryGetValue(id, out var item))
            return item;

        Debug.LogError($"Item not found: {id}");
        return null;
    }
}