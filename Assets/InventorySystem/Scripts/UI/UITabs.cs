using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITabs : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject tab;
    [SerializeField] EquipmentSystem equipmentSystem;

    public event Action<GameObject, GameObject, EquipmentSystem> OnTabClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTabClicked?.Invoke(tab, gameObject, equipmentSystem);
    }
}
