using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivingObstacleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPattern
    {
        public string patternName;
        public LanePosition[] lanes;
        public float delayAfterPattern = 1f;
    }

    public enum LanePosition { Left, Middle, Right, None }

    [Header("Lane Settings")]
    public float laneWidth = 3f;
    public float spawnDistance = 50f;
    public Transform spawnOrigin;

    [Header("Spawn Settings")]
    public List<GameObject> carPrefabs;
    public float minSpeed = 15f;
    public float maxSpeed = 25f;

    [Header("Patterns")]
    public List<SpawnPattern> patterns;

    private Dictionary<LanePosition, Vector3> lanePositions;
    private Coroutine spawnCoroutine;
    private List<GameObject> activeCars = new List<GameObject>();

    private void Awake()
    {
        InitializeLanes();
        this.enabled = false; // Explicitly start disabled
    }

    private void OnEnable()
    {
        Debug.Log("DrivingObstacleSpawner Enabled");
        spawnCoroutine = StartCoroutine(SpawnPatternsRoutine());
    }

    private void OnDisable()
    {
        Debug.Log("DrivingObstacleSpawner Disabled");
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        ClearAllCars();
    }

    private void InitializeLanes()
    {
        lanePositions = new Dictionary<LanePosition, Vector3>
        {
            { LanePosition.Left, spawnOrigin.position + spawnOrigin.right * -laneWidth },
            { LanePosition.Middle, spawnOrigin.position },
            { LanePosition.Right, spawnOrigin.position + spawnOrigin.right * laneWidth }
        };
    }

    private IEnumerator SpawnPatternsRoutine()
    {
        // Remove the while(true) loop to only run once
        foreach (var pattern in patterns)
        {
            yield return StartCoroutine(SpawnPatternRoutine(pattern));
        }

        yield return new WaitForSeconds(7f); // Adjust delay as needed


        // Optionally disable the spawner after completing all patterns
        this.enabled = false;
    }

    private IEnumerator SpawnPatternRoutine(SpawnPattern pattern)
    {
        if (carPrefabs.Count == 0) yield break; // Exit if no prefabs available

        // Pick a random car prefab for this entire pattern
        GameObject selectedCarPrefab = carPrefabs[Random.Range(0, carPrefabs.Count)];
        LanePosition previousLane = LanePosition.None;

        foreach (LanePosition currentLane in pattern.lanes)
        {
            if (currentLane != LanePosition.None)
            {
                // Add delay if same lane as previous
                if (currentLane == previousLane)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                SpawnCarInLane(currentLane, selectedCarPrefab); // Pass the selected prefab
                previousLane = currentLane;
            }
            yield return null;
        }
        yield return new WaitForSeconds(pattern.delayAfterPattern);
    }

    private void SpawnCarInLane(LanePosition lane, GameObject carPrefab)
    {
        Vector3 spawnPos = lanePositions[lane] + spawnOrigin.forward * spawnDistance;
        Quaternion rotation = Quaternion.Euler(0, 180f, 0);

        GameObject car = Instantiate(carPrefab, spawnPos, rotation, transform);
        activeCars.Add(car);

        CarMovement movement = car.GetComponent<CarMovement>() ?? car.AddComponent<CarMovement>();
        movement.Initialize(Random.Range(minSpeed, maxSpeed));
    }

    private void ClearAllCars()
    {
        foreach (var car in activeCars)
        {
            if (car != null) Destroy(car);
        }

        activeCars.Clear();
    }

    [ContextMenu("Create Snake Pattern")]
    public void CreateSnakePattern()
    {
        SpawnPattern snakePattern = new SpawnPattern
        {
            patternName = "Snake",
            lanes = new LanePosition[] {
                LanePosition.Left, LanePosition.Middle, LanePosition.Right,
                LanePosition.Right, LanePosition.Middle, LanePosition.Left
            },
            delayAfterPattern = 2f
        };
        patterns.Add(snakePattern);
    }
}