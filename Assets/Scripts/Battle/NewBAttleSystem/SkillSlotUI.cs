using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button skillButton;
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillNameText;

    private BaseSkill assignedSkill;
    private SkillsHolderUI ownerUI;

    private void Awake()
    {
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(OnSkillPressed);
        }
    }
    public void Initialize(BaseSkill skill, SkillsHolderUI skillsHolderUI)
    {
        assignedSkill = skill;
        ownerUI = skillsHolderUI;

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
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (assignedSkill == null || ownerUI == null)
            return;

        ownerUI.ShowSkillInfo(assignedSkill);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ownerUI == null)
            return;

        ownerUI.HideSkillInfo();
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
