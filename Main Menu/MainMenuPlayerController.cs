using UnityEngine;

public class MainMenuPlayerController : MonoBehaviour
{
    // Slow Motion Controller

    public float slowMotionFactor = 0.2f; // Adjust this value (e.g., 0.5 for half speed)
    private float originalTimeScale;


    [SerializeField] private float moveSpeed = 5f; // Speed at which the player moves
    [SerializeField] private float leftTurnAroundPoint = -10f; // X position where the player turns around (left side)
    [SerializeField] private float rightTurnAroundPoint = 10f; // X position where the player turns around (right side)
    [SerializeField] private float waitTime = 2f; // Time to wait before turning around
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private int direction = 1; // 1 for moving right, -1 for moving left

    void Start()
    {
        //originalTimeScale = Time.timeScale;
        //ActivateSlowMotion();
    }

    void Update()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                RotatePlayer();
            }
        }
        else
        {
            MovePlayer();
            CheckTurnAround();
        }
    }

    private void MovePlayer()
    {
        // Move the player along the world X axis based on direction
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);
    }

    private void CheckTurnAround()
    {
        // Check if the player has reached the turn-around point
        if (direction == -1 && transform.position.x <= leftTurnAroundPoint)
        {
            StartWaiting();
        }
        else if (direction == 1 && transform.position.x >= rightTurnAroundPoint)
        {
            StartWaiting();
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
    }

    private void RotatePlayer()
    {
        // Rotate the player 180 degrees on the Y-axis
        transform.Rotate(0, 180, 0);
        // Flip the direction
        direction = -direction;
    }

    public void ActivateSlowMotion()
    {
        Time.timeScale = slowMotionFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Important for physics
    }

    public void DeactivateSlowMotion()
    {
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = 0.02f; // Reset to default physics timestep
    }
}

