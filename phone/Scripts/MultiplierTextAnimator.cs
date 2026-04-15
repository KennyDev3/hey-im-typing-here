using UnityEngine;
using TMPro;

public class MultiplierTextAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text multiplierText;
     private float flashDuration = 0.2f; // Duration of the flash animation
     private float scaleIncrement = 0.02f; // How much to increase the scale per correct letter
     private float maxRotationAngle = 10f; // Maximum rotation angle (left or right)

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float currentScaleIncrease = 0f;

    private Color originalColor; // Original color of the text (yellow)
    private Color targetColor = Color.red; // Target color (red)

    private void Awake()
    {
        // Store the original scale, rotation, and color of the text
        originalScale = multiplierText.transform.localScale;
        originalRotation = multiplierText.transform.rotation;
        originalColor = multiplierText.color; // Default to yellow (set in the Inspector)
    }

    // Called when a correct letter is typed
    public void OnCorrectLetter(int multiplier)
    {
        // Increase the scale increment
        currentScaleIncrease += scaleIncrement;

        // Calculate the target scale
        Vector3 targetScale = originalScale + new Vector3(currentScaleIncrease, currentScaleIncrease, 0);

        // Flash animation: Scale up, rotate, then scale down and reset rotation
        LeanTween.scale(multiplierText.gameObject, targetScale, flashDuration / 2)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Scale back to original size
                LeanTween.scale(multiplierText.gameObject, originalScale, flashDuration / 2)
                    .setEase(LeanTweenType.easeInQuad);

                // Rotate back to original rotation
                LeanTween.rotate(multiplierText.gameObject, originalRotation.eulerAngles, flashDuration / 2)
                    .setEase(LeanTweenType.easeInOutQuad);
            });

        // Apply the random rotation during the animation
        float randomRotation = Random.Range(-maxRotationAngle, maxRotationAngle);
        LeanTween.rotateZ(multiplierText.gameObject, randomRotation, flashDuration / 2)
            .setEase(LeanTweenType.easeInOutQuad);

        // Update the text color based on the multiplier
        UpdateTextColor(multiplier);
    }

    // Called when an incorrect letter is typed or the multiplier resets
    public void ResetMultiplier()
    {
        // Reset the scale increase
        currentScaleIncrease = 0f;

        // Reset the scale and rotation
        LeanTween.cancel(multiplierText.gameObject); // Cancel any ongoing animations
        multiplierText.transform.localScale = originalScale;
        multiplierText.transform.rotation = originalRotation;

        // Reset the text color to yellow
        multiplierText.color = originalColor;
    }

    // Update the text color based on the multiplier
    private void UpdateTextColor(int multiplier)
    {
        // Clamp the multiplier between 0 and 100
        multiplier = Mathf.Clamp(multiplier, 0, 100);

        // Calculate the interpolation factor (0 = yellow, 1 = red)
        float t = multiplier / 100f;

        // Interpolate the color
        multiplierText.color = Color.Lerp(originalColor, targetColor, t);
    }
}