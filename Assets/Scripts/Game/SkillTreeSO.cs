using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "SkillTreeSO", menuName = "Skill Tree/Skill Tree")]
public class SkillTreeSO : ScriptableObject
{
    public List<SkillNode> nodes = new();
    public Sprite heroSprite;
    public HeroEntity hero;

    public enum NodeType
    {
        Skill,
        Branch
    }

    public void ClearUnlocks()
    {
        foreach (SkillNode node in nodes)
        {
            if (node.unlocked)
            {
                node.unlocked = false;
            }
        }
        nodes[0].unlocked = true; // keep base node unlocked
    }

    [ContextMenu("Generate Default Tree")]
    public void GenerateDefaultTree()
    {
        nodes.Clear();

        // --- main path positions ---
        Vector2 posBase = new Vector2(0, 0);
        Vector2 posTier1 = new Vector2(300, 0);
        Vector2 posTier2 = new Vector2(600, 0);
        Vector2 posTier3 = new Vector2(900, 0);
        Vector2 posUltimate = new Vector2(1200, 0);

        // Create main path: base -> tier1(branch) -> tier2(branch) -> tier3(branch) -> ultimate
        SkillNode baseNode = CreateNode("base", posBase, "Base Skill", NodeType.Skill);
        baseNode.unlocked = true; // base skill starts unlocked
        baseNode.cost = 0; // base node is free / always unlocked
        SkillNode tier1 = CreateNode("tier1_branch", posTier1, "Tier 1", NodeType.Branch);
        SkillNode tier2 = CreateNode("tier2_branch", posTier2, "Tier 2", NodeType.Branch);
        SkillNode tier3 = CreateNode("tier3_branch", posTier3, "Tier 3", NodeType.Branch);
        SkillNode ultimate = CreateNode("ultimate", posUltimate, "Ultimate", NodeType.Skill);

        // link main path
        tier1.parentIDs.Add(baseNode.id);
        tier2.parentIDs.Add(tier1.id);
        tier3.parentIDs.Add(tier2.id);
        ultimate.parentIDs.Add(tier3.id);

        // add main nodes
        nodes.Add(baseNode);
        nodes.Add(tier1);
        nodes.Add(tier2);
        nodes.Add(tier3);
        nodes.Add(ultimate);

        // =========================
        // BASE branching: 4 leaf skill nodes off base (no further children)
        // =========================
        float baseBranchYStep = 150f;
        CreateLeafSideNode(baseNode, "base_side_1", posBase + new Vector2(-150, -baseBranchYStep * 2), "Base Side 1");
        CreateLeafSideNode(baseNode, "base_side_2", posBase + new Vector2(-150, -baseBranchYStep * 1), "Base Side 2");
        CreateLeafSideNode(baseNode, "base_side_3", posBase + new Vector2(-150, baseBranchYStep * 1), "Base Side 3");
        CreateLeafSideNode(baseNode, "base_side_4", posBase + new Vector2(-150, baseBranchYStep * 2), "Base Side 4");

        // =========================
        // For each tier branch, create 2 branching skill nodes; each of those spawns 2 child skill nodes
        // =========================
        CreateBranchingTreeForTier(tier1, posTier1, "tier1");
        CreateBranchingTreeForTier(tier2, posTier2, "tier2");
        CreateBranchingTreeForTier(tier3, posTier3, "tier3");
    }

    private void CreateBranchingTreeForTier(SkillNode tierNode, Vector2 tierPosition, string prefix)
    {
        // two branching skill nodes for the tier
        float branchXOffset = -50f;
        float branchYStep = 150f;

        SkillNode branchA = CreateNode($"{prefix}_branch_a", tierPosition + new Vector2(branchXOffset, -branchYStep), $"{prefix} Branch A", NodeType.Skill);
        SkillNode branchB = CreateNode($"{prefix}_branch_b", tierPosition + new Vector2(branchXOffset, branchYStep), $"{prefix} Branch B", NodeType.Skill);

        branchA.parentIDs.Add(tierNode.id);
        branchB.parentIDs.Add(tierNode.id);

        nodes.Add(branchA);
        nodes.Add(branchB);

        // For each branch create two child skill nodes
        for (int i = 0; i < 2; i++)
        {
            float childX = tierPosition.x + 150f;
            float childY_A = branchA.editorPosition.y + (i == 0 ? -60f : 60f);
            SkillNode childA = CreateNode($"{branchA.id}_child_{i+1}", new Vector2(childX, childY_A), $"{branchA.nodeName} Child {i+1}", NodeType.Skill);
            childA.parentIDs.Add(branchA.id);
            nodes.Add(childA);

            float childY_B = branchB.editorPosition.y + (i == 0 ? -60f : 60f);
            SkillNode childB = CreateNode($"{branchB.id}_child_{i+1}", new Vector2(childX, childY_B), $"{branchB.nodeName} Child {i+1}", NodeType.Skill);
            childB.parentIDs.Add(branchB.id);
            nodes.Add(childB);
        }
    }

    // Creates a single leaf skill node off the given parent (no further children)
    private void CreateLeafSideNode(SkillNode parent, string id, Vector2 position, string name = "New Node")
    {
        SkillNode node = CreateNode(id, position, name, NodeType.Skill);
        node.parentIDs.Add(parent.id);
        nodes.Add(node);
    }

