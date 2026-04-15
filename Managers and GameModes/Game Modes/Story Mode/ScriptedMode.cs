using UnityEngine;

public class ScriptedMode : GameMode
{
    [SerializeField] private GameObject[] scriptedChunkPrefabs; // Predefined chunks for the scripted level
    [SerializeField] private float moveSpeed = 8f;

    private LevelGenerator levelGenerator;
    private ObstacleSpawner obstacleSpawner;

    public override void Initialize()
    {
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();

        // Configure LevelGenerator for the scripted level
        levelGenerator.SetChunkPrefabs(scriptedChunkPrefabs);
        levelGenerator.SetMoveSpeed(moveSpeed);
    }

    public override void StartMode()
    {
        // Start the scripted level
    }

    public override void UpdateMode()
    {
        // Update scripted level logic
    }

    public override void EndMode()
    {
        // Trigger game over logic
        GameManager.Instance.EndGame();
    }

    public override void AddTime(float time)
    {
        // Scripted mode might not use time extensions
    }

    public override float GetTotalTime()
    {
        return 0f;
    }
}