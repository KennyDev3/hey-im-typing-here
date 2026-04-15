using UnityEngine;
using UnityEngine.UI;

public class RadialTimerUI : MonoBehaviour
{
    public static RadialTimerUI Instance;
    public Image radialTimerImage;
    private float currentTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeTimer();
    }

    private void Update()
    {
        // If in Story Mode, keep radial full and skip timer logic
        if (GameManager.Instance.IsStoryMode)
        {
            radialTimerImage.fillAmount = 1f;
            return;
        }

        // Normal timer behavior for other modes
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            radialTimerImage.fillAmount = currentTimer / ScoreManager.Instance.multiplierResetTime;
        }
        else
        {
            radialTimerImage.fillAmount = 0;
        }
    }

    public void ResetTimer()
    {
        // In Story Mode, just keep it full
        if (GameManager.Instance.IsStoryMode)
        {
            radialTimerImage.fillAmount = 1f;
            return;
        }

        currentTimer = ScoreManager.Instance.multiplierResetTime;
        radialTimerImage.fillAmount = 1f;
    }

    private void InitializeTimer()
    {
        // Start with full radial in Story Mode
        if (GameManager.Instance.IsStoryMode)
        {
            radialTimerImage.fillAmount = 1f;
        }
        else
        {
            currentTimer = ScoreManager.Instance.multiplierResetTime;
            radialTimerImage.fillAmount = 1f;
        }
    }
}