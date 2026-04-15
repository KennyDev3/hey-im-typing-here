using UnityEngine;
using System.Collections.Generic;

public class BuildingSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> buildingPrefabs; // Single list for all buildings
    [SerializeField] private float buildingOffset = 2f; // Distance from the chunk edge outward
    [SerializeField] private float duplicateChance = 0.2f; // 20% chance to duplicate a building


    private GameObject lastSpawnedBuilding = null;

    public void SpawnBuildings(Transform chunkTransform)
    {
        float chunkWidth = 20f; // Chunk width is 14 units
        SpawnLeftBuilding(chunkTransform, chunkWidth);
        SpawnRightBuilding(chunkTransform, chunkWidth);
    }

    private void SpawnLeftBuilding(Transform chunkTransform, float chunkWidth)
    {
        float edgePosition = -chunkWidth / 2; // Left edge of the chunk (-7)
        Vector3 spawnPosition = new Vector3(edgePosition - buildingOffset, 0, chunkTransform.position.z);
        Quaternion rotation = Quaternion.Euler(0, 270, 0); // Left buildings face right (270 degrees)

        SpawnBuilding(chunkTransform, spawnPosition, rotation);
    }

    private void SpawnRightBuilding(Transform chunkTransform, float chunkWidth)
    {
        float edgePosition = chunkWidth / 2; // Right edge of the chunk (7)
        Vector3 spawnPosition = new Vector3(edgePosition + buildingOffset, 0, chunkTransform.position.z);
        Quaternion rotation = Quaternion.Euler(0, 90, 0); // Right buildings face left (90 degrees)

        SpawnBuilding(chunkTransform, spawnPosition, rotation);
    }

    private void SpawnBuilding(Transform chunkTransform, Vector3 position, Quaternion rotation)
    {
        GameObject buildingPrefab = ChooseBuildingPrefab();
        Instantiate(buildingPrefab, position, rotation, chunkTransform);
    }

    private GameObject ChooseBuildingPrefab()
    {
        if (lastSpawnedBuilding != null && Random.value < duplicateChance)
        {
            return lastSpawnedBuilding;
        }

        int randomIndex = Random.Range(0, buildingPrefabs.Count);
        lastSpawnedBuilding = buildingPrefabs[randomIndex];
        return lastSpawnedBuilding;
    }
}
