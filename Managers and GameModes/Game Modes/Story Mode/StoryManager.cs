using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class StoryManager
{
    private List<StoryEvent> storyEvents; // List of story events
    private ChatManager chatManager; // Reference to the chat manager
    private TypeAlongTest typeAlongTest; // Reference to the typing test
    private ObstacleSpawner obstacleSpawner; // Reference to the obstacle spawner
    private LevelGenerator levelGenerator;
    private DrivingObstacleSpawner drivingObstacleSpawner;
    private StoryMode storyMode;
 



    private int currentEventIndex = 0;

    // Constructor to initialize the StoryManager
    public StoryManager(List<StoryEvent> events, ChatManager chatManager,
                       TypeAlongTest typeAlongTest, ObstacleSpawner obstacleSpawner,
                       LevelGenerator levelGenerator, DrivingObstacleSpawner drivingObstacleSpawner, StoryMode storyMode)
    {
        this.storyEvents = events;
        this.chatManager = chatManager;
        this.typeAlongTest = typeAlongTest;
        this.obstacleSpawner = obstacleSpawner;
        this.levelGenerator = levelGenerator;
        this.drivingObstacleSpawner = drivingObstacleSpawner;
        this.storyMode = storyMode;



        // Disable automatic random sentence generation in TypeAlongTest
        typeAlongTest.autoSetRandomSentence = false;

        // Subscribe to the sentence completion event
        TypeAlongTest.OnSentenceCompleted += OnSentenceCompleted;
    
    }

    // Method to start the story sequence
    public void StartStory()
    {
        // Start the coroutine to run the story events
        MonoBehaviour coroutineRunner = chatManager.GetComponent<MonoBehaviour>();
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(RunStoryEvents());
        }
        else
        {
            Debug.LogError("Coroutine runner not found!");
        }
    }

    // Coroutine to run the story events
    private IEnumerator RunStoryEvents()
    {
        while (currentEventIndex < storyEvents.Count)
        {
            StoryEvent currentEvent = storyEvents[currentEventIndex];

            // Apply gameplay changes (e.g., obstacle spawner settings)
            ApplyGameplayChanges(currentEvent.gameplayChange);

            // --- Trigger Music Change ---

            if (currentEvent.changeMusicState && currentEvent.musicStateIndex >= 0)
            {
                if (storyMode != null && storyMode.musicManager != null)
                {
                    storyMode.musicManager.TransitionToState(currentEvent.musicStateIndex);
                }

                else
                {
                    if (storyMode == null)
                    {
                        Debug.LogError("StoryMode reference is null in StoryManager!");
                    }
                    else if (storyMode.musicManager == null)
                    {
                        Debug.LogError("Music Manager reference not set in the StoryMode script component (Inspector)!");
                    }
                }
            }



            // --- Handle Chat message Events ---


            if (!string.IsNullOrEmpty(currentEvent.friendMessage))
            {
                chatManager.SendFriendMessage(currentEvent.friendMessage);
            }

            if (!string.IsNullOrEmpty(currentEvent.sentence))
            {
                typeAlongTest.SetSpecificSentence(currentEvent.sentence);
                yield return new WaitUntil(() => typeAlongTest.IsSentenceComplete());
            }

            yield return new WaitForSeconds(currentEvent.delayBeforeNextEvent);
            currentEventIndex++;
        }

        typeAlongTest.ClearInputAndDisplay();
        Debug.Log("Story sequence completed.");

        // Start the next sequence automatically
    }

    private void ApplyGameplayChanges(GameplayChange gameplayChange)
    {
        if (gameplayChange != null)
        {
            // Handle original obstacle spawner (unchanged)
            if (gameplayChange.changeObstacleSpawner && obstacleSpawner != null)
            {
                obstacleSpawner.enabled = gameplayChange.enableObstacleSpawner;

                if (gameplayChange.newSpawnTime > 0)
                {
                    obstacleSpawner.SetObstacleSpawnTime(gameplayChange.newSpawnTime);
                }

                if (gameplayChange.obstaclesToSpawn != null)
                {
                    obstacleSpawner.SetActiveObstacles(gameplayChange.obstaclesToSpawn);
                }
            }

            if (gameplayChange.changeDrivingSpawner && drivingObstacleSpawner != null)
            {
                Debug.Log($"Setting DrivingObstacleSpawner to: {gameplayChange.enableDrivingSpawner}");
                drivingObstacleSpawner.enabled = gameplayChange.enableDrivingSpawner;
            }


            // Existing chunk changes
            if (gameplayChange.changeChunks && levelGenerator != null)
            {
                levelGenerator.SetChunkPrefabs(gameplayChange.newChunks);
            }

            if (gameplayChange.changeStoryModeMoveSpeed && storyMode != null)
            {
                storyMode.SetPlayerMoveMentSpeed(gameplayChange.newMovementSpeed);
            }

            if (gameplayChange.freezeGame)
            {
                GameFreezeManager.Instance.FreezeGame();
                Debug.Log("I am freezing the game");
            }

            if (gameplayChange.unFreezeGame)
            {
                GameFreezeManager.Instance.UnfreezeGame();
            }

            if (gameplayChange.endGame)
            {
                GameManager.Instance.EndGame();
            }


        }

    }



    // Handle sentence completion
    private void OnSentenceCompleted(bool isPerfect)
    {
        // Do nothing here, as the coroutine already handles progression
    }

    // Check if the current story sequence is complete
    public bool IsSequenceComplete()
    {
        return currentEventIndex >= storyEvents.Count;
    }
}