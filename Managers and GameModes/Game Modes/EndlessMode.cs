using UnityEngine;
using System.Collections;


public class EndlessMode : GameMode
{
    [Header("Endless Mode Settings")]
    [SerializeField] private GameObject[] chunkPrefabs; // Three chunk prefabs to cycle through
    [SerializeField] private GameObject cleanPhasePrefab; // Clean prefab for the initial phase
    [SerializeField] private int chunksPerPhase = 8; // Number of chunks before switching to the next prefab
    [SerializeField] private int moveSpeed = 7;


    private LevelGenerator levelGenerator;
    private ObstacleSpawner obstacleSpawner;
    private ChatManager chatManager;
    private TypeAlongTest typeAlongTest;

    private int currentChunkIndex = 0; // Tracks the current chunk in the sequence
    private int currentPrefabIndex = 0; // Tracks the current chunk prefab index
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
        levelGenerator.SetMoveSpeed(moveSpeed);

        // Send initial messages
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

        // Start with the first chunk prefab
        currentChunkIndex = 0;
        currentPrefabIndex = 0;
        ConfigureCurrentChunk();

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

        // Check if it's time to switch to the next chunk prefab
        if (currentChunkIndex >= chunksPerPhase)
        {
            currentChunkIndex = 0;
            currentPrefabIndex = (currentPrefabIndex + 1) % chunkPrefabs.Length; // Cycle through prefabs
            ConfigureCurrentChunk();
        }
    }

    public override float GetTotalTime()
    {
        return 0f; // Endless mode has no total time
    }

    public override void EndMode()
    {
        // Nothing special to do here since GameManager.EndGame handles the game over state
    }

    public override void AddTime(float time)
    {
        // This method is no longer needed since the timer increments endlessly
    }


    private void ConfigureCurrentChunk()
    {
        if (obstacleSpawner != null)
        {
            obstacleSpawner.enabled = true; // Enable the script, not the GameObject
        }

        if (levelGenerator == null)
        {
            Debug.LogError("LevelGenerator is null in ConfigureCurrentChunk!");
            return;
        }

        if (chunkPrefabs != null && chunkPrefabs.Length > 0)
        {
            GameObject[] currentChunks = { chunkPrefabs[currentPrefabIndex] };
            levelGenerator.SetChunkPrefabs(currentChunks);
            Debug.Log($"Switched to chunk prefab {currentPrefabIndex + 1}");
        }
        else
        {
            Debug.LogError("Chunk prefabs are missing!");
        }
    }






}









