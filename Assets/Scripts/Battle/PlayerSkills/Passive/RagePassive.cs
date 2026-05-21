using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class RagePassive : PassiveSkill
{
    public int rageValue;
    public int maxRage = 5;
    public event Action OnRageChanged;
    private void Start()
    {
        owner = transform.root.gameObject.GetComponent<FightingEntity>();
        RageMeterUI ui = owner.GetComponentInChildren<RageMeterUI>();
        if (ui != null)
            ui.Initialize(this);
        else
        {
            EntityStatusUI statusUI = owner.GetComponentInChildren<EntityStatusUI>();
            statusUI.ConnectUI(EntityUIType.Rage);

            RageMeterUI rageUI =
                statusUI.GetUI<RageMeterUI>(EntityUIType.Rage);

            rageUI.Initialize(this);
        }
    }
    public void AddRage(int value)
    {
        rageValue = Mathf.Min(rageValue + value, maxRage);
        OnRageChanged?.Invoke();
    }
    public int RemoveRage(int value)
    {
        int usedRage = Mathf.Min(value, rageValue);
        rageValue -= usedRage;

        OnRageChanged?.Invoke();

        if (rageValue <= 0)
            Destroy(this);
        return usedRage;
    }
    public int RemoveAllRage()
    {
        int result = rageValue;
        rageValue = 0;
        OnRageChanged?.Invoke();
        Destroy(this);

        return result;
    }
    public override string GetDescription(int spellPower)
    {
        return $"Rage ({rageValue}/{maxRage})";
    }
    private void OnDestroy()
    {
        EntityStatusUI ui = owner.GetComponentInChildren<EntityStatusUI>();

        if (ui != null)
            ui.DisconnectUI(EntityUIType.Rage);
    }
}
