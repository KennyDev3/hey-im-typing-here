using UnityEngine;
using System.Collections;


public class TimeAttackMode : GameMode

{
    [Header("Time Attack Settings")]
    [SerializeField] private GameObject[] phaseChunkPrefabs; // One prefab per phase
    [SerializeField] private GameObject cleanPhasePrefab; // Clean prefab for the initial phase
    [SerializeField] private float phaseTimeDuration = 5f; // Each phase lasts 10 seconds
    [SerializeField] private float[] moveSpeeds = { 5f, 8f, 12f }; // Move speeds for each phase
    [SerializeField] AudioSource soundTrackAudioSource;

    private int currentPhase = 0;

    private LevelGenerator levelGenerator;
    private ObstacleSpawner obstacleSpawner;
    private ChatManager chatManager;
    private TypeAlongTest typeAlongTest;


    private float phaseTimer = 0f;
    private bool isGameStarted = false; // Track if the game has started



    public override void Initialize()
    {
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
        chatManager = FindFirstObjectByType<ChatManager>();
        typeAlongTest = FindFirstObjectByType<TypeAlongTest>();


        if (obstacleSpawner != null)
        {
            obstacleSpawner.enabled = false; // Disable the script, not the GameObject
        }


        if (levelGenerator == null)
        {
            Debug.LogError("LevelGenerator not found in the scene!");
            return;
        }

        // Start with the clean phase
        StartCleanPhase();

        if (typeAlongTest != null)
        {
            // Set the first sentence to "Yes"
            typeAlongTest.SetSpecificSentence("Yes");

            typeAlongTest.OnFirstWordCompleted += OnFirstWordCompleted;
            



        }
        else
        {
            Debug.LogError("TypeAlongTest not found in the scene!");
        }


        


    }

    private void Update()
    {
        UpdateMode();
    }



    private void StartCleanPhase()
    {
        // Set the clean prefab
        GameObject[] cleanChunks = { cleanPhasePrefab };
        levelGenerator.SetChunkPrefabs(cleanChunks);

        // Set a default move speed
        levelGenerator.SetMoveSpeed(moveSpeeds[0]);

        // Send initial messages
        ChatManager chatManager = FindFirstObjectByType<ChatManager>();
        if (chatManager != null)
        {
            StartCoroutine(SendMessagesWithDelay());

        }
        else
        {
            Debug.LogError("ChatManager not found in the scene!");
        }
    }

    private IEnumerator SendMessagesWithDelay()
    {
        // First message
        chatManager.SendFriendMessage("Remember, this is a Deep learning Predictive Generative Big Data LLM AI Phone.");
        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        // Second message
        chatManager.SendFriendMessage("You do not need to add spaces, send messages or write any non-letter characters.");
        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        // Third message
        chatManager.SendFriendMessage("When you are ready to start, type 'Yes'.");
    }

    public void StartGame()
    {
        if (isGameStarted) return; // Ignore if the game has already started


        isGameStarted = true;
        GameManager.Instance.StartGame();


        currentPhase = 0;
        phaseTimer = 0f;
        ConfigureCurrentPhase(); // Switch to the first non-clean phase
        soundTrackAudioSource.Play();


        // Unregister the callback to avoid multiple triggers
        if (chatManager != null)
        {
            chatManager.UnregisterMessageCallback("Yes");
        }
    }

    private void OnFirstWordCompleted()
    {
        // Start the game (e.g., start the timer, enable obstacles, etc.)
        StartGame();
    }

    public override void StartMode()
    {
        // Ensure the game is not in slow motion
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public override void UpdateMode()
    {
        if (!isGameStarted) return; // Don't update if the game hasn't started

        phaseTimer += Time.deltaTime;

        if (phaseTimer >= phaseTimeDuration)
        {
            currentPhase++;

            if (currentPhase >= phaseChunkPrefabs.Length)
            {
                currentPhase = 0;
                Debug.Log("Looping back to phase 1!");
            }

            ConfigureCurrentPhase();
            phaseTimer = 0f;
        }
    }
    

    public override float GetTotalTime()
    {
        return phaseTimeDuration * phaseChunkPrefabs.Length; // Total time for all phases
    }

    public override void EndMode()
    {
        // Nothing special to do here since GameManager.EndGame handles the game over state
    }

    public override void AddTime(float time)
    {
        // This method is no longer needed since the GameManager handles the overall timer
    }

    private void ConfigureCurrentPhase()
    {
        if (obstacleSpawner != null)
        {
            obstacleSpawner.enabled = true; // Enable the script, not the GameObject
        }

        if (levelGenerator == null)
        {
            Debug.LogError("LevelGenerator is null in ConfigureCurrentPhase!");
            return;
        }

        if (phaseChunkPrefabs != null && phaseChunkPrefabs.Length > 0 && currentPhase < phaseChunkPrefabs.Length)
        {
            GameObject[] currentPhaseChunks = { phaseChunkPrefabs[currentPhase] };
            levelGenerator.SetChunkPrefabs(currentPhaseChunks);
            Debug.Log($"Switched to phase {currentPhase + 1} using prefab at index {currentPhase}");
        }
        else
        {
            Debug.LogError("Phase chunk prefabs are missing or index out of range!");
        }

        if (moveSpeeds != null && moveSpeeds.Length > 0 && currentPhase < moveSpeeds.Length)
        {
            levelGenerator.SetMoveSpeed(moveSpeeds[currentPhase]);
            Debug.Log($"Set move speed to {moveSpeeds[currentPhase]} for phase {currentPhase + 1}");
        }
        else
        {
            Debug.LogError("Move speeds are missing or index out of range!");
        }


    }
}