using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillTreeSO", menuName = "Skill Tree/Skill Tree")]
public class SkillTreeSO : ScriptableObject
{
    public List<SkillNode> nodes = new();
    public Sprite heroSprite;

    public enum NodeType
    {
        Skill,
        Branch
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
            nodeType = type
        };
    }
}

[System.Serializable]
public class SkillNode
{
    [Header("Node")]
    public string id;

    public string nodeName;

    public Vector2 editorPosition;

    [Tooltip("Required nodes before this one unlocks")]
    public List<string> parentIDs = new();

    [Header("Node Type")]
    public SkillTreeSO.NodeType nodeType = SkillTreeSO.NodeType.Skill;

    [Header("Skill")]
    public BaseSkill baseSkill;
}