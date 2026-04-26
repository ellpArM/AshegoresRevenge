using UnityEngine;

public class CameraPointEntity : MonoBehaviour, IMapEntity, ISaveableWorldEntity
{
    public Vector3 offest;
    public void Register(MapRuntimeContext context)
    {
        context.cameraPoint = transform;
    }

    public WorldEntitySaveData Save()
    {
        return new WorldEntitySaveData
        {
            type = name.Replace("(Clone)", "").Trim(),
            position = transform.position,
            jsonData = JsonUtility.ToJson(this)
        };
    }

    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;

        JsonUtility.FromJsonOverwrite(data.jsonData, this);
    }
}