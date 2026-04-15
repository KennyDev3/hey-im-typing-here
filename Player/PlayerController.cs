using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static bool isAnimationPlaying = false;
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rb;
    [SerializeField] ShieldController shieldController; // Reference to the ShieldController



    [Header("Player Movement")]
    [SerializeField] float playerSpeed = 5f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float jumpDuration = 1f;
    [SerializeField] float laneSwitchSpeed = 5f; // Speed of lane switching
    Vector2 movement;
    bool isJumping = false;
    float jumpTimer = 0f;
    Vector3 jumpStartPosition;
    private bool isMovementAllowed = true;

    // Lane positions
    private float[] lanes = { -2.65f, -0.28f, 2.9f }; // Original lane positions with offset applied
    private int currentLane = 1; // Start in the middle lane
    private float targetXPosition;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        isAnimationPlaying = false;

        // Initialize target position to the middle lane (already includes the offset)
        targetXPosition = lanes[currentLane];
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!isMovementAllowed) return;
        movement = context.ReadValue<Vector2>();

        if (context.performed && !isJumping && movement.y > 0.5f)
        {
            TriggerJump();
        }

        // Handle lane switching
        if (context.performed && Mathf.Abs(movement.x) > 0.5f)
        {
            int direction = movement.x > 0 ? 1 : -1;
            SwitchLane(direction);
            PlayerAudioManager.Instance.PlayChangeLaneSound();
        }

        // Handle shield activation (down key)
        if (context.performed && movement.y < -0.5f)
        {
            shieldController.ActivateShield();
        }
    }

    void SwitchLane(int direction)
    {
        int newLane = currentLane + direction;
        if (newLane >= 0 && newLane < lanes.Length)
        {
            currentLane = newLane;
            targetXPosition = lanes[currentLane];
        }
    }

    void TriggerJump()
    {
        isJumping = true;
        jumpTimer = 0f;
        jumpStartPosition = rb.position;
        PlayJumpAnimation();
        PlayerAudioManager.Instance.PlayJumpSound();
    }

    public void PlayJumpAnimation()
    {
        if (!isAnimationPlaying)
        {
            animator.SetTrigger("Jump");
            isAnimationPlaying = true;
        }
    }

    void UpdatePosition()
    {
        if (!isMovementAllowed) return;

        Vector3 targetPosition = rb.position;

        // Smoothly move to the target lane
        targetPosition.x = Mathf.Lerp(targetPosition.x, targetXPosition, laneSwitchSpeed * Time.fixedDeltaTime);

        // Apply jump if active
        if (isJumping)
        {
            jumpTimer += Time.fixedDeltaTime;
            float jumpProgress = jumpTimer / jumpDuration;

            if (jumpProgress <= 1f)
            {
                float verticalOffset = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
                targetPosition.y = jumpStartPosition.y + verticalOffset;
            }
            else
            {
                isJumping = false;
                targetPosition.y = jumpStartPosition.y;
            }
        }

        // Apply final position
        rb.MovePosition(targetPosition);

        // Update animation
        animator.SetBool("isRunning", Mathf.Abs(targetPosition.x - targetXPosition) > 0.1f);
    }

    public static void ReleaseAnimationLock()
    {
        isAnimationPlaying = false;
    }

    public void AllowMovement()
    {
        isMovementAllowed = true;
    }

    public void DisallowMovement()
    {
        isMovementAllowed = false;
    }
}