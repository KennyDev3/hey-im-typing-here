using UnityEngine;
using System.Collections;

public class PhoneAudioManager : MonoBehaviour
{
    [Header("Phone Reference")]
    [SerializeField] private Transform phoneTransform; // Reference to the PhoneDAD GameObject

    [Header("Audio Sources (Assign in Inspector or will be auto-added)")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioSource phoneEffectsAudioSource;

    [Header("Typing Sounds")]
    [SerializeField] private AudioClip[] typingSounds;

    [Header("Other Sounds")]
    [SerializeField] private AudioClip incorrectLetterSound;
    [SerializeField] private AudioClip messageSentSound;
    [SerializeField] private AudioClip messageReceivedSound;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f; // Duration of the shake
    [SerializeField] private float shakeIntensity = 0.1f; // How far the phone moves
    [SerializeField] private float shakeSpeed = 50f; // Speed of the shake

    private Vector3 originalPosition; // Store the original position of the phone

    private void Awake()
    {
        AudioSource[] existingSources = GetComponents<AudioSource>();

        if (typingAudioSource == null)
        {
            if (existingSources.Length > 0)
            {
                typingAudioSource = existingSources[0];
            }
            else
            {
                typingAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (phoneEffectsAudioSource == null)
        {
            bool foundDifferentSource = false;
            if (existingSources.Length > 0)
            {
                foreach (AudioSource source in existingSources)
                {
                    if (source != typingAudioSource)
                    {
                        phoneEffectsAudioSource = source;
                        foundDifferentSource = true;
                        break;
                    }
                }
            }

            if (!foundDifferentSource)
            {
                phoneEffectsAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (typingAudioSource != null && typingAudioSource == phoneEffectsAudioSource)
        {
            phoneEffectsAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Store the original position of the phone
        if (phoneTransform != null)
        {
            originalPosition = phoneTransform.localPosition;
        }
    }

    public void PlayTypingSound()
    {
        if (typingAudioSource != null && typingSounds != null && typingSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, typingSounds.Length);
            typingAudioSource.PlayOneShot(typingSounds[randomIndex]);
        }
    }

    public void PlayIncorrectLetterSound()
    {
        if (phoneEffectsAudioSource != null && incorrectLetterSound != null)
        {
            phoneEffectsAudioSource.PlayOneShot(incorrectLetterSound);
            StartCoroutine(Shake()); // Start the shake effect
        }
    }

    public void PlayMessageSentSound()
    {
        if (phoneEffectsAudioSource != null && messageSentSound != null)
        {
            phoneEffectsAudioSource.PlayOneShot(messageSentSound);
        }
    }

    public void PlayMessageReceived()
    {
        if (phoneEffectsAudioSource != null && messageReceivedSound != null)
        {
            phoneEffectsAudioSource.PlayOneShot(messageReceivedSound);
        }
    }

    private IEnumerator Shake()
    {
        if (phoneTransform == null) yield break; // Exit if no phone transform is set

        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // Calculate a random offset for the shake
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);

            // Apply the offset to the phone's position
            phoneTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            // Increase the elapsed time based on the shake speed
            elapsedTime += Time.deltaTime * shakeSpeed;

            yield return null; // Wait for the next frame
        }

        // Reset the phone to its original position
        phoneTransform.localPosition = originalPosition;
    }
}