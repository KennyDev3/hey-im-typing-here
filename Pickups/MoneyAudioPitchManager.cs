using UnityEngine;

public class MoneyAudioPitchManager : MonoBehaviour
{
    public static MoneyAudioPitchManager Instance { get; private set; }

    [SerializeField] private AudioSource coinAudioSource;
    private float lastPickupTime = -1f;
    private const float pitchResetTime = 1f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayCoinSound()
    {
        coinAudioSource.pitch += 0.05f;
        coinAudioSource.Play();
        lastPickupTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastPickupTime > pitchResetTime)
        {
            coinAudioSource.pitch = 1f;
        }
    }
}
