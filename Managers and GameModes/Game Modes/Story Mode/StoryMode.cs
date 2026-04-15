using UnityEngine;
using System.Collections.Generic;

public class StoryMode : GameMode
{
    [Header("Story Mode Settings")]
    [SerializeField] private float movementSpeed = 6f; // Movement speed for the level
    [SerializeField] private List<StorySequence> storySequences; // List of StorySequence Scriptable Objects
    [SerializeField] private GameObject chunkPrefab; // Chunk prefab for testing

    [Header("Testing")]
    [SerializeField] private bool isTestMode = false; // Enable this for testing the scene independently

    private LevelGenerator levelGenerator;
    private ChatManager chatManager;
    private TypeAlongTest typeAlongTest;
    private StoryManager storyManager;
    private ObstacleSpawner obstacleSpawner; // Reference to the obstacle spawner
    private DrivingObstacleSpawner drivingObstacleSpawner;
    private GameFreezeManager gameFreezeManager;
    private GameManager gameManager;
    public AdaptiveMusicManager musicManager;

    private int currentSequenceIndex = 0; // Tracks the current story sequence

    public override void Initialize()
    {
        obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
        drivingObstacleSpawner = FindFirstObjectByType<DrivingObstacleSpawner>();
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        chatManager = FindFirstObjectByType<ChatManager>();
        typeAlongTest = FindFirstObjectByType<TypeAlongTest>();
        gameFreezeManager = FindFirstObjectByType<GameFreezeManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        musicManager = FindFirstObjectByType<AdaptiveMusicManager>();



        gameManager.StartGame();
        // Start with obstacle spawner off
        if (obstacleSpawner != null)
        {
            obstacleSpawner.enabled = false; // Disable the spawner script
        }


        if (drivingObstacleSpawner != null)
        {
            drivingObstacleSpawner.enabled = false;
        }


        // If in test mode, set the game mode to "Story" manually
        if (isTestMode)
        {
            GameController.Instance.SetGameMode(GameController.GAME_MODE_STORY);
        }

       

        if (levelGenerator == null || chatManager == null || typeAlongTest == null)
        {
            Debug.LogError("Required components not found in the scene!");
            return;
        }

        // Set movement speed
        levelGenerator.SetMoveSpeed(movementSpeed);

        // Set the chunk prefab for testing
        if (chunkPrefab != null)
        {
            GameObject[] chunks = { chunkPrefab };
            levelGenerator.SetChunkPrefabs(chunks);
        }

        // Initialize the story manager with the first sequence
        if (storySequences != null && storySequences.Count > 0)
        {
            storyManager = new StoryManager(storySequences[currentSequenceIndex].storyEvents, chatManager, typeAlongTest, obstacleSpawner, levelGenerator, drivingObstacleSpawner, this);
            storyManager.StartStory();
        }
        else
        {
            Debug.LogError("No StorySequence Scriptable Objects are assigned!");
        }
    }

    private void Update()
    {
        UpdateMode();
    }

    public override void StartMode()
    {
        // Ensure the game is not in slow motion
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public override void UpdateMode()
    {
        // Check if the current story sequence is complete
        if (storyManager != null && storyManager.IsSequenceComplete())
        {
            // Move to the next story sequence
            currentSequenceIndex++;
            if (currentSequenceIndex < storySequences.Count)
            {
                storyManager = new StoryManager(storySequences[currentSequenceIndex].storyEvents, chatManager, typeAlongTest, obstacleSpawner, levelGenerator, drivingObstacleSpawner, this);
                storyManager.StartStory();
            }
            else
            {
                //Debug.Log("All story sequences completed!");
                // Handle end of story logic (e.g., transition to another scene)
            }
        }
    }

    public void SetPlayerMoveMentSpeed(float speed)
    {
        movementSpeed = speed;
        if (levelGenerator != null)
        {
            levelGenerator.SetMoveSpeed(movementSpeed); // Apply the new speed
        }
    }



    public override float GetTotalTime()
    {
        return 0f; // Story mode has no total time
    }

    public override void EndMode()
    {
        // Nothing special to do here since GameManager.EndGame handles the game over state
    }

    public override void AddTime(float time)
    {
        // This method is not needed for story mode
    }
}