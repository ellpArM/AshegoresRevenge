using System;
using UnityEngine;

public class EntitiesDatabaseBootstrap : MonoBehaviour
{
    public static EntitiesDatabaseBootstrap instance;
    public EntitiesDatabase database;

    internal GameObject Get(string guid)
    {
        return database.Get(guid);
    }

    void Awake()
    {
        instance = this;
        database.BuildLookup();
    }
}