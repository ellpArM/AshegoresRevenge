using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class EntitiesPaletteUI : MonoBehaviour
{
    public EntitiesEditorDatabase database;
    public EntitiesEditorController controller;

    [Header("UI")]
    public Transform container;
    public Button buttonPrefab;

    private List<Button> buttons = new List<Button>();
    private int selectedIndex = -1;

    private void Start()
    {
        GenerateUI();
    }

    private void GenerateUI()
    {
        for (int i = 0; i < database.entities.Count; i++)
        {
            int index = i;

            Button btn = Instantiate(buttonPrefab, container);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = database.entities[i].name;

            btn.onClick.AddListener(() => Select(index));

            buttons.Add(btn);
        }
    }

    private void Select(int index)
    {
        selectedIndex = index;
        controller.SetSelectedEntity(index);

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ColorBlock colors = buttons[i].colors;

            if (i == selectedIndex)
                colors.normalColor = Color.green;
            else
                colors.normalColor = Color.white;

            buttons[i].colors = colors;
        }
    }
}