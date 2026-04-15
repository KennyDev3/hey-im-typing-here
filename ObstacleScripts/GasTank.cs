using Unity.Cinemachine;
using UnityEngine;

public class GasTank : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ParticleSystem collisionParticleSystemDebris; // First particle system
    [SerializeField] ParticleSystem collisionParticleSystemSmoke; // Second particle system
    [SerializeField] AudioSource collisionAudioSource;

    [Header("Variables")]
    [SerializeField] float shakeModifier = 10f;
    [SerializeField] float maxCameraShakeAmount = 0.4f;
    [SerializeField] float collisionFXCooldown = 1.5f; // Cooldown time in seconds
    CinemachineImpulseSource cinemachineImpulseSource;
    private float lastCollisionFXTime = -1f; // Initialize to -1 to allow the first collision


    private void Awake()
    {
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnCollisionEnter(Collision other)
    {
        FireImpulseScreenShake();

        // Check if the cooldown period has passed
        if (Time.time - lastCollisionFXTime >= collisionFXCooldown)
        {
            CollisionFX(other);
            lastCollisionFXTime = Time.time; // Update the last collision FX time
        }
    }

    private void FireImpulseScreenShake()
    {
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float shakeIntensity = (1f / distance) * shakeModifier;
        shakeIntensity = Mathf.Min(shakeIntensity, maxCameraShakeAmount);
        cinemachineImpulseSource.GenerateImpulse(shakeIntensity);
    }

    void CollisionFX(Collision other)
    {
        ContactPoint contactPoint = other.contacts[0];
        collisionParticleSystemSmoke.transform.position = contactPoint.point;
        collisionParticleSystemDebris.transform.position = contactPoint.point;
        collisionAudioSource.Play();
        collisionParticleSystemDebris.Play();
        collisionParticleSystemSmoke.Play();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
