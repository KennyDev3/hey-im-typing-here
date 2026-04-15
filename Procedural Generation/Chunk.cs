using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Chunk : MonoBehaviour
{
    // Enum to represent what's in a grid cell
    private enum GridCellType { Empty, Block, Money, Coffee, Multiplier }

    // --- Prefabs & References ---
    [Header("Object Prefabs")]
    [SerializeField] private GameObject blockHazardPrefab;
    [SerializeField] private GameObject coffeePrefab;
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private GameObject multiplierPrefab;

    [Header("Spawners & Managers")]
    [SerializeField] private BuildingSpawner buildingSpawner;
    private LevelGenerator levelGenerator;
    private ScoreManager scoreManager;

    // --- Spawning Configuration ---
    [Header("Pickup Spawn Chances")]
    [SerializeField] private float moneySpawnChance = 0.6f; // Overall chance for a money sequence attempt
    [SerializeField] private float coffeeSpawnChance = 0.2f;
    [SerializeField] private float multiplierSpawnChance = 0.1f;

    [Header("Hazard Spawn Chances")]
    [SerializeField] private float blockSpawnChancePerLane = 0.15f; // Chance per lane at the chosen Z row

    [Header("Layout Settings")]
    [SerializeField] private float[] lanes = { -16f, 0f, 16f };
    [SerializeField] private float chunkLength = 10f;
    [SerializeField] private float startBufferZ = 1.0f; // Adjusted buffer slightly for grid
    [SerializeField] private float endBufferZ = 1.0f;   // Adjusted buffer slightly for grid
    [SerializeField] private float moneySeparationLength = 0f; // Z distance between money - NOW DETERMINED BY GRID CELL SIZE
    [SerializeField] private float pickupYOffset = 0.6f;
    [SerializeField] private float blockHazardYOffset = 0.009f;

    // --- Grid Settings ---
    private const int NumLanes = 3; // Should match lanes.Length
    private const int NumZPositions = 5; // Number of longitudinal grid spots
    private GridCellType[,] grid = new GridCellType[NumLanes, NumZPositions];
    private float zCellSpacing; // Calculated distance between Z grid centers

    // --- Internal State ---
    private bool hasSpawnedContents = false;


    // Init is called by LevelGenerator
    public void Init(LevelGenerator lg, ScoreManager sm)
    {
        levelGenerator = lg;
        scoreManager = sm;
        ResetChunkState();
        CalculateGridSpacing();
    }

    void ResetChunkState()
    {
        hasSpawnedContents = false;
        // Initialize grid to Empty
        for (int lane = 0; lane < NumLanes; lane++)
        {
            for (int zPos = 0; zPos < NumZPositions; zPos++)
            {
                grid[lane, zPos] = GridCellType.Empty;
            }
        }
        // If pooling: Destroy previously spawned children here
    }

    void CalculateGridSpacing()
    {
        float spawnableZLength = chunkLength - startBufferZ - endBufferZ;
        if (NumZPositions > 0)
        {
            zCellSpacing = spawnableZLength / NumZPositions;
        }
        else
        {
            zCellSpacing = spawnableZLength; // Avoid division by zero
        }
        // Update money separation based on grid (can be removed if not needed elsewhere)
        moneySeparationLength = zCellSpacing;
    }

    // Start is called automatically ONCE
    void Start()
    {
        if (hasSpawnedContents) return;

        PlanSpawnsOnGrid();   // Decide what goes where first
        InstantiateFromGrid(); // Then create the objects
        SpawnBuildings();      // Buildings are separate for now
        hasSpawnedContents = true;
    }

    // --- Grid-Based Spawning Logic ---

    void PlanSpawnsOnGrid()
    {
        // --- Step 1: Place Blocks ---
        // Decide on ONE Z-row index for potential blocks this chunk
        int blockZIndex = Random.Range(0, NumZPositions);
        for (int laneIndex = 0; laneIndex < NumLanes; laneIndex++)
        {
            if (Random.value < blockSpawnChancePerLane)
            {
                // Mark cell as Block, assuming nothing else is there yet (blocks go first)
                grid[laneIndex, blockZIndex] = GridCellType.Block;
            }
        }

        // --- Step 2: Place Pickups (Money, Coffee, Multiplier) in ONE lane ---
        int pickupLaneIndex = Random.Range(0, NumLanes);

        // Try placing Money Sequence
        TryPlaceMoneySequenceOnGrid(pickupLaneIndex);

        // Try placing Coffee
        if (Random.value < coffeeSpawnChance)
        {
            TryPlaceSingleItemOnGrid(pickupLaneIndex, GridCellType.Coffee);
        }

        // Try placing Multiplier
        if (Random.value < multiplierSpawnChance)
        {
            TryPlaceSingleItemOnGrid(pickupLaneIndex, GridCellType.Multiplier);
        }
    }

    // Weighted money count calculation
    private int GetRandomMoneyCount()
    {
        float roll = Random.value;
        if (roll < 0.10f) return 1;
        if (roll < 0.20f) return 2;
        if (roll < 0.50f) return 3;
        if (roll < 0.75f) return 4;
        return 5;
    }

    void TryPlaceMoneySequenceOnGrid(int laneIndex)
    {
        // Check overall chance
        if (Random.value > moneySpawnChance) return;

        int moneyToSpawn = GetRandomMoneyCount();
        if (moneyToSpawn <= 0) return;

        // Find a starting Z index for the sequence in the chosen lane
        List<int> possibleStartIndices = new List<int>();
        for (int startZ = 0; startZ <= NumZPositions - moneyToSpawn; startZ++) // Ensure sequence fits
        {
            bool sequenceFits = true;
            for (int i = 0; i < moneyToSpawn; i++)
            {
                // Check if cell is Empty (Money CAN be adjacent to Blocks)
                if (grid[laneIndex, startZ + i] != GridCellType.Empty)
                {
                    sequenceFits = false;
                    break;
                }
            }
            if (sequenceFits)
            {
                possibleStartIndices.Add(startZ);
            }
        }

        // If valid starting positions exist, pick one randomly
        if (possibleStartIndices.Count > 0)
        {
            int chosenStartIndex = possibleStartIndices[Random.Range(0, possibleStartIndices.Count)];
            // Place the money sequence on the grid
            for (int i = 0; i < moneyToSpawn; i++)
            {
                grid[laneIndex, chosenStartIndex + i] = GridCellType.Money;
            }
        }
    }

    void TryPlaceSingleItemOnGrid(int laneIndex, GridCellType itemType)
    {
        // Find all empty spots in the target lane
        List<int> emptyZIndices = new List<int>();
        for (int zIndex = 0; zIndex < NumZPositions; zIndex++)
        {
            // Coffee/Multipliers CANNOT be adjacent to blocks or other items
            // Check current cell AND neighbours if needed (simple check for now: just current cell empty?)
            // More robust: Check if zIndex-1 and zIndex+1 in this lane are also empty or outside bounds.

            // Refined Check: Cell must be empty, AND not directly adjacent (Z-axis) to a block in ANY lane.
            bool canPlaceHere = grid[laneIndex, zIndex] == GridCellType.Empty;

            if (canPlaceHere && itemType != GridCellType.Money) // Apply block adjacency rule only for non-money pickups
            {
                // Check if there's a block directly above or below in any lane
                for (int checkLane = 0; checkLane < NumLanes; checkLane++)
                {
                    if (grid[checkLane, zIndex] == GridCellType.Block)
                    {
                        canPlaceHere = false;
                        break;
                    }
                }
                // Optional: Check zIndex-1 and zIndex+1 for blocks as well for extra safety?
                // Requires bounds checking. For now, just check same Z row.
            }


            if (canPlaceHere)
            {
                emptyZIndices.Add(zIndex);
            }
        }

        // If empty spots exist, pick one randomly and place the item
        if (emptyZIndices.Count > 0)
        {
            int chosenZIndex = emptyZIndices[Random.Range(0, emptyZIndices.Count)];
            grid[laneIndex, chosenZIndex] = itemType;
        }
    }


    // --- Instantiation Phase ---

    void InstantiateFromGrid()
    {
        for (int laneIndex = 0; laneIndex < NumLanes; laneIndex++)
        {
            for (int zIndex = 0; zIndex < NumZPositions; zIndex++)
            {
                GridCellType cellType = grid[laneIndex, zIndex];
                if (cellType == GridCellType.Empty) continue; // Skip empty cells

                GameObject prefabToSpawn = null;
                float yOffset = pickupYOffset; // Default to pickup offset

                switch (cellType)
                {
                    case GridCellType.Block:
                        prefabToSpawn = blockHazardPrefab;
                        yOffset = blockHazardYOffset;
                        break;
                    case GridCellType.Money:
                        prefabToSpawn = moneyPrefab;
                        break;
                    case GridCellType.Coffee:
                        prefabToSpawn = coffeePrefab;
                        yOffset = 0f; // Assuming coffee has its own base offset or is 0
                        break;
                    case GridCellType.Multiplier:
                        prefabToSpawn = multiplierPrefab;
                        break;
                }

                if (prefabToSpawn != null)
                {
                    Vector3 worldPos = GetWorldPosition(laneIndex, zIndex, yOffset);
                    GameObject spawnedItem = Instantiate(prefabToSpawn, worldPos, Quaternion.identity, this.transform);

                    // Initialize scripts if needed
                    if (cellType == GridCellType.Money && spawnedItem.TryGetComponent<Money>(out var moneyScript) && scoreManager != null)
                    {
                        moneyScript.Init(scoreManager);
                    }
                    else if (cellType == GridCellType.Coffee && spawnedItem.TryGetComponent<Coffee>(out var coffeeScript) && levelGenerator != null)
                    {
                        coffeeScript.Init(levelGenerator);
                    }
                    else if (cellType == GridCellType.Multiplier && spawnedItem.TryGetComponent<Multiplier>(out var multiScript) && scoreManager != null)
                    {
                        multiScript.Init(scoreManager);
                    }
                }
            }
        }
    }

    // Helper to convert grid coordinates to world position
    Vector3 GetWorldPosition(int laneIndex, int zIndex, float yOffset)
    {
        // Calculate the Z position for the center of the grid cell
        float zPos = transform.position.z + startBufferZ + (zIndex * zCellSpacing) + (zCellSpacing * 0.5f);
        // Get the X position from the lanes array
        float xPos = lanes[laneIndex];
        // Get the base Y position and add the specific offset
        float yPos = transform.position.y + yOffset;

        return new Vector3(xPos, yPos, zPos);
    }

    // --- Building Spawning (Separate) ---
    private void SpawnBuildings()
    {
        if (buildingSpawner != null)
        {
            buildingSpawner.SpawnBuildings(this.transform);
        }
    }

} // End of Chunk class