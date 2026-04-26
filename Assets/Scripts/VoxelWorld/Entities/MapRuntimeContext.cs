using System.Collections.Generic;
using UnityEngine;

public class MapRuntimeContext : MonoBehaviour
{
    public List<Transform> playerSpawnPoints = new();
    public List<Transform> enemySpawnPoints = new();

    public List<Transform> playerPositions = new();
    public List<Transform> enemyPositions = new();

    public Transform cameraPoint;

    public void RegisterAll()
    {
        var entities = FindObjectsOfType<MonoBehaviour>(true);

        foreach (var e in entities)
        {
            if (e is IMapEntity mapEntity)
            {
                mapEntity.Register(this);
            }
        }
    }
}