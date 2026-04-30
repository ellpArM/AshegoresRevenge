using UnityEngine;

public class ExitMapAction : MonoBehaviour, IInteractionAction
{
    public string targetScene;

    public string GetLabel() => "Exit";

    public void Execute(GameObject interactor)
    {
        Debug.Log("Exiting map...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }
}