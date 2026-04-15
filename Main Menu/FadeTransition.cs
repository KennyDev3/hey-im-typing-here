using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public CanvasGroup fadePanel; // Reference to the Canvas Group on the FadePanel
    public float fadeDuration = 1f; // Duration of the fade in seconds

    public static FadeTransition Instance { get; private set; } // Singleton instance

    private void Awake()
    {
        // Ensure only one instance of the FadeManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persistent
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Start()
    {
        // Fade in when the scene starts
        StartCoroutine(FadeIn());
    }

    // Call this method to start the fade-out transition and load a new scene
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    // Coroutine to fade out (to black)
    private IEnumerator FadeOut(string sceneName)
    {
        fadePanel.blocksRaycasts = true; // Prevent interactions during the fade

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadePanel.alpha = Mathf.Clamp01(elapsedTime / fadeDuration); // Increase alpha (fade to black)
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 1; // Ensure it's fully black

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // Fade in after the new scene loads
        StartCoroutine(FadeIn());
    }

    // Coroutine to fade in (from black)
    private IEnumerator FadeIn()
    {
        fadePanel.alpha = 1; // Start fully black
        fadePanel.blocksRaycasts = true; // Prevent interactions during the fade

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadePanel.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration)); // Decrease alpha (fade in)
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 0; // Ensure it's fully transparent
        fadePanel.blocksRaycasts = false; // Allow interactions again
    }
}