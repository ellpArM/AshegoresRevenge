using UnityEngine;

[CreateAssetMenu(fileName = "HeroSelectionData", menuName = "Scriptable Objects/HeroSelectionData")]
public class HeroSelectionData : ScriptableObject
{
    public GameObject[] selectedHeroes;
    public bool hasSelection = false;

    public void SaveHeroes(GameObject[] heroes)
    {
        selectedHeroes = heroes;
        hasSelection = true;
    }

    public void Clear()
    {
        selectedHeroes = null;
        hasSelection = false;
    }
}
