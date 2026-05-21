using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[EditorWindowTitle(title = "Player Data Manager Viewer")]
public class PlayerDataManagerViewer : EditorWindow
{
    Vector2 _scroll;
    Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();

    // Keep a selected skill picker per character GUID
    Dictionary<string, BaseSkill> _skillToAdd = new Dictionary<string, BaseSkill>();

    [MenuItem("Window/Player Data Manager Viewer")]
    public static void ShowWindow() => GetWindow<PlayerDataManagerViewer>("Player Data Manager");

    void OnInspectorUpdate()
    {
        // Keep window responsive to runtime changes
        Repaint();
    }

    void OnGUI()
    {
        GUILayout.Label("PlayerDataManager Viewer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        PlayerDataManager pdm = PlayerDataManager.instance;
        if (pdm == null)
        {
            // try to find in scene (in edit-mode there might be a GameObject with the component)
            pdm = FindObjectOfType<PlayerDataManager>();
        }

        if (pdm == null)
        {
            EditorGUILayout.HelpBox("No PlayerDataManager instance found in the scene. Make sure a GameObject with PlayerDataManager exists.", MessageType.Warning);
            if (GUILayout.Button("Search Scene"))
                Repaint();
            return;
        }

        EditorGUILayout.LabelField("Found PlayerDataManager", EditorStyles.helpBox);
        EditorGUILayout.Space();

        if (pdm.party == null || pdm.party.Count == 0)
        {
            EditorGUILayout.HelpBox("Party is empty.", MessageType.Info);
            if (GUILayout.Button("Refresh"))
                Repaint();
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var kv in pdm.party.OrderBy(k => k.Key))
        {
            string guid = kv.Key;
            CharacterData cd = kv.Value;

            if (!_foldouts.ContainsKey(guid))
                _foldouts[guid] = false;

            // header shows guid and level (if available)
            string header = guid;
            if (cd != null) header += $"  (Level {cd.level}  LP:{cd.skillPoints})";

            _foldouts[guid] = EditorGUILayout.Foldout(_foldouts[guid], header, true);
            if (_foldouts[guid])
            {
                EditorGUI.indentLevel++;

                if (cd == null)
                {
                    EditorGUILayout.LabelField("CharacterData is null");
                }
                else
                {
                    EditorGUILayout.LabelField("Level", cd.level.ToString());
                    EditorGUILayout.LabelField("Experience", cd.experience.ToString());
                    EditorGUILayout.LabelField("Skill Points", cd.skillPoints.ToString());

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("+10 EXP"))
                    {
                        if (Application.isPlaying)
                        {
                            cd.AddExperience(10);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(
                                "Runtime Required",
                                "Experience can only be added while the game is playing.",
                                "OK"
                            );
                        }
                    }

                    if (GUILayout.Button("+50 EXP"))
                    {
                        if (Application.isPlaying)
                        {
                            cd.AddExperience(50);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(
                                "Runtime Required",
                                "Experience can only be added while the game is playing.",
                                "OK"
                            );
                        }
                    }

                    if (GUILayout.Button("+100 EXP"))
                    {
                        if (Application.isPlaying)
                        {
                            cd.AddExperience(100);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(
                                "Runtime Required",
                                "Experience can only be added while the game is playing.",
                                "OK"
                            );
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("HP", $"{cd.currentHP} / {cd.maxHP}");
                    EditorGUILayout.LabelField("Spell Power", cd.spellPower.ToString());

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Equipped Skills", EditorStyles.boldLabel);

                    if (cd.equippedSkills == null || cd.equippedSkills.Count == 0)
                    {
                        EditorGUILayout.LabelField("None");
                    }
                    else
                    {
                        for (int i = 0; i < cd.equippedSkills.Count; i++)
                        {
                            var skill = cd.equippedSkills[i];
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField($"Slot {i}", skill as Object, typeof(MonoBehaviour), true);
                            if (GUILayout.Button("Unequip", GUILayout.Width(80)))
                            {
                                if (Application.isPlaying)
                                {
                                    cd.UnequipSkill(skill);
                                }
                                else
                                {   
                                    EditorUtility.DisplayDialog("Runtime Required", "Unequip can only run while the game is playing.", "OK");
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    // --- Add Equipped Skill UI ---
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Add Equipped Skill", EditorStyles.boldLabel);

                    BaseSkill currentlyPicked = null;
                    _skillToAdd.TryGetValue(guid, out currentlyPicked);
                    BaseSkill picked = (BaseSkill)EditorGUILayout.ObjectField("Skill", currentlyPicked as Object, typeof(BaseSkill), true);
                    _skillToAdd[guid] = picked;

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Equipped Skill"))
                    {
                        if (!Application.isPlaying)
                        {
                            EditorUtility.DisplayDialog("Runtime Required", "Adding equipped skills can only run while the game is playing.", "OK");
                        }
                        else
                        {
                            if (picked == null)
                            {
                                EditorUtility.DisplayDialog("No Skill Selected", "Please select a BaseSkill to add.", "OK");
                            }
                            else
                            {
                                bool added = cd.EquipSkill(picked);
                                if (!added)
                                {
                                    EditorUtility.DisplayDialog("Add Failed", "Equip failed: slot limit reached or other restriction.", "OK");
                                }
                            }
                        }
                    }

                    if (GUILayout.Button("Clear Picker", GUILayout.Width(100)))
                    {
                        _skillToAdd[guid] = null;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Unlocked Node IDs", EditorStyles.boldLabel);
                    if (cd.unlockedNodeIds == null || cd.unlockedNodeIds.Count == 0)
                        EditorGUILayout.LabelField("None");
                    else
                    {
                        // show a wrapped label
                        string combined = string.Join(", ", cd.unlockedNodeIds);
                        EditorGUILayout.SelectableLabel(combined, GUILayout.Height(40));
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Ping Hero GameObject"))
                    {
                        // find HeroEntity with matching Guid and ping in hierarchy
                        var heroes = FindObjectsOfType<HeroEntity>();
                        var match = heroes.FirstOrDefault(h => h.Guid == guid);
                        if (match != null)
                        {
                            EditorGUIUtility.PingObject(match.gameObject);
                            Selection.activeGameObject = match.gameObject;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Not found", "No HeroEntity with matching Guid found in scene.", "OK");
                        }
                    }

                    if (GUILayout.Button("Clear Equipped (runtime only)"))
                    {
                        if (Application.isPlaying)
                        {
                            cd.equippedSkills.Clear();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Runtime Required", "Clearing equipped skills can only run while the game is playing.", "OK");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
            Repaint();
        if (GUILayout.Button("Copy Party GUIDs"))
        {
            string joined = string.Join("\n", pdm.party.Keys);
            EditorGUIUtility.systemCopyBuffer = joined;
            ShowNotification(new GUIContent("Copied GUIDs to clipboard"));
        }
        EditorGUILayout.EndHorizontal();
    }
}