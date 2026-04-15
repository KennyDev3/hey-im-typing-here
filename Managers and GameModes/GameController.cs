using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    // Define game mode constants
    public const string GAME_MODE_TIME_ATTACK = "TimeAttack";
    public const string GAME_MODE_ENDLESS = "Endless";
    public const string GAME_MODE_STORY = "Story";

    // The current selected game mode
    private string currentGameMode;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameMode(string gameMode)
    {
        currentGameMode = gameMode;
        PlayerPrefs.SetString("SelectedGameMode", gameMode);
        PlayerPrefs.Save();
    }

    public string GetCurrentGameMode()
    {
        if (string.IsNullOrEmpty(currentGameMode))
        {
            currentGameMode = PlayerPrefs.GetString("SelectedGameMode", GAME_MODE_TIME_ATTACK);
        }
        return currentGameMode;
    }
}