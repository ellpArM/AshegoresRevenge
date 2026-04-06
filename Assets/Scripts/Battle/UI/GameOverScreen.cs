using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen instance;
    public GameObject root;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Hide();
    }
    void Update()
    {
        
    }
    public void Show()
    {
        root.SetActive(true);
    }
    public void Hide()
    {
        root.SetActive(false);
    }
    public void ExitToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
