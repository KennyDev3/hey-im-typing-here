using UnityEngine;
using System.Collections;

public class PlayerCollisionHandler : MonoBehaviour
{


    [SerializeField] Animator animator;
    public float cooldownDuration = 1.5f; // Duration of the cooldown in seconds
    private float cooldownTimer = 0f; // Timer to track the cooldown


    [Header("Flashing Effect")]
    [SerializeField] GameObject playerModel;
    [SerializeField] SkinnedMeshRenderer modelRenderer;
    [SerializeField] ScoreManager scoreManager;


    [Header("Hit Stop")]
    [SerializeField] private float hitStopDuration = 0.1f; // Duration of the hit stop effect

    public float flashInterval = 0.1f; // Initial time between flashes
 

    const string HAZARD_TAG = "Hazard";
    const string OBSTACLE_TAG = "Obstacle";

    private bool isImmune = false; // Track if the player is immune
    private float shieldCooldownTimer = 0f;
    private float shieldooldownDuration = 1f; // Cooldown duration after shield is destroyed


    private void OnEnable()
    {
        // Subscribe to shield events
        GameShield.OnShieldActivated += EnableImmunity;
        GameShield.OnShieldDestroyed += StartGracePeriod;
        ShieldController.OnShieldTimedOut += StartGracePeriod;
    }

    private void OnDisable()
    {
        // Unsubscribe from shield events
        GameShield.OnShieldActivated -= EnableImmunity;
        GameShield.OnShieldDestroyed -= StartGracePeriod;
        ShieldController.OnShieldTimedOut += StartGracePeriod;

    }


    void Start()
    {
        cooldownTimer = 1.5f;

       


    }

    void Update()
    {
            cooldownTimer += Time.deltaTime;

        if (shieldCooldownTimer < shieldooldownDuration)
        {
            shieldCooldownTimer += Time.deltaTime;
        }

    }

 

    private void OnCollisionEnter(Collision collision)
    {
       

        if (isImmune || cooldownTimer < cooldownDuration) return;



        if (collision.gameObject.CompareTag(HAZARD_TAG))
        {
           
            StartCoroutine(ApplyHitStop()); // Apply hit stop
            PlayStumbleAnimation();
            PlayerAudioManager.Instance.PlayRandomHazardSound(); // Audio managed on PlayerAudioManager
            StartCoroutine(FlashPlayer()); // Start flashing effect


        }

        if (collision.gameObject.CompareTag(OBSTACLE_TAG))
        {
            StartCoroutine(ApplyHitStop()); // Apply hit stop
            PlayHitAnimation();
            PlayerAudioManager.Instance.PlayRandomObstacleSound(); // Audio managed on PlayerAudioManager
            StartCoroutine(FlashPlayer()); // Start flashing effect

        }
        StartCoroutine(FlashPlayer()); // Start flashing effect
        cooldownTimer = 0f;
        scoreManager.ResetMultiplier();
    }





    public void PlayStumbleAnimation()
    {
        if (!PlayerController.isAnimationPlaying) // Check if no animation is playing
        {
            animator.SetTrigger("Stumble"); // Play the stumble animation
            PlayerController.isAnimationPlaying = true; // Lock animations


        }
    }

    public void PlayHitAnimation()
    {
        if (!PlayerController.isAnimationPlaying) // Check if no animation is playing
        {
            animator.SetTrigger("Hit"); // Play the hit animation
            PlayerController.isAnimationPlaying = true; // Lock animations


        }
    }

    private IEnumerator ApplyHitStop()
    {
        Time.timeScale = 0f; // Pause the game
        yield return new WaitForSecondsRealtime(hitStopDuration); // Wait for the hit stop duration
        Time.timeScale = 1f; // Resume the game
    }



    private void EnableImmunity()
    {
        isImmune = true;
    }

    private void DisableImmunity()
    {
        isImmune = false;
    }

    private void StartGracePeriod()
    {
        isImmune = true; // Player is immune during grace period
        Invoke(nameof(DisableImmunity), 1f); // Disable immunity after 1 second
    }










    private IEnumerator FlashPlayer()
    {
       
        float endTime = Time.time + cooldownDuration;

        while (Time.time < endTime)
        {
            ToggleModelVisibility();
            yield return new WaitForSeconds(flashInterval);

            
        }

        // Ensure the model is visible at the end
        SetModelVisibility(true);
    }

    private void ToggleModelVisibility()
    {
        if (modelRenderer != null)
        {
            modelRenderer.enabled = !modelRenderer.enabled;
        }
    }

    private void SetModelVisibility(bool isVisible)
    {
        if (modelRenderer != null)
        {
            modelRenderer.enabled = isVisible;
        }
    }




}
