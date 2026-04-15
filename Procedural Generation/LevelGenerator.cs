using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    private GameObject[] chunkPrefabs; // Current chunk prefabs
    private Queue<GameObject[]> chunkPrefabQueue = new Queue<GameObject[]>(); // Queue for new chunk prefabs

    private float moveSpeed;

    [Header("Debugging")]
    [SerializeField] private bool enableCheckpoints = true;

    [Header("References")]
    [SerializeField] GameObject checkPointChunk;
    [SerializeField] Transform chunkParent;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] CameraController cameraController;

    [Header("Level Settings")]
    [SerializeField] int startingChunksAmount = 7;
    [Tooltip("Do not change Chunk length value unless chunk prefab size reflects change")]
    [SerializeField] float chunkLength = 10f;
    [SerializeField] float minMoveSpeed = 5f;
    [SerializeField] float maxMoveSpeed = 12f;
    [SerializeField] float minGravityZ = -18f;
    [SerializeField] float maxGravityZ = -2f;
    [SerializeField] float checkpointChunkInterval = 5;

    private float chunkCountToCheckPoint = 1;
    private List<GameObject> chunks = new List<GameObject>();

    private void Start()
    {
        SpawnInitialChunks();
    }

    private void Update()
    {
        MoveChunks();
    }

    public void SetChunkPrefabs(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("No chunk prefabs provided!");
            return;
        }

        // Immediately set the provided prefabs as the current chunk prefabs
        chunkPrefabs = prefabs;

        Debug.Log($"Chunk prefabs set: {string.Join(", ", System.Array.ConvertAll(prefabs, p => p.name))}");

        // If there are queued prefabs, clear the queue (optional, depending on your design)
        chunkPrefabQueue.Clear();
    }

    // Method to apply queued chunk prefabs
    private void ApplyQueuedChunkPrefabs()
    {
        if (chunkPrefabQueue.Count > 0)
        {
            chunkPrefabs = chunkPrefabQueue.Dequeue();
            Debug.Log($"Chunk prefabs updated to: {string.Join(", ", System.Array.ConvertAll(chunkPrefabs, p => p.name))}");
        }
    }

    private void ClearChunks()
    {
        foreach (var chunk in chunks)
        {
            Destroy(chunk);
        }
        chunks.Clear();
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void ChangeChunkMoveSpeed(float speedAmount)
    {
        float newMoveSpeed = moveSpeed + speedAmount;
        newMoveSpeed = Mathf.Clamp(newMoveSpeed, minMoveSpeed, maxMoveSpeed);

        if (newMoveSpeed != moveSpeed)
        {
            moveSpeed = newMoveSpeed;

            float newGravityZ = Physics.gravity.z - speedAmount;
            newGravityZ = Mathf.Clamp(newGravityZ, minGravityZ, maxGravityZ);

            Physics.gravity = new Vector3(Physics.gravity.x, Physics.gravity.y, Physics.gravity.z - speedAmount);
            cameraController.ChangeCameraFov(speedAmount);
        }
    }

    private void SpawnChunk(Vector3 position)
    {
        GameObject chunkToSpawn = ChooseChunkToSpawn(); // Selects if Checkpoint Chunk or not

        if (chunkToSpawn == null)
        {
            Debug.LogError("Failed to spawn chunk: No valid chunk prefab found.");
            return;
        }

        GameObject newChunkGO = Instantiate(chunkToSpawn, position, Quaternion.identity, chunkParent);
        chunks.Add(newChunkGO);

        Chunk newChunk = newChunkGO.GetComponent<Chunk>();
        newChunk.Init(this, scoreManager);
        chunkCountToCheckPoint++;
    }

    private GameObject ChooseChunkToSpawn()
    {
        if (enableCheckpoints && chunkCountToCheckPoint % checkpointChunkInterval == 0 && chunkCountToCheckPoint != 0)
        {
            return checkPointChunk;
        }
        else
        {
            if (chunkPrefabs != null && chunkPrefabs.Length > 0)
            {
                // If only one chunk prefab is available, always return it
                if (chunkPrefabs.Length == 1)
                {
                    return chunkPrefabs[0];
                }
                else
                {
                    // Randomly select a chunk prefab
                    return chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
                }
            }
            else
            {
                Debug.LogError("No chunk prefabs are assigned to LevelGenerator!");
                return null;
            }
        }
    }


    private void SpawnInitialChunks()
    {
        for (int i = 0; i < startingChunksAmount; i++)
        {
            Vector3 position = new Vector3(0, 0, i * chunkLength);
            SpawnChunk(position);
        }
    }

    private void SpawnOneChunk()
    {
        float newChunkZPosition = chunkLength + chunks[chunks.Count - 1].transform.position.z;
        Vector3 position = new Vector3(0, 0, newChunkZPosition);
        SpawnChunk(position);
    }

    private void MoveChunks()
    {
        if (chunks.Count == 0)
        {
            // If no chunks exist, spawn an initial chunk
            SpawnOneChunk();
            return;
        }

        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = chunks[i];
            chunk.transform.Translate(-transform.forward * (moveSpeed * Time.deltaTime));

            if (chunk.transform.position.z <= Camera.main.transform.position.z - chunkLength)
            {
                chunks.Remove(chunk); // Remove chunk from the list
                Destroy(chunk); // Destroy the chunk object

                // If there are queued chunk prefabs, apply them
                if (chunkPrefabQueue.Count > 0)
                {
                    ApplyQueuedChunkPrefabs();
                }

                // Spawn a new chunk at the end
                SpawnOneChunk();
            }
        }
    }
}