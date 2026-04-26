using UnityEngine;

public class PlacedEntity : MonoBehaviour
{
    public string entityId;

    // Optional: reference back to definition
    public EntityDefinition definition;

    // Helps editor logic (selection, removal)
    public void Initialize(EntityDefinition def)
    {
        definition = def;
        entityId = def.id;
    }
}