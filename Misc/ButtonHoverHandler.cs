using UnityEngine;
using TMPro;
using System.Collections;

public class ButtonHoverHandler : MonoBehaviour
{
    public GameObject descriptionPanel; // Reference to the Panel GameObject
    public TextMeshProUGUI descriptionText; // Reference to the TMP Text element for the description


    public string timeAttackDescription = "3 Minute run in an increasingly challenging level, Can you get the highest score?";
    public string storyModeDescription = "Like a Call Of Duty Campaign, just without the budget or the corporate interference.";
    public string endlessModeDescription = "No timers or Game Over's, sit back, relax and type away.";
    public string howToPlayerDescription = "An explanation of the controls and mechanics.";

    public float fadeDuration = 0.2f; // Duration of the fade animation

    private CanvasRenderer panelRenderer;

    private void Start()
    {
        panelRenderer = descriptionPanel.GetComponent<CanvasRenderer>();
        panelRenderer.SetAlpha(0); // Ensure the panel is fully transparent at the start
        descriptionPanel.SetActive(false); // Disable the panel initially
    }

    public void OnTimeAttackHover()
    {
        descriptionText.text = timeAttackDescription;
        ShowPanel();
    }

    public void OnStoryModeHover()
    {
        descriptionText.text = storyModeDescription;
        ShowPanel();
    }

    public void OnEndlessModeHover()
    {
        descriptionText.text = endlessModeDescription;
        ShowPanel();
    }

    public void OnHowToPlayerHover()
    {
        descriptionText.text = howToPlayerDescription;
        ShowPanel();
    }

    public void OnHoverExit()
    {
        HidePanel();
    }

    private void ShowPanel()
    {
        StopAllCoroutines(); // Stop any ongoing fade animations
        descriptionPanel.SetActive(true); // Enable the panel
        StartCoroutine(FadePanel(1)); // Fade in the panel
    }

    private void HidePanel()
    {
        StopAllCoroutines(); // Stop any ongoing fade animations
        StartCoroutine(FadePanel(0)); // Fade out the panel
    }

    private IEnumerator FadePanel(float targetAlpha)
    {
        float startAlpha = panelRenderer.GetAlpha();
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            panelRenderer.SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panelRenderer.SetAlpha(targetAlpha); // Ensure the final alpha is set

        if (targetAlpha == 0)
        {
            descriptionPanel.SetActive(false); // Disable the panel when fully transparent
        }
    }
}