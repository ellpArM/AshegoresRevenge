using UnityEngine;

public class TalkAction : MonoBehaviour, IInteractionAction
{
    [TextArea]
    public string dialogue;

    public string GetLabel() => "Talk";

    public void Execute(GameObject interactor)
    {
        Debug.Log("NPC says: " + dialogue);
    }
}