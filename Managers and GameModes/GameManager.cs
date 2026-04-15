using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   

    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    public AudioMixer audioMixer;
 

    [Header("UI References")]
    [SerializeField] public TMP_Text _timerText; // Timer UI text
    [SerializeField] private GameObject gameOverCanvas; // Reference to the game over UI canvas
    [SerializeField] private GameObject pauseScreen;

    [SerializeField] private TMP_Text finalMoneyText; // Reference to the final money text in the game over UI
    [SerializeField] private TMP_Text finalScore;
    [SerializeField] private TMP_Text finalAccuracyText;
    [SerializeField] private TMP_Text finalTotalScoreText;
    [SerializeField] private TMP_Text finalWPM;

    [SerializeField] private TMP_Text finalCorrectWords;
    [SerializeField] private TMP_Text finalHighestMult;
    [SerializeField] private TMP_Text finalMistakes;
    [SerializeField] private TMP_Text finalMessagesSent;

    [Header("Game Settings")]
    [SerializeField] public float checkpointAddedTime = 10f;
    private float storyMultiplierResetTimer = 0;
    private float defaultMultiplerResetTimer = 3;

    private ScoreManager scoreManager;
    private GameMode currentGameMode;
    private bool isGameOver = false;
    private bool isGameStarted = false;

    public bool GameOver => isGameOver;
    public static GameManager Instance { get; private set; } // Singleton For GameManager

    private float overallTimer = 0f; // Overall game timer

    // Pause Menu Settings
    public string mainMenuSceneName = "MainMenu"; // Name of your main menu scene
    private float originalVolume = 1.0f;
    private float reducedVolume = 0.7f; // 30% lower volume
    private bool isMuted = false;
    private bool isPaused = false;




    public void Init(ScoreManager scoreManager)
    {
        this.scoreManager = scoreManager;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Determine which game mode to start based on the selected mode
        string selectedMode = "";
        

        // First check if GameController exists
        if (GameController.Instance != null)
        {
            selectedMode = GameController.Instance.GetCurrentGameMode();
        }
        else
        {
            // Fallback to PlayerPrefs
            selectedMode = PlayerPrefs.GetString("SelectedGameMode", "TimeAttack");
        }

        // If in Unity Editor, always start as Story Mode

        //#if UNITY_EDITOR // This code will only be compiled in the Unity Editor
        //        selectedMode = "Story";
        //#endif

        // Start the appropriate game mode
        if (selectedMode == "TimeAttack")
        {
            TimeAttackMode timeAttackMode = GetComponent<TimeAttackMode>();
            if (timeAttackMode == null)
            {
                Debug.LogError("TimeAttackMode component not found on the Managers GameObject!");
                return;
            }

            StartGameMode(timeAttackMode);

            // Initialize the overall timer
            overallTimer = timeAttackMode.GetTotalTime();
            Debug.Log(overallTimer);
        }
        else if (selectedMode == "Endless")
        {
            EndlessMode endlessMode = GetComponent<EndlessMode>();
            if (endlessMode == null)
            {
                Debug.LogError("EndlessMode component not found on the Managers GameObject!");
                return;
            }

            StartGameMode(endlessMode);

            // Endless mode has no total time, so set overallTimer to 0
            overallTimer = 0f;
        }
        else if (selectedMode == "Story")
        {
            StoryMode storyMode = GetComponent<StoryMode>();
            if (storyMode == null)
            {
                Debug.LogError("StoryMode component not found on the Managers GameObject!");
                return;
            }

            StartGameMode(storyMode);

            // Story mode has no total time, so set overallTimer to 0
            overallTimer = 0f;
        }
        else
        {
            Debug.LogError("Invalid game mode selected!");
        }

        // Validate that the dependency was injected
        if (scoreManager != null)
        {
            if (selectedMode == "Story")
            {
                scoreManager.SetMultiplierResetTime(storyMultiplierResetTimer);
            }
            else
            {
                scoreManager.SetMultiplierResetTime(defaultMultiplerResetTimer);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (!isGameOver && currentGameMode != null && isGameStarted) // Only update if the game has started
        {
            if (currentGameMode is TimeAttackMode)
            {
                // Update the overall timer
                overallTimer -= Time.deltaTime;

                // Update the timer UI
                if (_timerText != null)
                {
                    _timerText.text = $"{overallTimer:F1}s";
                }

                // Check if the game is over
                if (overallTimer <= 0f)
                {
                    EndGame();
                }
            }
            else if (currentGameMode is EndlessMode || currentGameMode is StoryMode)
            {
                // Update the endless/story timer (count up)
                overallTimer += Time.deltaTime;

                // Update the timer UI
                if (_timerText != null)
                {
                    _timerText.text = $"{overallTimer:F1}s";
                }
            }
        }
    }



    public void StartGameMode(GameMode mode)
    {
        // Deactivate previous mode if it exists
        if (currentGameMode != null && currentGameMode != mode)
        {
            currentGameMode.EndMode();
            // Instead of destroying, we just disable if it's a component on this object
            MonoBehaviour modeComponent = currentGameMode as MonoBehaviour;
            if (modeComponent != null)
            {
                modeComponent.enabled = false;
            }
        }

        currentGameMode = mode;

        // Enable the new mode component
        MonoBehaviour newModeComponent = currentGameMode as MonoBehaviour;
        if (newModeComponent != null)
        {
            newModeComponent.enabled = true;
        }

        // Initialize and start the mode
        if (currentGameMode != null)
        {
            currentGameMode.Initialize();
            currentGameMode.StartMode();
        }
        else
        {
            Debug.LogError("Failed to start game mode - mode is null!");
        }
    }

    public void EndGame()
    {
        if (currentGameMode != null)
        {
            currentGameMode.EndMode();
        }
        isGameOver = true;
        playerController.DisallowMovement();

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over Canvas is not assigned in the GameManager!");
        }



        // Calculate and Show End Game stats
        EndScreenTextDisplayAndCalculation();       
        SlowMotion();
    }


    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            UnpauseGame();
        }
    }

    private static void SlowMotion()
    {
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void AddTimeCheckpoint()
    {
        if (currentGameMode != null)
        {
            currentGameMode.AddTime(checkpointAddedTime);
            overallTimer += checkpointAddedTime; // Add time to the overall timer
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        pauseScreen.SetActive(true);

        // Only adjust volume if not muted
        if (!isMuted)
        {
            SetVolume(reducedVolume);
        }
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);

        // Only adjust volume if not muted
        if (!isMuted)
        {
            SetVolume(originalVolume);
        }
    }

    public void RestartGame()
    {
        // Get the current active scene and reload it
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);

        // Reset time scale in case it was altered (e.g., slow motion)
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        isGameStarted = true;
    }

    public void SetVolume(float volume)
    {
        // Only set volume if not muted
        if (!isMuted)
        {
            float volumeDB = Mathf.Log10(volume) * 20;
            audioMixer.SetFloat("masterVolume", volumeDB);
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f; // Ensure time is normal when quitting
        pauseScreen.SetActive(false);

        FadeTransition.Instance.FadeToScene(mainMenuSceneName); // Use fade transition

    }

    public void ToggleAudio()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Mute: Set volume to -80dB
            audioMixer.SetFloat("masterVolume", -80f);
            Debug.Log("Muted");
        }
        else
        {
            // Unmute: Restore the original volume
            SetVolume(originalVolume);
            Debug.Log("Unmuted");
        }
    }



    private void EndScreenTextDisplayAndCalculation()
    {
        if (scoreManager == null) return;

        float finalMoney = scoreManager.GetFinalMoney();
        long totalScore = scoreManager.GetTotalScore();
        float highestMult = scoreManager.GetHighestMultiplier();
        int correctWords = scoreManager.GetCorrectWords();
        int messagesSent = scoreManager.GetMessagesSent();
        int mistakes = scoreManager.GetMistakes();
        float accuracy = scoreManager.GetAccuracy();
        float wpm = scoreManager.CalculateWPM();
        long totalFinalScore = totalScore + Mathf.FloorToInt(finalMoney * 0.5f);

        if (finalMoneyText != null)
            finalMoneyText.text = $"Money: {finalMoney}$";
        if (finalScore != null)
            finalScore.text = $"Score: {totalScore}";
        if (finalAccuracyText != null)
            finalAccuracyText.text = $"Accuracy: {accuracy:F1}%";
        if (finalTotalScoreText != null)
            finalTotalScoreText.text = $"Total Score: {totalFinalScore}";
        if (finalWPM != null)
            finalWPM.text = $"WPM: {wpm:F0}";
        if (finalCorrectWords != null)
            finalCorrectWords.text = $"Correct Words: {correctWords}";
        if (finalHighestMult != null)
            finalHighestMult.text = $"Highest Multiplier: {highestMult:F1}x";
        if (finalMessagesSent != null)
            finalMessagesSent.text = $"Messages Sent: {messagesSent}";
        if (finalMistakes != null)
            finalMistakes.text = $"Mistakes: {mistakes}";
    }

    public bool IsStoryMode => currentGameMode is StoryMode;



}