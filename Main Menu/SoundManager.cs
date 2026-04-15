using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] carHitSounds; // Array of car hit sound clips
    [SerializeField] private float minPitch = 0.9f; // Minimum pitch variation
    [SerializeField] private float maxPitch = 1.1f; // Maximum pitch variation
    [SerializeField] private float minVolume = 0.8f; // Minimum volume variation
    [SerializeField] private float maxVolume = 1f; // Maximum volume variation
    [SerializeField] private AudioMixerGroup mainMenuFXGroup; // Reference to the MainMenuFX Audio Mixer group
    private int maxSimultaneousSounds = 5; // Max number of sounds playing at once


    // Audio filtering parameters
    [Header("Audio EQ Settings")]
    [SerializeField] private float lowFrequency = 200f; // Low frequency cutoff
    [SerializeField][Range(0.0f, 1.0f)] private float lowFrequencyGain = 0.7f; // Reduce low frequencies by 30%
    [SerializeField][Range(0.1f, 5.0f)] private float lowFrequencyQ = 1.0f; // Q factor for low frequency

    [Header("Advanced Sound Variety")]
    [SerializeField][Range(0.0f, 0.3f)] private float stereoSpreadAmount = 0.1f; // Stereo spread variation
    [SerializeField][Range(0.0f, 0.5f)] private float distanceVariation = 0.2f; // Spatial blend variation
    [SerializeField][Range(0.0f, 0.2f)] private float reverbMixVariation = 0.1f; // Reverb mix variation

    private List<AudioSource> audioSources; // Pool of AudioSources
    private List<AudioLowPassFilter> lowPassFilters; // Corresponding low-pass filters
    private List<AudioHighPassFilter> highPassFilters; // Corresponding high-pass filters

    // Nested static class for collision events
    public static class CollisionEventSystem
    {
        public static event Action OnCollisionOccurred;

        public static void TriggerCollision()
        {
            OnCollisionOccurred?.Invoke();
        }
    }

    private void Awake()
    {
        // Initialize the pools
        audioSources = new List<AudioSource>();
        lowPassFilters = new List<AudioLowPassFilter>();
        highPassFilters = new List<AudioHighPassFilter>();

        // Initialize the pool of AudioSources and filters
        for (int i = 0; i < maxSimultaneousSounds; i++)
        {
            // Create a child GameObject for each audio source
            GameObject audioSourceObj = new GameObject("AudioSource_" + i);
            audioSourceObj.transform.parent = this.transform;

            // Add AudioSource to the child GameObject
            AudioSource source = audioSourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mainMenuFXGroup; // Assign the AudioSource to the MainMenuFX group
            audioSources.Add(source);

            // Add filters to the same child GameObject
            AudioLowPassFilter lowPassFilter = audioSourceObj.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = 10000f; // Default high cutoff to not affect sound initially
            lowPassFilter.lowpassResonanceQ = 1.0f;
            lowPassFilters.Add(lowPassFilter);

            AudioHighPassFilter highPassFilter = audioSourceObj.AddComponent<AudioHighPassFilter>();
            highPassFilter.cutoffFrequency = 10f; // Default low cutoff to not affect sound initially
            highPassFilter.highpassResonanceQ = 1.0f;
            highPassFilters.Add(highPassFilter);
        }

        // Subscribe to the collision event
        CollisionEventSystem.OnCollisionOccurred += PlayCarHitSound;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the collision event
        CollisionEventSystem.OnCollisionOccurred -= PlayCarHitSound;
    }

    private void PlayCarHitSound()
    {
        // Find an available AudioSource
        int sourceIndex = GetAvailableAudioSourceIndex();
        if (sourceIndex == -1) return; // No available sources

        AudioSource availableSource = audioSources[sourceIndex];
        AudioLowPassFilter lowPassFilter = lowPassFilters[sourceIndex];
        AudioHighPassFilter highPassFilter = highPassFilters[sourceIndex];

        // Apply EQ to reduce harsh low frequencies
        highPassFilter.cutoffFrequency = lowFrequency;
        highPassFilter.highpassResonanceQ = lowFrequencyQ;

        // Randomize filter cutoff frequencies for variety
        lowPassFilter.cutoffFrequency = Random.Range(7000f, 15000f);

        // Randomize pitch and volume
        availableSource.pitch = Random.Range(minPitch, maxPitch);
        availableSource.volume = Random.Range(minVolume, maxVolume);

        // Add more variety with stereo spread
        availableSource.panStereo = Random.Range(-stereoSpreadAmount, stereoSpreadAmount);

        // Add slight spatial blend variation for more realistic sound
        availableSource.spatialBlend = Random.Range(0f, distanceVariation);

        // Vary the reverb send level if using reverb
        availableSource.reverbZoneMix = Random.Range(1f - reverbMixVariation, 1f);

        // Play a random car hit sound
        AudioClip clip = carHitSounds[Random.Range(0, carHitSounds.Length)];
        availableSource.PlayOneShot(clip);
    }

    private int GetAvailableAudioSourceIndex()
    {
        // Find the first AudioSource that is not playing
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                return i;
            }
        }
        return -1; // All sources are busy
    }

    // Optional: Add this method to snapshot EQ settings in editor for testing
#if UNITY_EDITOR
    [ContextMenu("Apply Current EQ Settings")]
    private void ApplyEQSettings()
    {
        for (int i = 0; i < highPassFilters.Count; i++)
        {
            highPassFilters[i].cutoffFrequency = lowFrequency;
            highPassFilters[i].highpassResonanceQ = lowFrequencyQ;
        }
        Debug.Log("Applied EQ settings to all audio sources");
    }
#endif
}