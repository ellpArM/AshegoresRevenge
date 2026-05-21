using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityUIEntry
{
    public EntityUIType type;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "EntityUILinks", menuName = "Scriptable Objects/EntityUILinks")]
public class EntityUILinks : ScriptableObject
{
    [SerializeField] private List<EntityUIEntry> entries;

    private Dictionary<EntityUIType, GameObject> cachedLookup;

    private void BuildLookup()
    {
        cachedLookup = new Dictionary<EntityUIType, GameObject>();

        foreach (EntityUIEntry entry in entries)
        {
            if (entry.prefab == null)
                continue;

            cachedLookup[entry.type] = entry.prefab;
        }
    }

    public GameObject GetPrefab(EntityUIType type)
    {
        if (cachedLookup == null)
            BuildLookup();

        cachedLookup.TryGetValue(type, out GameObject prefab);

        return prefab;
    }
}
