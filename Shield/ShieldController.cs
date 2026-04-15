using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShieldController : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] GameObject shieldPrefab;
    [SerializeField] Transform ShieldPowerUp;
    [SerializeField] private Slider cooldownSlider;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownDuration = 8f;
    [SerializeField] private float shieldDuration = 5f;

    private GameObject currentShield;
    private float cooldownTimer;
    private bool isCooldownActive = false;
    private bool isShieldActive = false;

    //Shield Time Out Event for PlayerCollisionHandler
    public static event Action OnShieldTimedOut;


    private void Start()
    {
        // Initialize the slider to be empty at the start
        cooldownSlider.value = 1f;
        isCooldownActive= true;
        
    }

    private void OnEnable()
    {
        // Subscribe to the shield destroyed event
        GameShield.OnShieldDestroyed += OnShieldDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from the shield destroyed event
        GameShield.OnShieldDestroyed -= OnShieldDestroyed;
    }

    private void Update()
    {
        // Only update cooldown when shield is not active
        if (isCooldownActive && !isShieldActive)
        {
            cooldownTimer -= Time.deltaTime;

            // Update the slider value based on the cooldown progress
            cooldownSlider.value = (cooldownDuration - cooldownTimer) / cooldownDuration;

            // Check if the cooldown is complete
            if (cooldownTimer <= 0f)
            {
                CompleteCooldown();
            }
        }
    }

    private void CompleteCooldown()
    {
        isCooldownActive = false;
        cooldownTimer = cooldownDuration;
        cooldownSlider.value = 1f;
    }

    public void ActivateShield()
    {
        // Can only activate shield if cooldown is complete and shield is not active
        if (isCooldownActive || isShieldActive) return;

        // Instantiate the shield at the spawn point
        currentShield = Instantiate(shieldPrefab, ShieldPowerUp.position, ShieldPowerUp.rotation, ShieldPowerUp);

        // Set the shield as active
        isShieldActive = true;

        // Reset slider to 0 when activated
        cooldownSlider.value = 0f;

        // Initialize the shield with its duration
        GameShield gameShield = currentShield.GetComponentInChildren<GameShield>();
        if (gameShield != null)
        {
            gameShield.Initialize(shieldDuration);
        }

        // Start a coroutine to wait for the shield to deactivate
        StartCoroutine(WaitForShieldDeactivation());
    }

    private IEnumerator WaitForShieldDeactivation()
    {
        // Wait for the shield duration to expire
        yield return new WaitForSeconds(shieldDuration);

        // If shield is still active after duration, deactivate it
        Debug.Log("Cory is being called");
            DeactivateShield();
            OnShieldTimedOut?.Invoke();



    }

    private void OnShieldDestroyed()
    {
        // If the shield is destroyed, deactivate it
        if (isShieldActive)
        {
            DeactivateShield();
        }
    }

    public void DeactivateShield()
    {
        
        // Destroy the shield
        //Destroy(currentShield);
        currentShield = null;

        // Set the shield as inactive
        isShieldActive = false;

        // Start cooldown after shield deactivation
        StartCooldown();
    }

    private void StartCooldown()
    {
        // Start the cooldown timer
        isCooldownActive = true;
        cooldownTimer = cooldownDuration;
        Debug.Log("I am being called");
        Debug.Log(isCooldownActive);
    }
}