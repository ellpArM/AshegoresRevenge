using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquippedSkillUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image iconImage;

    BaseSkill storedSkill;

    // Raised when the equipped-slot UI is clicked
    public event Action<EquippedSkillUI> OnClicked;

    // Expose the stored skill (read-only)
    public BaseSkill StoredSkill => storedSkill;

    public void SetSkill(BaseSkill skill)
    {
        storedSkill = skill;
        if (iconImage != null)
            iconImage.sprite = storedSkill != null ? storedSkill.skillIcon : null;
    }

    public void Clear()
    {
        storedSkill = null;
        if (iconImage != null)
            iconImage.sprite = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}
