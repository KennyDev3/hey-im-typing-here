using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    public static PlayerAudioManager Instance { get; private set; }

    public AudioClip[] hazardSounds;
    public AudioClip[] obstacleSounds;
    public AudioClip jumpSound;
    public AudioClip changeLaneSound;
    public AudioClip footstepsSound;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on AudioManager.");
        }
    }

    public void PlayRandomHazardSound()
    {
        PlayRandomSound(hazardSounds);
    }

    public void PlayRandomObstacleSound()
    {
        PlayRandomSound(obstacleSounds);
    }

    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(jumpSound);
    }

    public void PlayChangeLaneSound()
    {
        audioSource.PlayOneShot(changeLaneSound);
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, clips.Length);
            audioSource.PlayOneShot(clips[randomIndex]);
        }
    }
}
