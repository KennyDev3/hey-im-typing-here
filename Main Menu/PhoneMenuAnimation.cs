using System.Collections;
using UnityEngine;

public class PhoneMenuAnimation : MonoBehaviour
{
    public float animationDuration = 1.0f; // Duration of the animation in seconds
    public Vector3 targetPosition; // The final position of the phone on the screen

    private Vector3 initialPosition; // The starting position of the phone (off-screen)

    void Start()
    {
        // Store the initial position (off-screen to the left)
        initialPosition = transform.position;

        // Start the animation coroutine
        StartCoroutine(SlideIn());
    }

    IEnumerator SlideIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Lerp between the initial position and the target position
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / animationDuration);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the phone is exactly at the target position at the end
        transform.position = targetPosition;
    }
}
