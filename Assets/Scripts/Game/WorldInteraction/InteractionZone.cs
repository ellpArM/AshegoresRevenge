using UnityEngine;
using System.Collections.Generic;
public interface IInteractionAction
{
    string GetLabel();
    void Execute(GameObject interactor);
}
public class InteractionZone : MonoBehaviour, ISaveableWorldEntity
{
    [Header("Actions")]
    public List<MonoBehaviour> actions;
    // Must implement IInteractionAction

    private List<IInteractionAction> cachedActions = new();

    public void Load(WorldEntitySaveData data)
    {
        transform.position = data.position;

        //JsonUtility.FromJsonOverwrite(data.jsonData, this);
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

    private void Awake()
    {
        foreach (var a in actions)
        {
            if (a is IInteractionAction action)
                cachedActions.Add(action);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ShowUI(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        InteractionUI.Instance.Hide();
    }

    private void ShowUI(GameObject player)
    {
        InteractionUI.Instance.Show(cachedActions, player);
        InteractionUI.Instance.transform.position = transform.position;
    }
}