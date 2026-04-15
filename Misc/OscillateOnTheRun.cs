using UnityEngine;

public class OscillateOnTheRun : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3 movementVector;

    Vector3 startPosition;
    Vector3 endPosition;
    float movementFactor = 1f;

    void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + movementVector;
    }

    // Update is called once per frame
    void Update()
    {
        movementFactor = Mathf.PingPong(Time.time * speed, 1f);

        // Only modify the Y position while keeping X and Z unchanged
        transform.position = new Vector3(transform.position.x, Mathf.Lerp(startPosition.y, endPosition.y, movementFactor), transform.position.z);
    }
}
