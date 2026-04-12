using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Game/Entities Database")]
public class EntitiesDatabase : ScriptableObject
{
    [SerializeField]
    private List<GameObject> entities = new();

    private Dictionary<string, GameObject> lookup;

    public void Register(FightingEntity entity)
    {
        if (entity == null)
            return;

        entities.RemoveAll(e => e == null);

        if (!entities.Contains(entity.gameObject))
        {
            entities.Add(entity.gameObject);
        }
    }

    public void Unregister(FightingEntity entity)
    {
        if (entities.Contains(entity.gameObject))
        {
            entities.Remove(entity.gameObject);
        }
    }

    public void BuildLookup()
    {
        lookup = new Dictionary<string, GameObject>();

        foreach (var e in entities)
        {
            if (e == null)
                continue;

            if (!lookup.ContainsKey(e.GetComponent<FightingEntity>().Guid))
                lookup.Add(e.GetComponent<FightingEntity>().Guid, e);
        }
    }

    public GameObject Get(string guid)
    {
        return lookup[guid];
    }
    public GameObject GetRandom()
    {
        return entities[UnityEngine.Random.Range(0, entities.Count)];
    }
}