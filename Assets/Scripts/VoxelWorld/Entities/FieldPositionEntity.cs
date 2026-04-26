using UnityEngine;

public class FieldPositionEntity : MonoBehaviour, IMapEntity, ISaveableWorldEntity
{
    public enum Team
    {
        Player,
        Enemy
    }
    public WorldEntitySaveData Save()
    {
        return new WorldEntitySaveData
        {
            type = name.Replace("(Clone)", "").Trim(),
            position = transform.position,
            jsonData = ""
        };
    }

    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;

        JsonUtility.FromJsonOverwrite(data.jsonData, this);
    }

    public Team team;

    public void Register(MapRuntimeContext context)
    {
        if (team == Team.Player)
            context.playerPositions.Add(transform);
        else
            context.enemyPositions.Add(transform);
    }
}