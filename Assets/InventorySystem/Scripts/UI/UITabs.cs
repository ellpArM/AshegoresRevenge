using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITabs : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject tab;

    public event Action<GameObject, GameObject> OnTabClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTabClicked?.Invoke(tab, gameObject);
    }
}
