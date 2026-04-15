using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private float speed;
    private Vector3 movementDirection;

    public void Initialize(float carSpeed)
    {
        speed = carSpeed;
        movementDirection = Vector3.forward; // Moving forward

        // Rotate 180 degrees to face player
        transform.rotation = Quaternion.Euler(0, 180f, 0);

        // Destroy the GameObject after 5 seconds
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.Translate(movementDirection * speed * Time.deltaTime);
    }
}