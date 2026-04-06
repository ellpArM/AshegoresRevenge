using UnityEngine;

public class HoverManager : MonoBehaviour
{
    public static HoverManager Instance { get; private set; }

    [Header("UI")]
    public CharacterHoverUI hoverUIPrefab;
    private CharacterHoverUI hoverUI;

    private FightingEntity currentCard;
    private Canvas uiCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        uiCanvas = FindAnyObjectByType<Canvas>();
        if (hoverUIPrefab != null && uiCanvas != null)
        {
            hoverUI = Instantiate(hoverUIPrefab, uiCanvas.transform);
            hoverUI.Hide();
        }
    }

    public void ShowNow(FightingEntity card)
    {
        if (hoverUI == null || card == null) return;
        currentCard = card;
        hoverUI.Show(card);
        //hoverUI.SetScreenPosition(Input.mousePosition);
    }

    public void HideNow(FightingEntity card)
    {
        if (card == null) return;
        if (currentCard == card)
        {
            currentCard = null;
            if (hoverUI != null) hoverUI.Hide();
        }
    }
}