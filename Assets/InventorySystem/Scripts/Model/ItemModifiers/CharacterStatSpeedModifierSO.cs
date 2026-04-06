using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatSpeedModifierSO", menuName = "StatModifiers/CharacterStatSpeedModifierSO")]
public class CharacterStatSpeedModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerMovement playerMovement = character.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.moveSpeed += val;
        }
    }
}
