using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Button skillButton;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillNameText;

    private BaseSkill assignedSkill;

    private void Awake()
    {
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(OnSkillPressed);
        }
    }
    public void Initialize(BaseSkill skill)
    {
        assignedSkill = skill;

        if (assignedSkill == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (skillIcon != null)
        {
            skillIcon.sprite = assignedSkill.skillIcon;
            skillIcon.enabled = assignedSkill.skillIcon != null;
        }

        if (skillNameText != null)
        {
            skillNameText.text = assignedSkill.skillName;
        }
    }

    private void OnSkillPressed()
    {
        if (assignedSkill == null)
            return;

        StartCoroutine(ExecuteSkillCoroutine());
    }
    private IEnumerator ExecuteSkillCoroutine()
    {
        yield return StartCoroutine(assignedSkill.Execute());
    }

}
