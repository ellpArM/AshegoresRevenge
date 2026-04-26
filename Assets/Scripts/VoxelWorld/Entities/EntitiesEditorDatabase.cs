using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Map Editor/Entities Database")]
public class EntitiesEditorDatabase : ScriptableObject
{
    public List<EntityDefinition> entities;
    private Dictionary<string, GameObject> lookup;
    public void Init()
    {
        lookup = new Dictionary<string, GameObject>();

        foreach (var e in entities)
            lookup[e.name] = e.prefab;
    }

    public GameObject GetPrefab(string type)
    {
        if (lookup == null)
            Init();

        return lookup.TryGetValue(type, out var prefab) ? prefab : null;
    }

    public EntityDefinition GetByIndex(int index)
    {
        if (index < 0 || index >= entities.Count) return null;
        return entities[index];
    }
}
