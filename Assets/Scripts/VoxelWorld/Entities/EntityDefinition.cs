using UnityEngine;

[CreateAssetMenu(menuName = "Map Editor/Entity Definition")]
public class EntityDefinition : ScriptableObject
{
    public string id;
    public GameObject prefab;

    [Header("Placement Settings")]
    public Vector3 offset;
    public bool alignToSurfaceNormal = true;
}