    private SkillNode CreateNode(
        string id,
        Vector2 position,
        string name = "New Node",
        NodeType type = NodeType.Skill
    )
    {
        return new SkillNode
        {
            id = id,
            editorPosition = position,
            nodeName = name,
            nodeType = type,
            unlocked = false,
            cost = 1
        };
    }

    // --- New runtime API for unlocking / resetting ---

    public SkillNode GetNodeById(string id)
    {
        return nodes.FirstOrDefault(n => n.id == id);
    }

    // Check if a node is unlocked for this character (uses CharacterData.unlockedNodeIds)
    public bool IsNodeUnlocked(CharacterData character, string nodeId)
    {
        if (character == null)
            return false;

        return character.unlockedNodeIds.Contains(nodeId);
    }

    // Returns true if the node can be unlocked (parents satisfied and character has enough skill points)
    public bool CanUnlockNode(CharacterData character, string nodeId)
    {
        SkillNode node = GetNodeById(nodeId);
        if (node == null || character == null)
            return false;

        if (IsNodeUnlocked(character, nodeId))
            return false;

        // All parent nodes must be unlocked first
        foreach (string parentId in node.parentIDs)
        {
            if (!IsNodeUnlocked(character, parentId))
                return false;
        }

        // Enough skill points
        return character.skillPoints >= node.cost;
    }

    // Attempt to unlock node; deduct skill points on success and register any skill granted by node.
    // Returns true when unlocked successfully.
    public bool TryUnlockNode(CharacterData character, string nodeId)
    {
        if (!CanUnlockNode(character, nodeId))
            return false;

        SkillNode node = GetNodeById(nodeId);
        if (node == null)
            return false;

        character.skillPoints -= node.cost;
        character.unlockedNodeIds.Add(nodeId);

        if (node.baseSkill != null && !character.equippedSkills.Contains(node.baseSkill))
            character.equippedSkills.Add(node.baseSkill);

        return true;
    }

    // Force-unlock: ensures parents are unlocked (recursively) and then unlocks this node for the character
    // Does not deduct skill points.
    public bool ForceUnlockNode(CharacterData character, string nodeId)
    {
        if (character == null)
            return false;

        SkillNode node = GetNodeById(nodeId);
        if (node == null)
            return false;

        if (IsNodeUnlocked(character, nodeId))
            return false;

        // Ensure parents are unlocked first (recursively)
        foreach (string parentId in node.parentIDs)
        {
            if (!IsNodeUnlocked(character, parentId))
            {
                // If parent exists, force unlock it as well
                ForceUnlockNode(character, parentId);
            }
        }

        // Now unlock this node
        character.unlockedNodeIds.Add(nodeId);

        if (node.baseSkill != null && !character.equippedSkills.Contains(node.baseSkill))
            character.equippedSkills.Add(node.baseSkill);

        return true;
    }

    // Called when player reaches a new level to auto-unlock tier branches.
    // Level thresholds: tier1 at level 2, tier2 at level 5, tier3 at level 8.
    public void HandleLevelUnlocks(CharacterData character, int newLevel)
    {
        if (character == null)
            return;

        if (newLevel >= 2)
            ForceUnlockNode(character, "tier1_branch");

        if (newLevel >= 5)
            ForceUnlockNode(character, "tier2_branch");

        if (newLevel >= 8)
            ForceUnlockNode(character, "tier3_branch");
    }

    // Resets all unlocked nodes for the character, refunds skill points for unlocked nodes except base nodes.
    // Keeps the base nodes (nodes that were marked unlocked in the SO by default) unlocked.
    // Returns the total refunded skill points.
    public int ResetTree(CharacterData character)
    {
        if (character == null)
            return 0;

        // Nodes that are considered base (stay unlocked after reset)
        HashSet<string> baseNodeIds = new HashSet<string>(nodes.Where(n => n.unlocked).Select(n => n.id));

        // Copy current unlocked ids to iterate safely
        List<string> currentlyUnlocked = character.unlockedNodeIds.ToList();

        int refunded = 0;
        foreach (string id in currentlyUnlocked)
        {
            if (baseNodeIds.Contains(id))
                continue; // keep base nodes

            SkillNode node = GetNodeById(id);
            if (node != null)
            {
                refunded += node.cost;

                if (node.baseSkill != null)
                    character.UnequipSkill(node.baseSkill);
            }

            character.unlockedNodeIds.Remove(id);
        }

        // Ensure base nodes exist in the unlocked set
        foreach (string baseId in baseNodeIds)
        {
            if (!character.unlockedNodeIds.Contains(baseId))
                character.unlockedNodeIds.Add(baseId);
        }

        character.skillPoints += refunded;
        return refunded;
    }
}

[System.Serializable]
public class SkillNode
{
    [Header("Node")]
    public string id;

    public string nodeName;

    public Vector2 editorPosition;

    public bool unlocked;

    [Tooltip("Required nodes before this one unlocks")]
    public List<string> parentIDs = new();

    [Header("Node Type")]
    public SkillTreeSO.NodeType nodeType = SkillTreeSO.NodeType.Skill;

    [Header("Skill")]
    public BaseSkill baseSkill;

    [Header("Unlock")]
    public int cost = 1;
}