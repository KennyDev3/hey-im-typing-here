using Unity.Cinemachine;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public enum ObstacleSize { Small, Medium, Large }

    [Header("Obstacle Settings")]
    [SerializeField] private ObstacleSize size = ObstacleSize.Medium; // Size of the obstacle
    [SerializeField] private bool hasScreenShake = false; // Enable/disable screen shake
    [SerializeField] private bool hasTwoParticleSystems = false; // Enable/disable second particle system

    [Header("References")]
    [SerializeField] private ParticleSystem collisionParticleSystem1; // First particle system
    [SerializeField] private ParticleSystem collisionParticleSystem2; // Second particle system (optional)
    [SerializeField] private AudioSource collisionAudioSource; // Audio source for collision

    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeModifier = 10f; // Intensity of screen shake
    [SerializeField] private float maxCameraShakeAmount = 0.4f; // Maximum screen shake intensity
    private CinemachineImpulseSource cinemachineImpulseSource; // Cinemachine impulse source

    [Header("Collision Settings")]
    [SerializeField] private float collisionFXCooldown = 1.5f; // Cooldown time for collision FX
    private float lastCollisionFXTime = 0f; // Time of the last collision FX

    protected virtual void Awake()
    {
        lastCollisionFXTime = -collisionFXCooldown; // Add this line

        if (hasScreenShake)
        {
            cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
            if (cinemachineImpulseSource == null)
            {
                Debug.LogWarning("CinemachineImpulseSource component missing. Screen shake will not work.");
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        // Check if the cooldown period has passed
        if (Time.time - lastCollisionFXTime >= collisionFXCooldown)
        {
            PlayCollisionFX(other);
            lastCollisionFXTime = Time.time; // Update the last collision FX time
        }

        // Trigger screen shake if enabled
        if (hasScreenShake)
        {
            FireImpulseScreenShake();
        }
    }

    protected virtual void PlayCollisionFX(Collision other)
    {
        // Get the collision contact point
        ContactPoint contactPoint = other.contacts[0];

        // Play the first particle system if it exists
        if (collisionParticleSystem1 != null)
        {
            collisionParticleSystem1.transform.position = contactPoint.point;
            collisionParticleSystem1.Play();
        }

        // Play the second particle system if it exists and is enabled
        if (hasTwoParticleSystems && collisionParticleSystem2 != null)
        {
            collisionParticleSystem2.transform.position = contactPoint.point;
            collisionParticleSystem2.Play();
        }

        // Play collision audio if it exists
        if (collisionAudioSource != null)
        {
            collisionAudioSource.Play();
        }
    }

    protected virtual void FireImpulseScreenShake()
    {
        if (cinemachineImpulseSource != null)
        {
            // Calculate screen shake intensity based on distance and obstacle size
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float shakeIntensity = (1f / distance) * shakeModifier;

            // Adjust shake intensity based on obstacle size
            switch (size)
            {
                case ObstacleSize.Small:
                    shakeIntensity *= 0.5f; // Small obstacles have less shake
                    break;
                case ObstacleSize.Medium:
                    shakeIntensity *= 1f; // Medium obstacles have normal shake
                    break;
                case ObstacleSize.Large:
                    shakeIntensity *= 2f; // Large obstacles have more shake
                    break;
            }

            // Clamp the shake intensity to the maximum allowed value
            shakeIntensity = Mathf.Min(shakeIntensity, maxCameraShakeAmount);

            // Generate the screen shake impulse
            cinemachineImpulseSource.GenerateImpulse(shakeIntensity);
        }
    }
}
