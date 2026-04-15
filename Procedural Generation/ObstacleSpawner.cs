using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] obstaclePrefabs;
    [SerializeField] float obstacleSpawnTime = 1f;
    [SerializeField] Transform obstaclesParent;
    [SerializeField] float spawnWidth = 4f;
    [SerializeField] float kickForce = 10f; // Serialized field for the kick force

    private Coroutine spawnCoroutine; // Reference to the coroutine
    private int[] activeObstacleIndices; // Indices of obstacles that should be spawned


    private void OnEnable()
    {
        // Start the coroutine when the script is enabled
        spawnCoroutine = StartCoroutine(SpawnObstacleCoroutine());
    }

    private void OnDisable()
    {
        // Stop the coroutine when the script is disabled
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    public void SetObstacleSpawnTime(float spawnTime)
    {
        obstacleSpawnTime = spawnTime;
    }

    public void SetActiveObstacles(int[] obstacleIndices)
    {
        activeObstacleIndices = obstacleIndices;
        Debug.Log("Active obstacles updated.");
    }

    IEnumerator SpawnObstacleCoroutine()
    {
        while (true)
        {
            if (activeObstacleIndices != null && activeObstacleIndices.Length > 0)
            {
                // Select a random index from the active obstacles
                int randomIndex = activeObstacleIndices[Random.Range(0, activeObstacleIndices.Length)];
                GameObject obstaclePrefab = obstaclePrefabs[randomIndex];

                // Calculate a random spawn position within the specified width
                Vector3 spawnPosition = new Vector3(Random.Range(-spawnWidth, spawnWidth), transform.position.y, transform.position.z);

                // Instantiate the obstacle
                GameObject spawnedObstacle = Instantiate(obstaclePrefab, spawnPosition, Random.rotation, obstaclesParent);

                // Apply a kick force to the obstacle
                ApplyKickForce(spawnedObstacle);

                // Destroy the obstacle after a delay
                Destroy(spawnedObstacle, 4f);
            }

            // Wait for the next spawn
            yield return new WaitForSeconds(obstacleSpawnTime);
        }
    }

    private void ApplyKickForce(GameObject obstacle)
    {
        // Get the Rigidbody component of the obstacle
        Rigidbody rb = obstacle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Calculate the direction toward the player (negative Z-axis in this case)
            Vector3 forceDirection = -obstacle.transform.forward;

            // Apply the force to the Rigidbody
            rb.AddForce(forceDirection * kickForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Obstacle prefab does not have a Rigidbody component.");
        }
    }
}