using UnityEngine;

public class VendorAction : MonoBehaviour, IInteractionAction
{
    public string label = "Shop";

    public string GetLabel() => label;

    public void Execute(GameObject interactor)
    {
        Debug.Log("Opening shop UI...");
        // Call your shop system here
    }
}