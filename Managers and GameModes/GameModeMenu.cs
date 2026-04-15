using UnityEngine;
using System.Collections;

public class GameModeMenu : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // The music to fade out
    private const string TIME_ATTACK_MODE = "TimeAttack";
    private const string STORY_MODE = "Story";
    private const string ENDLESS_MODE = "Endless";

    public void StartTimeAttackMode() => StartGameMode(TIME_ATTACK_MODE, "TimeAttack");
    public void StartStoryMode() => StartGameMode(STORY_MODE, "StoryMode");
    public void StartEndlessMode() => StartGameMode(ENDLESS_MODE, "EndlessMode");

    private void StartGameMode(string gameMode, string sceneName)
    {
        SetGameMode(gameMode);
        StartCoroutine(FadeOutAudioAndScene(sceneName));
    }

    private IEnumerator FadeOutAudioAndScene(string sceneName)
    {
        // Start both fades simultaneously
        if (audioSource != null)
        {
            float startVolume = audioSource.volume;
            float fadeDuration = 2f; // Match this with FadeTransition's duration
            float elapsed = 0f;

            // Trigger screen fade (assuming FadeTransition handles its own timing)
            FadeTransition.Instance.FadeToScene(sceneName);

            // Fade audio in parallel
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }
            audioSource.volume = 0f;
        }
        else
        {
            // If no audio, just fade the scene
            FadeTransition.Instance.FadeToScene(sceneName);
        }
    }

    private void SetGameMode(string gameMode)
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.SetGameMode(gameMode);
        }
        else
        {
            PlayerPrefs.SetString("SelectedGameMode", gameMode);
            PlayerPrefs.Save();
        }
    }
}