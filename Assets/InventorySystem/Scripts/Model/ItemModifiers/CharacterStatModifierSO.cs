using UnityEngine;

//[CreateAssetMenu(fileName = "CharacterModifierSO", menuName = "Scriptable Objects/CharacterModifierSO")]
public abstract class CharacterStatModifierSO : ScriptableObject
{
    public abstract void AffectCharacter(GameObject character, float val);
}
