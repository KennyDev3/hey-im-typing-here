using UnityEngine;
using UnityEngine.UI;

public class RadialImageAnimator : MonoBehaviour
{
    [SerializeField] private Image radialImage; // Reference to the radial image
    [SerializeField] private float growthIncrement = 0.1f; // How much to increase the size every 10 multipliers
    [SerializeField] private float popDuration = 0.2f; // Duration of the "POP" animation
    [SerializeField] private float maxScale = 1.5f; // Maximum scale of the radial image

    private Vector3 originalScale;
    private int currentMultiplier = 0;

    private void Awake()
    {
        // Store the original scale of the radial image
        originalScale = radialImage.transform.localScale;
    }

    // Called when the multiplier increases
    public void OnMultiplierIncreased(int multiplier)
    {
        // Check if the multiplier has reached a multiple of 10
        if (multiplier % 10 == 0 && multiplier > currentMultiplier)
        {
            currentMultiplier = multiplier;

            // Calculate the target scale
            float growthFactor = (multiplier / 10) * growthIncrement;
            Vector3 targetScale = originalScale + new Vector3(growthFactor, growthFactor, 0);

            // Clamp the target scale to the maximum scale
            targetScale = Vector3.Min(targetScale, originalScale * maxScale);

            // Apply the "POP" animation
            LeanTween.scale(radialImage.gameObject, targetScale, popDuration)
                .setEase(LeanTweenType.easeOutBack) // Adds a "POP" feeling
                .setOnComplete(() =>
                {
                    // Optionally, add a slight bounce-back effect
                    LeanTween.scale(radialImage.gameObject, targetScale * 0.95f, popDuration / 2)
                        .setEase(LeanTweenType.easeInOutQuad);
                });
        }
    }

    // Called when the multiplier resets
    public void ResetRadialImage()
    {
        // Reset the scale to the original size
        LeanTween.cancel(radialImage.gameObject); // Cancel any ongoing animations
        radialImage.transform.localScale = originalScale;
        currentMultiplier = 0;
    }
}