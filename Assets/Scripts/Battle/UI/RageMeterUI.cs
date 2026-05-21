using UnityEngine;

public class RageMeterUI : MonoBehaviour
{
    public GameObject rageIconPrefab;
    public Transform container;
    private RagePassive currentRagePassive;

    public void Initialize(RagePassive ragePassive)
    {
        if (currentRagePassive != null)
            currentRagePassive.OnRageChanged -= RefreshUI;

        currentRagePassive = ragePassive;

        if (currentRagePassive != null)
            currentRagePassive.OnRageChanged += RefreshUI;

        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }

        if (currentRagePassive == null)
            return;

        for (int i = 0; i < currentRagePassive.rageValue; i++)
        {
            Instantiate(rageIconPrefab, container);
        }
    }

    public void RefreshUI(RagePassive ragePassive)
    {
        currentRagePassive = ragePassive;
        RefreshUI();
    }
    //private void OnEnable()
    //{
    //    if (currentRagePassive != null)
    //        currentRagePassive.OnRageChanged += RefreshUI;
    //}

    //private void OnDisable()
    //{
    //    if (currentRagePassive != null)
    //        currentRagePassive.OnRageChanged -= RefreshUI;
    //}
}
