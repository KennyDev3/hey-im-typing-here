using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public Vector3 Offset = new Vector3(0.5f, 1, 0);
    public float randomXRange = 2f; // This will allow random values from -2 to 2
    public float destroyAfterSeconds = 2f;

    void Start()
    {
        // First apply the base offset
        transform.position += Offset;

        // Then only randomize the X position
        float randomX = Random.Range(-randomXRange, randomXRange);
        transform.position += new Vector3(randomX, 0, 0);

        Destroy(gameObject, destroyAfterSeconds);

    }

    void Update()
    {

    }
}