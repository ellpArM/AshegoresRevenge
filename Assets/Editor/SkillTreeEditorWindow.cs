using UnityEditor;
using UnityEngine;

public class SkillTreeEditorWindow : EditorWindow
{
    private SkillTreeSO tree;

    private Vector2 drag;
    private Vector2 offset;

    private SkillNode selectedNode;

    private const float NODE_WIDTH = 140f;
    private const float NODE_HEIGHT = 80f;

    [MenuItem("Tools/Skill Tree Editor")]
    public static void Open()
    {
        GetWindow<SkillTreeEditorWindow>("Skill Tree Editor");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (tree == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a SkillTreeSO asset.",
                MessageType.Info
            );

            return;
        }

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnections();
        DrawNodes();

        ProcessEvents(Event.current);

        if (GUI.changed)
        {
            Repaint();
            EditorUtility.SetDirty(tree);
        }
    }

    // =========================================================
    // TOOLBAR
    // =========================================================

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        tree = (SkillTreeSO)EditorGUILayout.ObjectField(
            tree,
            typeof(SkillTreeSO),
            false,
            GUILayout.Width(250)
        );

        if (tree != null)
        {
            if (GUILayout.Button("Generate Default Tree", EditorStyles.toolbarButton))
            {
                tree.GenerateDefaultTree();
            }

            if (GUILayout.Button("Add Node", EditorStyles.toolbarButton))
            {
                AddNode();
            }
        }

        GUILayout.EndHorizontal();
    }

    // =========================================================
    // NODES
    // =========================================================

    private void DrawNodes()
    {
        foreach (SkillNode node in tree.nodes)
        {
            Rect rect = new Rect(
                node.editorPosition.x + offset.x,
                node.editorPosition.y + offset.y,
                NODE_WIDTH,
                NODE_HEIGHT
            );

            GUI.color = IsMainPath(node) ? Color.white : Color.gray;

            GUI.Box(rect, "");

            GUI.color = Color.white;

            GUILayout.BeginArea(rect);

            GUILayout.Space(5);

            node.nodeName = EditorGUILayout.TextField(node.nodeName);

            string skillName = node.baseSkill != null
                ? node.baseSkill.name
                : "No Skill";

            GUILayout.Label(skillName, EditorStyles.miniLabel);

            node.baseSkill = (BaseSkill)EditorGUILayout.ObjectField(
                node.baseSkill,
                typeof(BaseSkill),
                false
            );
            node.unlocked = GUI.Toggle(
                new Rect(5, 60, 120, 20),
                node.unlocked,
                "Unlocked"
            );

            GUILayout.EndArea();

            HandleNodeDrag(node, rect);
        }
    }

    // =========================================================
    // CONNECTIONS
    // =========================================================

    private void DrawConnections()
    {
        foreach (SkillNode node in tree.nodes)
        {
            foreach (string parentID in node.parentIDs)
            {
                SkillNode parent = GetNode(parentID);

                if (parent == null)
                    continue;

                Vector3 startPos = new Vector3(
                    parent.editorPosition.x + NODE_WIDTH,
                    parent.editorPosition.y + NODE_HEIGHT / 2
                ) + (Vector3)offset;

                Vector3 endPos = new Vector3(
                    node.editorPosition.x,
                    node.editorPosition.y + NODE_HEIGHT / 2
                ) + (Vector3)offset;

                Handles.DrawBezier(
                    startPos,
                    endPos,
                    startPos + Vector3.right * 50,
                    endPos + Vector3.left * 50,
                    Color.white,
                    null,
                    3f
                );
            }
        }
    }

    // =========================================================
    // EVENTS
    // =========================================================

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDrag:

                if (e.button == 2)
                {
                    OnDrag(e.delta);
                }

                break;
        }
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        offset += delta;

        GUI.changed = true;
    }

    private void HandleNodeDrag(SkillNode node, Rect rect)
    {
        Event e = Event.current;

        if (!rect.Contains(e.mousePosition))
            return;

        switch (e.type)
        {
            case EventType.MouseDown:

                if (e.button == 0)
                {
                    selectedNode = node;
                }

                if (e.button == 1)
                {
                    ShowContextMenu(node);
                }

                break;

            case EventType.MouseDrag:

                if (e.button == 0 && selectedNode == node)
                {
                    node.editorPosition += e.delta;

                    GUI.changed = true;
                }

                break;
        }
    }

    // =========================================================
    // CONTEXT MENU
    // =========================================================

    private void ShowContextMenu(SkillNode node)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(
            new GUIContent("Delete Node"),
            false,
            () =>
            {
                tree.nodes.Remove(node);
            }
        );

        menu.ShowAsContext();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private void AddNode()
    {
        SkillNode node = new SkillNode
        {
            id = System.Guid.NewGuid().ToString(),
            nodeName = "New Node",
            editorPosition = Vector2.zero
        };

        tree.nodes.Add(node);
    }

    private SkillNode GetNode(string id)
    {
        foreach (SkillNode node in tree.nodes)
        {
            if (node.id == id)
                return node;
        }

        return null;
    }

    private bool IsMainPath(SkillNode node)
    {
        return node.id == "base"
            || node.id == "skill_1"
            || node.id == "skill_2"
            || node.id == "skill_3"
            || node.id == "ultimate";
    }

    // =========================================================
    // GRID
    // =========================================================

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();

        Handles.color = new Color(
            gridColor.r,
            gridColor.g,
            gridColor.b,
            gridOpacity
        );

        offset += drag * 0.5f;

        Vector3 newOffset = new Vector3(
            offset.x % gridSpacing,
            offset.y % gridSpacing,
            0
        );

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                new Vector3(gridSpacing * i, position.height, 0f) + newOffset
            );
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                new Vector3(position.width, gridSpacing * j, 0f) + newOffset
            );
        }

        Handles.color = Color.white;

        Handles.EndGUI();
    }
}