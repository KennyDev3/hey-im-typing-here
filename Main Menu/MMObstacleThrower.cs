using System.Collections;
using UnityEngine;


public class MMObstacleThrower : MonoBehaviour

{
    [Header("Spawn Settings")]
    [Tooltip("Prefabs of cars to throw")]
    [SerializeField] private GameObject[] carPrefabs;

    [Tooltip("Initial delay before first car is thrown (seconds)")]
    [SerializeField] private float initialDelay = 2f;

    [Tooltip("Time between throwing cars (seconds)")]
    [SerializeField] private float throwInterval = 1.5f;

    [Header("Throw Settings")]
    [Tooltip("Force applied to thrown cars")]
    [SerializeField] private float throwForce = 15f;

    [Tooltip("Random rotation angle range for prefab (-value to +value degrees)")]
    [SerializeField] private float prefabRotationRange = 15f;

    [Tooltip("Random position offset range (-value to +value units)")]
    [SerializeField] private float positionOffset = 1f;

    [Tooltip("Direction to throw objects (world space, ignores cube orientation)")]
    [SerializeField] private Vector3 throwDirection = Vector3.right;

    void Start()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("No car prefabs assigned to the CarThrower!");
            return;
        }

        // Start throwing cars immediately and continuously
        StartCoroutine(ThrowCarsRoutine());
    }

    IEnumerator ThrowCarsRoutine()
    {
        // Wait for initial delay
        yield return new WaitForSeconds(initialDelay);

        // Run forever, no condition to stop
        while (true)
        {
            ThrowCar();

            // Wait for the interval before throwing the next car
            yield return new WaitForSeconds(throwInterval);
        }
    }

    private void ThrowCar()
    {
        // Get a random car prefab from the array
        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

        // Create random position offset
        Vector3 spawnPositionOffset = new Vector3(
            Random.Range(-positionOffset, positionOffset),
            Random.Range(-positionOffset, positionOffset),
            Random.Range(-positionOffset, positionOffset)
        );

        // Create random rotation for the prefab itself
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-prefabRotationRange, prefabRotationRange),
            Random.Range(-prefabRotationRange, prefabRotationRange),
            Random.Range(-prefabRotationRange, prefabRotationRange)
        );

        // Create the car at the launch point with position offset and random rotation
        GameObject car = Instantiate(carPrefab, transform.position + spawnPositionOffset, randomRotation);

        // Get the rigidbody component of the car
        Rigidbody carRigidbody = car.GetComponent<Rigidbody>();

        // If the car doesn't have a rigidbody, add one
        if (carRigidbody == null)
        {
            carRigidbody = car.AddComponent<Rigidbody>();
        }

        // Make sure we're not inheriting any weird physics from the prefab
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;

        // Normalize the throw direction
        Vector3 normalizedThrowDirection = throwDirection.normalized;

        // Apply force in the specified world direction (ignoring cube orientation)
        carRigidbody.AddForce(normalizedThrowDirection * throwForce, ForceMode.Impulse);

        // Destroy the car after 3 seconds
        Destroy(car, 10f);
    }
}


