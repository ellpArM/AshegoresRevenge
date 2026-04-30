using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("UI")]
    public GameObject panel;
    public Button buttonPrefab;
    public Transform container;

    private GameObject currentInteractor;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(List<IInteractionAction> actions, GameObject interactor)
    {
        currentInteractor = interactor;

        panel.SetActive(true);

        transform.position = interactor.transform.position;

        // Clear old buttons
        foreach (Transform child in container)
            Destroy(child.gameObject);

        // Create new buttons
        foreach (var action in actions)
        {
            Button btn = Instantiate(buttonPrefab, container);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = action.GetLabel();

            btn.onClick.AddListener(() =>
            {
                action.Execute(currentInteractor);
                Hide();
            });
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}