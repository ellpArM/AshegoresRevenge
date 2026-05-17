using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUIController : MonoBehaviour
{
    [SerializeField] GameObject tabsHolder;
    [SerializeField] GameObject tabPrefab;
    [SerializeField] Image currentSprite;

    [SerializeField] HeroSelectionData heroSelectionData;

    public SkillTreeSO currentTree;

    void Start()
    {
        LoadTabs();
    }

    public void LoadSkillTreeUI()
    {
        if (currentTree == null)
        {
            currentSprite.sprite = null;
            return;
        }
        currentSprite.sprite = currentTree.heroSprite;
    }

    public void LoadTabs()
    {
        foreach (Transform child in tabsHolder.transform)
            Destroy(child.gameObject);

        foreach (GameObject hero in heroSelectionData.selectedHeroes)
        {
            SkillTreeSO tree = hero.GetComponent<HeroEntity>().skillTree;
            GameObject tab = Instantiate(tabPrefab, tabsHolder.transform);
            SwitchTreeUIButton switchTreeUIButton = tab.GetComponent<SwitchTreeUIButton>();
            switchTreeUIButton.SetSprite(hero.GetComponent<HeroEntity>().GetCardVisual());
            switchTreeUIButton.SetSkillTreeSO(tree);

            // Subscribe to the button event so controller switches tree when clicked
            switchTreeUIButton.OnTabClicked += HandleTreeSelected;
            
            // Optional: auto-select first tab
            if (currentTree == null && tree != null)
            {
                Debug.Log("Set TREE");
                currentTree = tree;
            }
        }
        LoadSkillTreeUI();
    }

    private void HandleTreeSelected(SkillTreeSO selected)
    {
        Debug.Log("Switch Tree");
        if (selected == null) return;
        currentTree = selected;
        LoadSkillTreeUI();
    }
}
