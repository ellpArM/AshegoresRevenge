using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnlockSkill : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] string nodeId;
    [SerializeField] Image nodeImage;

    // Event sent when this node UI is clicked; subscribers (e.g. SkillTreeUIController) will receive the node id.
    public event Action<string> OnNodeClicked;
    public BaseSkill skill;
    [SerializeField] Color lockedColor = Color.gray;
    [SerializeField] Color unlockedColor = Color.white;

    private void Awake()
    {
        nodeImage = gameObject.GetComponent<Image>();
    }

    public void SetNodeId(string id) => nodeId = id;

    public void SetNodeImage(Sprite sprite)
    {
        if (nodeImage != null)
        {
            nodeImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("UnlockSkill: nodeImage is not assigned.");
        }
    }

    public void SetNode(SkillNode node, bool unlocked)
    {
        if (node == null)
        {
            Clear();
            return;
        }

        nodeId = node.id;
        skill = node.baseSkill;

        // Set sprite
        if (nodeImage != null)
        {
            if (node.baseSkill != null)
            {
                nodeImage.enabled = true;
                nodeImage.sprite = node.baseSkill.skillIcon;
            }
            else
            {
                nodeImage.sprite = null;
                nodeImage.enabled = false;
            }
        }

        // Unlock visuals
        if (nodeImage != null)
        {
            nodeImage.color = unlocked ? unlockedColor : lockedColor;
        }

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        nodeId = "";
        skill = null;

        if (nodeImage != null)
        {
            nodeImage.sprite = null;
            nodeImage.enabled = false;
        }

        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            Debug.LogWarning("UnlockSkill: nodeId is empty.");
            return;
        }

        OnNodeClicked?.Invoke(nodeId);
    }
}
    