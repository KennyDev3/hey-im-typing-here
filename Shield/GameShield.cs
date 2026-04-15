using UnityEngine;
using System;


public class GameShield : MonoBehaviour
{
    [SerializeField] GameObject destryoedShieldParticle;

    [Header("Audio References")]

    [SerializeField] AudioClip activateShieldSound;
    [SerializeField] AudioClip deactivateShieldSound;
    

    [SerializeField] AudioSource audioSource;


    public const string HAZARD_TAG = "Hazard";
    public const string OBSTACLE_TAG = "Obstacle";

    public static event Action OnShieldActivated; // Event for shield activation
    public static event Action OnShieldDestroyed; // Event for shield destruction

    private float shieldDuration; // Duration of the shield
    private bool isShieldDestroyed = false; // Track if the shield has been destroyed

    private GameObject particlesChild; // Reference to the "ParticlesParent" child
    private ParticleSystem[] particleSystems; // Array to store all particle systems

    
    public void Initialize(float duration)
    {
        audioSource = GetComponent<AudioSource>();
        shieldDuration = duration;

        // Get all ParticleSystem components in this GameObject and its children
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // Find the "ParticlesParent" child GameObject
        particlesChild = transform.Find("ParticlesParent")?.gameObject;

        if (particlesChild == null)
        {
            Debug.LogError("Child 'ParticlesParent' not found in the shield prefab!");
        }

        audioSource.PlayOneShot(activateShieldSound);

      

        // Notify that the shield is active
        OnShieldActivated?.Invoke();

        // Destroy the shield after the specified duration
        Destroy(gameObject, shieldDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for collisions with the specified tags
        if ((other.CompareTag(HAZARD_TAG) || other.CompareTag(OBSTACLE_TAG)) && !isShieldDestroyed)
        {
            // Mark the shield as destroyed
            isShieldDestroyed = true;

            audioSource.PlayOneShot(deactivateShieldSound);

            // Notify that the shield is destroyed
            OnShieldDestroyed?.Invoke();

            // Destroy the "ParticlesParent" child GameObject
            DestroyParticlesChild();

            // Play the destroyed shield particle effect
            PlayDestroyedShieldParticles();

            // Destroy the parent shield object after a delay
            Destroy(gameObject, 1f); // Adjust the delay as needed
        }
    }


    private void PlayDestroyedShieldParticles()
    {
        Vector3 spawnPosition = transform.position + new Vector3(0, 1, 0); // Increase Y by 1

        GameObject particleInstance = Instantiate(destryoedShieldParticle, spawnPosition, Quaternion.identity, transform);
        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();

        if (particleSystem != null)
        {
            particleSystem.Play();
            // Destroy the particle system after it finishes playing
            Destroy(particleInstance, 1f);
        }

    }

    private void DestroyParticlesChild()
    {
        if (particlesChild != null)
        {
            Destroy(particlesChild);
            Debug.Log("Destroyed 'ParticlesParent.");
        }
    }


   

    private void OnDestroy()
    {
        // Cleanup or additional logic when the shield is destroyed
        Debug.Log("Shield destroyed.");
    }
}