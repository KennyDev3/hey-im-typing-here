using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;



    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _accuracyText;
    [SerializeField] TMP_Text _phoneScoreText;
    [SerializeField] TMP_Text _multiplierScoreText;
    [SerializeField] TMP_Text _wpmText; 

    [SerializeField] GameObject FloatingPhoneScoreTextPrefab;
    [SerializeField] private Transform FloatingTextPhonePrefabTransform;

    [SerializeField] private MultiplierTextAnimator multiplierTextAnimator; // Reference to the multiplier text animator
    [SerializeField] private RadialImageAnimator radialImageAnimator; // Reference to the radial image animator



    [SerializeField] private Animator _scoreAnimator;
    [SerializeField] GameManager gameManager;

    [Header("Garbage Managers Reference")] // To throw Text Prefabs under Managers
    [SerializeField] GameObject Managers;

    [Header("Debug")]
    public int debugScoreToAdd = 0;


    


    // Phone Score Settings

    private long phoneTotalScore = 0;
    private float phoneScoreMultiplier = 1f;
    private float multiplierTimer = 0f;

    //Words per Minute settings

    private float elapsedTime = 0f;
    private int wordsCompleted = 0;


    [Header("Score Settings")]
    public const float DEFAULT_MULTIPLIER_RESET_TIME = 3f; // Constant for default value
    public float multiplierResetTime = DEFAULT_MULTIPLIER_RESET_TIME; // Modifiable runtime value
    [SerializeField] float completedWordScore = 100f;
    [SerializeField] float perfectSentenceScore = 500f;
    [SerializeField] float imperfectSentenceScore = 250f;

    private int correctWords = 0;
    private int mistakes = 0;
    private int messagesSent = 0;
    private float highestMultiplier = 1f; // Start at 1 since multipliers don't go below this

    public int GetMistakes() => mistakes;
    public int GetMessagesSent() => messagesSent;
    public float GetHighestMultiplier() => highestMultiplier;


    private int totalLettersTyped = 0;
    private int correctLettersTyped = 0;




    public delegate void ScoreUpdatedDelegate(long score, float multiplier);
    public static event ScoreUpdatedDelegate OnScoreUpdated;

    //Debugging
    private float score = 0;
    public bool addScoreOnToggle = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        phoneScoreMultiplier = 1f;
        UpdateScoreUI();
        UpdateAccuracyDisplay();


        // Find the GameManager in the scene and inject this ScoreManager into it

        if (gameManager != null)
        {
            gameManager.Init(this); // Inject this ScoreManager into the GameManager
        }
        else
        {
            Debug.LogError("GameManager not found in the scene!");
        }


    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Handle multiplier timer
        if (multiplierTimer > 0)
        {
            multiplierTimer -= Time.deltaTime;
            if (multiplierTimer <= 0)
            {
                ResetMultiplier();
            }
        }

        //Debugging

        if (addScoreOnToggle)
        {
            phoneTotalScore += debugScoreToAdd;
            UpdateScoreDisplay();
            addScoreOnToggle = false; // Reset the toggle
        }


    }


    //Subscribing to Events from TypeAlongTest to update Score Logic and PhoneScoreMultiplier

    private void OnEnable()
    {
        TypeAlongTest.OnLetterTyped += HandleLetterTyped;
        TypeAlongTest.OnWordCompleted += HandleWordCompleted;
        TypeAlongTest.OnSentenceCompleted += HandleSentenceCompleted;
    }

    private void OnDisable()
    {
        TypeAlongTest.OnLetterTyped -= HandleLetterTyped;
        TypeAlongTest.OnWordCompleted -= HandleWordCompleted;
        TypeAlongTest.OnSentenceCompleted -= HandleSentenceCompleted;
    }

    private void HandleLetterTyped(bool isCorrect)
    {
        if (isCorrect)
        {
            OnCorrectLetter();
        }
        else
        {
            OnIncorrectLetter();
        }

        UpdateAccuracyDisplay();
        UpdateScoreDisplay();

    }

    private void HandleWordCompleted()
    {
        wordsCompleted++; // Increment word count
        OnWordCompleted();
        UpdateWPMDisplay(); // Update WPM display when a word is completed

    }

    private void HandleSentenceCompleted(bool isPerfect)
    {
        OnSentenceCompleted(isPerfect);
    }






    public void OnCorrectLetter()
    {
        


        // Calculate the score for the correct letter
        float letterScore = 10 * phoneScoreMultiplier;
        int roundedLetterScore = Mathf.FloorToInt(letterScore); // Round to nearest integer
        phoneTotalScore += roundedLetterScore;
        correctLettersTyped++;
        totalLettersTyped++;

        // Update multiplier
        phoneScoreMultiplier += 0.1f;
        multiplierTimer = multiplierResetTime; // Reset the timer

        if (RadialTimerUI.Instance != null)
        {
            RadialTimerUI.Instance.ResetTimer();
        }

        if (multiplierTextAnimator != null)
        {
            multiplierTextAnimator.OnCorrectLetter((int)phoneScoreMultiplier);
        }

        if (radialImageAnimator != null)
        {
            radialImageAnimator.OnMultiplierIncreased((int)phoneScoreMultiplier);
        }

        if (phoneScoreMultiplier > highestMultiplier)
        {
            highestMultiplier = phoneScoreMultiplier;
        }


        // Show floating text for the letter score
        ShowFloatingText(roundedLetterScore);

        // Trigger score update event
        OnScoreUpdated?.Invoke(phoneTotalScore, phoneScoreMultiplier);

        
        

        //Debug.Log($"Correct letter! Score: {phoneTotalScore}, Multiplier: {phoneScoreMultiplier.ToString("F1")}");
    }

    public void OnIncorrectLetter()
    {
        mistakes++; // Track mistakes
        totalLettersTyped++;   // Update accuracy

        phoneScoreMultiplier = Mathf.Max(1f, phoneScoreMultiplier - 0.1f);    // Update multiplier
        multiplierTimer = multiplierResetTime; // Reset the timer

        // Trigger score update event
        OnScoreUpdated?.Invoke(phoneTotalScore, phoneScoreMultiplier);

        if (multiplierTextAnimator != null)
        {
            multiplierTextAnimator.ResetMultiplier();
        }

        //Debug.Log($"Incorrect letter! Score: {phoneTotalScore}, Multiplier: {phoneScoreMultiplier.ToString("F1")}");

    }

    // Called when a word is completed
    public void OnWordCompleted()
    {
        // Calculate the score for the completed word
        float wordScore = completedWordScore * phoneScoreMultiplier;
        int roundedWordScore = Mathf.FloorToInt(wordScore); // Round to nearest integer
        phoneTotalScore += roundedWordScore;

        // Show floating text for the word score
        ShowFloatingText(roundedWordScore, "+", isWord: true);
        correctWords++;


        // Trigger score update event
        OnScoreUpdated?.Invoke(phoneTotalScore, phoneScoreMultiplier);


        Debug.Log($"Word completed! Score: {roundedWordScore}, Multiplier: {phoneScoreMultiplier.ToString("F1")}");
    }

    public void OnSentenceCompleted(bool isPerfect)
    {
        messagesSent++;
        // Calculate the score for the completed sentence
        float sentenceScore = isPerfect ? perfectSentenceScore * phoneScoreMultiplier : imperfectSentenceScore * phoneScoreMultiplier;
        int roundedSentenceScore = Mathf.FloorToInt(sentenceScore); // Round to nearest integer
        phoneTotalScore += roundedSentenceScore;

        // Update multiplier for perfect sentences
        if (isPerfect)
        {
            phoneScoreMultiplier += 1f; // Bonus for perfect sentence
        }

        // Show floating text for the sentence score
        ShowFloatingText(roundedSentenceScore, "+", isSentence: true);

        // Reset multiplier timer
        multiplierTimer = multiplierResetTime;

        // Trigger score update event
        OnScoreUpdated?.Invoke(phoneTotalScore, phoneScoreMultiplier);

        Debug.Log($"Sentence completed! Perfect: {isPerfect}, Score: {roundedSentenceScore}, Multiplier: {phoneScoreMultiplier.ToString("F1")}");
    }

    // Reset the multiplier to 1
    public void ResetMultiplier()
    {
        phoneScoreMultiplier = 1f;
        OnScoreUpdated?.Invoke(phoneTotalScore, phoneScoreMultiplier);

        if (multiplierTextAnimator != null)
        {
            multiplierTextAnimator.ResetMultiplier();
            phoneScoreMultiplier = 1f;
            UpdateScoreDisplay();
        }

        if (radialImageAnimator != null)
        {
            radialImageAnimator.ResetRadialImage();
        }


    }

    public void SetMultiplierResetTime(float time)
    {
        multiplierResetTime = time;
        // Optionally reset the current timer when changing the reset time
    }

    // Calculate accuracy as a percentage
    public float GetAccuracy()
    {
        if (totalLettersTyped == 0) return 100f;
        return (float)correctLettersTyped / totalLettersTyped * 100f;
    }

    public float CalculateWPM()
    {
        if (elapsedTime <= 0) return 0f;
        return (wordsCompleted / elapsedTime) * 60f; // WPM = (words / time in minutes)
    }

    private void UpdateWPMDisplay()
    {
        if (_wpmText != null)
        {
            float wpm = CalculateWPM();
            _wpmText.text = $"WPM: {wpm.ToString("F0")}"; // Format to one decimal place
        }
    }

    public void ResetWPM()
    {
        elapsedTime = 0f;
        wordsCompleted = 0;
        UpdateWPMDisplay();
    }





    // Update the accuracy display
    private void UpdateAccuracyDisplay()
    {
        if (_accuracyText != null)
        {
            float accuracy = GetAccuracy();
            _accuracyText.text = $"Acc: {accuracy.ToString("F1")}%"; // Format to one decimal place
        }
    }

    void UpdateScoreDisplay()
    {
        if (_phoneScoreText != null)
        {
            _phoneScoreText.text = phoneTotalScore.ToString("N0");
        }

        if (_multiplierScoreText != null)
        {
            string multiplier = GetMultiplierAsString();
            _multiplierScoreText.text = $"X{multiplier}";
        }

    }

    private void AdjustFontSize(TMP_Text textComponent)
    {
        if (textComponent == null) return;

        float preferredWidth = textComponent.preferredWidth;
        RectTransform textRectTransform = textComponent.rectTransform;
        float containerWidth = textRectTransform.rect.width;

        if (preferredWidth > containerWidth)
        {
            textComponent.fontSize = Mathf.Max(1, textComponent.fontSize * (containerWidth / preferredWidth));
        }
        else if (preferredWidth < containerWidth)
        {
            textComponent.fontSize = Mathf.Min(textComponent.fontSize * 1.1f, 100);
        }
    }

    public void AddDebugScore()
    {
        phoneTotalScore += debugScoreToAdd;
        UpdateScoreDisplay();
    }

    public long GetTotalScore()
    {
        return phoneTotalScore;
    }

    // Get current multiplier
    public float GetMultiplier()
    {
        return phoneScoreMultiplier;
    }

    public string GetMultiplierAsString()
    {
        // Round to one decimal place and handle floating-point precision errors
        float roundedMultiplier = Mathf.Round(phoneScoreMultiplier * 10 + 0.0001f) / 10;
        return roundedMultiplier.ToString("F1"); // Format to one decimal place
    }

  

    public int GetCorrectWords()
    {
        return correctWords;
    }

    public void IncreaseMultiplierBy10()
    {
        phoneScoreMultiplier += 10f; // Increase the multiplier by 10
        multiplierTimer = multiplierResetTime; // Reset the multiplier timer

        // Update the multiplier display
        UpdateScoreDisplay();

        // Trigger any animations or effects related to the multiplier increase
        if (multiplierTextAnimator != null)
        {
            multiplierTextAnimator.OnCorrectLetter((int)phoneScoreMultiplier);
        }

        if (radialImageAnimator != null)
        {
            radialImageAnimator.OnMultiplierIncreased((int)phoneScoreMultiplier);
        }

        // Trigger score update event
        OnScoreUpdated?.Invoke(phoneTotalScore, phoneScoreMultiplier);

        Debug.Log($"Multiplier increased by 10! New Multiplier: {phoneScoreMultiplier.ToString("F1")}");
    }


    public void IncreaseMoneyScore(float moneyIncreaseAmount)
    {
        if (gameManager.GameOver) return;

        score += moneyIncreaseAmount;
        UpdateScoreUI();

        if (_scoreAnimator != null)
        {
            _scoreAnimator.SetTrigger("AddScore"); // Trigger the "AddScore" animation
        }
    }

    

    private void UpdateScoreUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $" {score}$";
        }
    }

    public float GetFinalMoney()
    {
        return score;
    }

    private void ShowFloatingText(int scoreAmount, string prefix = "+", bool isWord = false, bool isSentence = false)
    {
        // Call the static method on PhoneScoreFloatingText
        PhoneScoreFloatingText.ShowFloatingText(FloatingPhoneScoreTextPrefab, FloatingTextPhonePrefabTransform, scoreAmount, prefix, isWord, isSentence, Managers.transform);
    }


}