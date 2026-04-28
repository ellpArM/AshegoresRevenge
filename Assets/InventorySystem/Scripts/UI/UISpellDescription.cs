using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class UISpellDescription : MonoBehaviour
{
    [SerializeField] GameObject elementIconPrefab;
    [SerializeField] GameObject elementIconParent;
    [SerializeField] TMP_Text spellNameText;
    [SerializeField] TMP_Text spellDamageText;
    [SerializeField] TMP_Text spellDescription;
    [SerializeField] List<Sprite> elementIcons;

    Dictionary<ElementType, int> elementIconDictionary = new Dictionary<ElementType, int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        elementIconDictionary[ElementType.Fire] = 0;
        elementIconDictionary[ElementType.Water] = 1;
        elementIconDictionary[ElementType.Nature] = 2;
        elementIconDictionary[ElementType.Wind] = 3;
        elementIconDictionary[ElementType.Spirit] = 4;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(BaseSkill spell)
    {
        ClearData();

        spellNameText.text = spell.skillName;
        if (spell is AttackSkill attack)
        {
            spellDamageText.text = attack.baseDamage > 0 ? attack.baseDamage.ToString() : "";
        }
        else
        {
            spellDamageText.text = "";
        }
        spellDescription.text = spell.description;
        foreach (ElementType element in spell.requiredElements)
        {
            GameObject icon = Instantiate(elementIconPrefab, elementIconParent.transform);
            Debug.Log(icon.GetComponent<Image>().sprite);
            Debug.Log(elementIconDictionary[element]);
            icon.GetComponent<Image>().sprite = elementIcons[elementIconDictionary[element]];
        }
    }

    public void ClearData()
    {
        spellNameText.text = "";
        spellDamageText.text = "";
        spellDescription.text = "";
        foreach (Transform child in elementIconParent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
