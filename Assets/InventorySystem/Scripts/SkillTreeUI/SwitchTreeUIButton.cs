using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwitchTreeUIButton : MonoBehaviour, IPointerClickHandler
{
    public SkillTreeSO skillTreeSO;

    [SerializeField] Image buttonSprite;

    // Event invoked when this tab/button is clicked
    public event Action<SkillTreeSO> OnTabClicked;


    void Start()
    {
        // (optional) ensure buttonSprite is assigned; nothing else required here
    }

    public void SetSprite(Sprite sprite)
    {
        if (buttonSprite != null)
            buttonSprite.sprite = sprite;
    }

    public void SetSkillTreeSO(SkillTreeSO skillTree)
    {
        skillTreeSO = skillTree;
    }

    // Call this from the Button component OnClick or from code to select this tab
    public void OnPointerClick(PointerEventData eventData)
    {
        if (skillTreeSO != null)
            OnTabClicked.Invoke(skillTreeSO);
    }
}
