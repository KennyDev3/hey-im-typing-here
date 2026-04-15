using UnityEngine;

public class HowToPlayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject howToPlayPanel; // Drag your HowToPlay panel here in Inspector

    // Called by the "How To Play" button's OnClick event
    public void ShowHowToPlayPanel()
    {
        howToPlayPanel.SetActive(true);
    }

    // Called by the "Return to Menu" button's OnClick event
    public void HideHowToPlayPanel()
    {
        howToPlayPanel.SetActive(false);
    }
}
