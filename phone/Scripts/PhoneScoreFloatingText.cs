using System.Threading;
using TMPro;
using UnityEngine;

public class PhoneScoreFloatingText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 1f; // Speed at which the text moves upward
    [SerializeField] private Vector3 positionOffsetRange = new Vector3(0.1f, 0.1f, 0f); // Random position offset
    [SerializeField] private Vector3 rotationOffsetRange = new Vector3(0f, 0f, 10f); // Random rotation offset
    [SerializeField] private float lifetimeDuration = 0.8f; // Duration before destruction

    [Header("Animation Settings")]
    [SerializeField] private float popInDuration = 0.1f; // Duration of the initial pop-in animation
    [SerializeField] private float popInScale = 1.3f; // Maximum scale for pop-in effect
    [SerializeField] private float fadeOutDuration = 0.3f; // Duration of fade out animation
    [SerializeField] private float wobbleAmount = 0.2f; // Amount of wobble effect
    [SerializeField] private float wobbleSpeed = 10f; // Speed of wobble effect

    [Header("Colors")]
    [SerializeField] private Color completedWordColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color completedSentenceColor = new Color(0.5f, 0f, 1f); // Purple

    private TextMeshPro textMesh;
    private Color defaultColor; // Store the prefab's default color
    private Vector3 originalScale;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            defaultColor = textMesh.color; // Store the prefab's default color
        }
        originalScale = transform.localScale;
        ApplyRandomOffset();
        RunAnimations();
    }

    private void ApplyRandomOffset()
    {
        // Randomize position
        Vector3 positionOffset = new Vector3(
            Random.Range(-positionOffsetRange.x, positionOffsetRange.x),
            Random.Range(-positionOffsetRange.y, positionOffsetRange.y),
            Random.Range(-positionOffsetRange.z, positionOffsetRange.z)
        );
        transform.position += positionOffset;

        // Randomize rotation
        Vector3 rotationOffset = new Vector3(
            Random.Range(-rotationOffsetRange.x, rotationOffsetRange.x),
            Random.Range(-rotationOffsetRange.y, rotationOffsetRange.y),
            Random.Range(-rotationOffsetRange.z, rotationOffsetRange.z)
        );
        transform.rotation = Quaternion.Euler(rotationOffset);
    }

    private void RunAnimations()
    {
        // Start with zero scale
        transform.localScale = Vector3.zero;

        // Initial pop-in animation
        LeanTween.scale(gameObject, originalScale * popInScale, popInDuration)
            .setEaseOutBack() // Gives a bouncy pop effect
            .setOnComplete(() => {
                // Slightly bounce back to normal size
                LeanTween.scale(gameObject, originalScale, popInDuration * 0.8f)
                    .setEaseInOutSine();

                // Add a subtle wobble effect
                LeanTween.rotateZ(gameObject, transform.rotation.eulerAngles.z + wobbleAmount, wobbleSpeed * 0.1f)
                    .setEaseShake()
                    .setLoopPingPong(3);
            });

        // Rising movement with easing
        LeanTween.moveY(gameObject, transform.position.y + (moveSpeed * lifetimeDuration * 1.2f), lifetimeDuration)
            .setEaseOutCubic();

        // Fade out animation
        LeanTween.value(gameObject, 1f, 0f, fadeOutDuration)
            .setDelay(lifetimeDuration - fadeOutDuration)
            .setOnUpdate((float val) => {
                Color color = textMesh.color;
                color.a = val;
                textMesh.color = color;
            })
            .setOnComplete(() => {
                Destroy(gameObject);
            });
    }

    // Set the text of the floating text
    public void SetText(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color; // Apply the color
        }
        else
        {
            Debug.LogError("TextMeshPro component not found!");
        }
    }

    // Static method to instantiate and show floating text
    public static void ShowFloatingText(GameObject prefab, Transform spawnPoint, int scoreAmount, string prefix = "+", bool isWord = false, bool isSentence = false, Transform parentTransform = null)
    {
        if (prefab == null || spawnPoint == null)
        {
            Debug.LogError("Floating text prefab or spawn point is not assigned!");
            return;
        }

        // Instantiate the floating text prefab at the spawn point
        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity, parentTransform);

        // Get the PhoneScoreFloatingText component
        PhoneScoreFloatingText floatingText = go.GetComponent<PhoneScoreFloatingText>();
        if (floatingText != null)
        {
            // Determine the color based on the context
            Color textColor = floatingText.defaultColor; // Default to prefab's color
            if (isWord)
            {
                textColor = floatingText.completedWordColor; // Use word color
            }
            else if (isSentence)
            {
                textColor = floatingText.completedSentenceColor; // Use sentence color
            }

            // Set the text and color of the floating text
            floatingText.SetText($"{prefix}{scoreAmount}", textColor);
        }
        else
        {
            Debug.LogError("PhoneScoreFloatingText component not found on prefab!");
        }
    }
}