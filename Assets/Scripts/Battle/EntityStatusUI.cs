using System.Collections.Generic;
using UnityEngine;

public enum EntityUIType { HP, Mana, Rage, Burn};
public static class EntityUIElements
{
    public static readonly Dictionary<EntityUIType, string> data = new()
    {
        {EntityUIType.Rage, "RageMeter"},
        {EntityUIType.Burn, "BurnMeter"},
    };
}
public class EntityStatusUI : MonoBehaviour
{
    [SerializeField] private EntityUILinks uiDatabase;
    [SerializeField] private Transform uiContainer;
    private readonly Dictionary<EntityUIType, GameObject> activeUIElements = new();
    public GameObject ConnectUI(EntityUIType type)
    {
        if (activeUIElements.ContainsKey(type))
            return null;

        GameObject prefab = uiDatabase.GetPrefab(type);

        GameObject instance = Instantiate(prefab, uiContainer);
        activeUIElements[type] = instance;
        return instance;
    }
    public void DisconnectUI(EntityUIType type)
    {
        if (!activeUIElements.TryGetValue(type, out GameObject uiObject))
            return;

        if (uiObject != null)
        {
            Destroy(uiObject);
        }

        activeUIElements.Remove(type);
    }

    public T GetUI<T>(EntityUIType type) where T : Component
    {
        if (!activeUIElements.TryGetValue(type, out GameObject uiObject))
            return null;

        return uiObject.GetComponent<T>();
    }
}
