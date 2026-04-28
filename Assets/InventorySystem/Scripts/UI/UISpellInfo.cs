using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISpellInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image sprite;
    public BaseSkill spell;
    public GameObject spellDescription;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (sprite == null)
        {
            sprite = this.gameObject.GetComponent<Image>();
        }

        // Ensure description is hidden initially
        if (spellDescription != null)
        {
            spellDescription.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpellInfo()
    {
        if (sprite == null || spell == null)
            return;

        sprite.sprite = spell.skillIcon;
    }

    // IPointerEnterHandler implementation - called when the pointer enters the UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spellDescription == null)
            return;

        spellDescription.SetActive(true);
        spellDescription.GetComponent<UISpellDescription>()?.ClearData();
        spellDescription.GetComponent<UISpellDescription>()?.SetData(spell);
        
    }

    // IPointerExitHandler implementation - called when the pointer exits the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        if (spellDescription == null)
            return;

        spellDescription.SetActive(false);
    }
}
