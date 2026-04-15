using UnityEngine;

[System.Serializable]
public class GameplayChange
{
    public bool changeChunks; // Whether to change chunks
    public GameObject[] newChunks; // New chunk prefabs

    public bool changeObstacleSpawner; // Whether to change obstacle spawner
    public bool enableObstacleSpawner; // Whether to enable/disable the spawner
    public float newSpawnTime; // New spawn time (if applicable)
    public int[] obstaclesToSpawn; // Indices of obstacles to spawn (e.g., 0 for buses, 1 for cars, etc.)

    public bool changeDrivingSpawner;
    public bool enableDrivingSpawner;



    public bool changeStoryModeMoveSpeed;
    public float newMovementSpeed;

    public bool freezeGame;
    public bool unFreezeGame;

    public bool endGame;
}