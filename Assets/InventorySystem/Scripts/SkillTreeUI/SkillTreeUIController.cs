using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class SkillTreeUIController : MonoBehaviour
{
    [SerializeField] GameObject tabsHolder;
    [SerializeField] GameObject tabPrefab;
    [SerializeField] Image currentSprite;

    [SerializeField] HeroSelectionData heroSelectionData;

    [SerializeField] List<EquippedSkillUI> equippedSkillUIs;
    [SerializeField] List<UnlockSkill> unlockSkillUIs;

    public SkillTreeSO currentTree;

    void Start()
    {
        LoadTabs();
    }

    private void OnEnable()
    {
        if (unlockSkillUIs != null)
        {
            foreach (var es in unlockSkillUIs)
            {
                if (es != null)
                    es.OnNodeClicked += HandleNodeClicked;
            }
        }

        if (equippedSkillUIs != null)
        {
            foreach (var es in equippedSkillUIs)
            {
                if (es != null)
                    es.OnClicked += HandleEquippedClicked;
            }
        }
    }

    private void OnDisable()
    {
        if (unlockSkillUIs != null)
        {
            foreach (var es in unlockSkillUIs)
            {
                if (es != null)
                    es.OnNodeClicked -= HandleNodeClicked;
            }
        }

        if (equippedSkillUIs != null)
        {
            foreach (var es in equippedSkillUIs)
            {
                if (es != null)
                    es.OnClicked -= HandleEquippedClicked;
            }
        }
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

        currentTree = null;
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
        RefreshEquippedSkillSlots();
        RefreshSkillNodeUI();
    }

    private void RefreshSkillNodeUI()
    {
        for (int i = 0; i < unlockSkillUIs.Count; i++)
        {
            if (unlockSkillUIs[i] == null)
                continue;
            if (currentTree != null && i < currentTree.nodes.Count)
                unlockSkillUIs[i].SetNode(currentTree.nodes[i], currentTree.IsNodeUnlocked(PlayerDataManager.instance.GetHeroData(currentTree.hero.Guid), currentTree.nodes[i].id));
            else
                unlockSkillUIs[i].Clear();

            UISpellInfo uiInfo = unlockSkillUIs[i].GetComponent<UISpellInfo>();
            if (uiInfo != null && currentTree != null && i < currentTree.nodes.Count)
            {
                uiInfo.spell = currentTree.nodes[i].baseSkill;
                uiInfo.SetSpellInfo();
            }
        }
    }

    private void RefreshEquippedSkillSlots()
    {
        if (equippedSkillUIs == null || equippedSkillUIs.Count == 0)
            return;

        if (currentTree == null)
        {
            // Clear all slots if there's no selected tree
            foreach (var slot in equippedSkillUIs)
                slot?.Clear();
            return;
        }

        // Resolve hero instance that owns this skill tree
        HeroEntity hero = currentTree.hero;
        if (hero == null && heroSelectionData != null && heroSelectionData.selectedHeroes != null)
        {
            foreach (var go in heroSelectionData.selectedHeroes)
            {
                if (go == null) continue;
                var he = go.GetComponent<HeroEntity>();
                if (he != null && he.skillTree == currentTree)
                {
                    hero = he;
                    break;
                }
            }
        }

        if (hero == null)
        {
            Debug.LogWarning("SkillTreeUIController: could not find hero for current tree when refreshing equipped slots.");
            foreach (var slot in equippedSkillUIs)
                slot?.Clear();
            return;
        }

        if (PlayerDataManager.instance == null)
        {
            Debug.LogWarning("SkillTreeUIController: PlayerDataManager.instance is null when refreshing equipped slots.");
            foreach (var slot in equippedSkillUIs)
                slot?.Clear();
            return;
        }

        var character = PlayerDataManager.instance.GetHeroData(hero.Guid);
        if (character == null)
        {
            Debug.LogWarning("SkillTreeUIController: CharacterData is null when refreshing equipped slots.");
            foreach (var slot in equippedSkillUIs)
                slot?.Clear();
            return;
        }

        // Fill UI slots from character.equippedSkills (or clear if not present)
        Debug.Log($"Refreshing equipped skill slots for hero '{hero.visibleName ?? hero.Guid}' with {character.equippedSkills.Count} equipped skills.");
        for (int i = 0; i < equippedSkillUIs.Count; i++)
        {
            if (equippedSkillUIs[i] == null)
                continue;

            if (i < character.equippedSkills.Count && character.equippedSkills[i] != null)
                equippedSkillUIs[i].SetSkill(character.equippedSkills[i]);
            else
                equippedSkillUIs[i].Clear();

            UISpellInfo uiInfo = equippedSkillUIs[i].GetComponent<UISpellInfo>();
            if (uiInfo != null)
            {
                uiInfo.spell = i < character.equippedSkills.Count ? character.equippedSkills[i] : null;
                uiInfo.SetSpellInfo();
            }
        }
    }

    private void HandleTreeSelected(SkillTreeSO selected)
    {
        Debug.Log("Switch Tree");
        if (selected == null) return;
        currentTree = selected;
        LoadSkillTreeUI();

        // refresh equipped slots for newly selected tree/hero
        RefreshEquippedSkillSlots();
        RefreshSkillNodeUI();
        //foreach (SkillNode node in currentTree.nodes)
        //{
        //    Debug.Log($"Node '{node.id}' - Unlocked: {currentTree.IsNodeUnlocked(PlayerDataManager.instance.GetHeroData(currentTree.hero.Guid), node.id)}");
        //}
    }

    // Called when a node UI raises the click event (passes node id)
    private void HandleNodeClicked(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
            return;

        if (currentTree == null)
        {
            Debug.LogWarning("SkillTreeUIController: currentTree is null when handling node click.");
            return;
        }

        // Resolve hero instance that owns this skill tree
        HeroEntity hero = currentTree.hero;
        if (hero == null && heroSelectionData != null && heroSelectionData.selectedHeroes != null)
        {
            foreach (var go in heroSelectionData.selectedHeroes)
            {
                if (go == null) continue;
                var he = go.GetComponent<HeroEntity>();
                if (he != null && he.skillTree == currentTree)
                {
                    hero = he;
                    break;
                }
            }
        }

        if (hero == null)
        {
            Debug.LogWarning("SkillTreeUIController: could not find hero for current tree.");
            return;
        }

        if (PlayerDataManager.instance == null)
        {
            Debug.LogWarning("SkillTreeUIController: PlayerDataManager.instance is null.");
            return;
        }

        // Ensure character data exists
        if (!PlayerDataManager.instance.party.ContainsKey(hero.Guid))
            PlayerDataManager.instance.AddToParty(hero);

        var character = PlayerDataManager.instance.GetHeroData(hero.Guid);
        if (character == null)
        {
            Debug.LogWarning("SkillTreeUIController: CharacterData is null after ensuring party entry.");
            return;
        }

        // Get node information
        SkillNode node = currentTree.GetNodeById(nodeId);
        if (node == null)
        {
            Debug.LogWarning($"SkillTreeUIController: node '{nodeId}' not found in currentTree.");
            return;
        }

        // If node already unlocked for this character, ensure its skill is present in CharacterData.unlockedSkills
        if (currentTree.IsNodeUnlocked(character, nodeId))
        {
            if (node.baseSkill != null)
            {
                character.EquipSkill(node.baseSkill);
                Debug.Log($"SkillTreeUIController: node '{nodeId}' already unlocked — added skill '{node.baseSkill.skillName}' to character unlockedSkills.");
                // TODO: refresh any UI that shows equipped/unlocked skills
                RefreshEquippedSkillSlots();
            }
            else
            {
                Debug.Log($"SkillTreeUIController: node '{nodeId}' already unlocked. No skill to add or skill already present.");
            }

            return;
        }

        // Otherwise, attempt to unlock the node (this will deduct skill points and add the skill on success)
        bool unlocked = currentTree.TryUnlockNode(character, nodeId);
        if (unlocked)
        {
            Debug.Log($"SkillTreeUIController: unlocked node '{nodeId}' for hero '{hero.visibleName ?? hero.Guid}'.");
            LoadSkillTreeUI();
            // TODO: refresh node visuals in your UI (call into your node UI to update states)
            RefreshEquippedSkillSlots();
            RefreshSkillNodeUI();
        }
        else
        {
            Debug.Log($"SkillTreeUIController: failed to unlock node '{nodeId}'. Check parents or skill points.");
        }
    }

    // Called when an equipped-skill UI slot is clicked.
    private void HandleEquippedClicked(EquippedSkillUI slot)
    {
        if (slot == null || slot.StoredSkill == null)
            return;

        if (currentTree == null)
        {
            Debug.LogWarning("SkillTreeUIController: currentTree is null when handling equipped slot click.");
            return;
        }

        // Resolve hero instance that owns this skill tree (same logic as in node handler)
        HeroEntity hero = currentTree.hero;
        if (hero == null && heroSelectionData != null && heroSelectionData.selectedHeroes != null)
        {
            foreach (var go in heroSelectionData.selectedHeroes)
            {
                if (go == null) continue;
                var he = go.GetComponent<HeroEntity>();
                if (he != null && he.skillTree == currentTree)
                {
                    hero = he;
                    break;
                }
            }
        }

        if (hero == null)
        {
            Debug.LogWarning("SkillTreeUIController: could not find hero for current tree when unequipping.");
            return;
        }

        if (PlayerDataManager.instance == null)
        {
            Debug.LogWarning("SkillTreeUIController: PlayerDataManager.instance is null when unequipping.");
            return;
        }

        var character = PlayerDataManager.instance.GetHeroData(hero.Guid);
        if (character == null)
        {
            Debug.LogWarning("SkillTreeUIController: CharacterData is null when unequipping.");
            return;
        }

        // Perform unequip
        var skill = slot.StoredSkill;
        character.UnequipSkill(skill);

        // Clear the UI slot
        slot.Clear();

        Debug.Log($"SkillTreeUIController: unequipped skill '{skill.skillName}' from hero '{hero.visibleName ?? hero.Guid}'.");
    }
}