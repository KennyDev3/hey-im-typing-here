using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;

public class TypeAlongTest : MonoBehaviour
{
    public delegate void LetterTypedDelegate(bool isCorrect);
    public static event LetterTypedDelegate OnLetterTyped;

    public delegate void WordCompletedDelegate();
    public static event WordCompletedDelegate OnWordCompleted;

    public delegate void FirstWordCompletedDelegate();
    public event FirstWordCompletedDelegate OnFirstWordCompleted;

    public delegate void SentenceCompletedDelegate(bool isPerfect);
    public static event SentenceCompletedDelegate OnSentenceCompleted;

    private bool isFirstWord = true;

    [Header("References")]
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private PhoneAudioManager phoneAudioManager;
    [SerializeField] private BlinkingManager blinkingManager;

    [Header("Settings")]
    [SerializeField] private Color incompleteColor = Color.gray;
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float errorFlashDuration = 0.3f;

    [Header("Size Animation Settings")]
    [SerializeField] private float sizeIncreaseMultiplier = 1.2f;
    [SerializeField] private float sizeAnimationDuration = 0.5f;

    [SerializeField] private int scrollStartAfterWords = 3;
    [SerializeField] private float charOffset = 10f;


    [Header("Sentence Data")]
    [Tooltip("Assign the ScriptableObject asset containing the list of sentences here.")]
    [SerializeField] private SentenceListSO sentenceDataSource;

    private string currentSentence = "";
    private string currentInput = "";
    private bool sentenceComplete = false;
    private List<string> sentenceWords = new List<string>();
    private List<string> strippedWords = new List<string>();

    private Dictionary<int, Dictionary<int, float>> errorCharTimers = new Dictionary<int, Dictionary<int, float>>();
    private Dictionary<int, Dictionary<int, float>> sizeAnimationTimers = new Dictionary<int, Dictionary<int, float>>();

    private int currentWordIndex = 0;
    private int currentLetterIndex = 0;

    private float horizontalOffset = 0f;
    private Vector2 initialAnchoredPosition;

    public bool ignoreInputChange = false;
    private bool hasErrorsInSentence = false;

    public bool autoSetRandomSentence = true;

    private void Awake()
    {
        if (displayText != null)
        {
            initialAnchoredPosition = displayText.rectTransform.anchoredPosition;
        }
    }

    private void Start()
    {
        playerInputField.onValueChanged.AddListener(OnInputChanged);
        playerInputField.onEndEdit.AddListener(OnInputEnd);
        playerInputField.onValidateInput += PreventEnterKey;

        horizontalOffset = 0f;
        UpdateDisplayTextPosition();

        if (autoSetRandomSentence)
        {
            if (sentenceDataSource == null)
            {
                Debug.LogError("Sentence Data Source (ScriptableObject) is not assigned in the Inspector!", this);
                SetSentence("Error: Sentence data not assigned.");
            }
            else if (sentenceDataSource.sentences == null || sentenceDataSource.sentences.Count == 0)
            {
                Debug.LogWarning("Sentence Data Source is assigned but contains no sentences.", this);
                SetSentence("No sentences available in data source.");
            }
            else
            {
                SetRandomSentence();
            }
        }
    }

    private void Update()
    {
        UpdateDisplayText();

        foreach (var wordIndex in new List<int>(sizeAnimationTimers.Keys))
        {
            foreach (var letterIndex in new List<int>(sizeAnimationTimers[wordIndex].Keys))
            {
                sizeAnimationTimers[wordIndex][letterIndex] -= Time.deltaTime;
                if (sizeAnimationTimers[wordIndex][letterIndex] <= 0)
                {
                    sizeAnimationTimers[wordIndex].Remove(letterIndex);
                }
            }
            if (sizeAnimationTimers[wordIndex].Count == 0)
            {
                sizeAnimationTimers.Remove(wordIndex);
            }
        }

        if (playerInputField.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                playerInputField.caretPosition = playerInputField.text.Length;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            // Simple backspace handling might need adjustment based on desired behavior
            // This version resets to last known correct input
            ignoreInputChange = true; // Prevent loop
            playerInputField.text = currentInput;
            playerInputField.caretPosition = currentInput.Length;
            ignoreInputChange = false; // Allow input again
        }
    }

    public void SetRandomSentence()
    {
        if (sentenceDataSource != null && sentenceDataSource.sentences != null && sentenceDataSource.sentences.Count > 0)
        {
            int randomIndex = Random.Range(0, sentenceDataSource.sentences.Count);
            SetSentence(sentenceDataSource.sentences[randomIndex]);
        }
        else
        {
            Debug.LogWarning("Cannot set random sentence: Sentence Data Source is missing, not assigned, or empty.", this);
            SetSentence("Error: No sentences available.");
        }
    }

    public void SetSpecificSentence(string sentence)
    {
        SetSentence(sentence);
    }

    public void SetSentence(string sentence)
    {
        currentSentence = sentence;
        ProcessSentence();
        currentInput = "";
        sentenceComplete = false;
        currentWordIndex = 0;
        currentLetterIndex = 0;
        horizontalOffset = 0f; // Reset scroll
        errorCharTimers.Clear();
        sizeAnimationTimers.Clear();
        hasErrorsInSentence = false;
        isFirstWord = true;

        ignoreInputChange = true;
        playerInputField.text = "";
        ignoreInputChange = false;

        UpdateDisplayText();
        UpdateDisplayTextPosition(); // Apply reset scroll
    }

    private void ProcessSentence()
    {
        sentenceWords.Clear();
        strippedWords.Clear();
        if (string.IsNullOrEmpty(currentSentence)) return;
        sentenceWords.AddRange(currentSentence.Split(' '));

        foreach (string word in sentenceWords)
        {
            string stripped = Regex.Replace(word, "[^a-zA-Z0-9]", "");
            if (!string.IsNullOrEmpty(stripped))
            {
                strippedWords.Add(stripped);
            }
        }
    }

    private void OnInputChanged(string input)
    {
        if (ignoreInputChange || sentenceComplete)
        {
            return;
        }

        if (input.Length < currentInput.Length)
        {
            // Handle deletion / backspace by resetting
            ignoreInputChange = true;
            playerInputField.text = currentInput;
            playerInputField.caretPosition = currentInput.Length;
            ignoreInputChange = false;
            return;
        }

        if (currentWordIndex >= strippedWords.Count) return;

        string currentWord = strippedWords[currentWordIndex];

        if (input.Length > currentInput.Length)
        {
            char? latestChar = input[input.Length - 1];

            if (latestChar.HasValue)
            {
                if (currentLetterIndex < currentWord.Length)
                {
                    char correctChar = currentWord[currentLetterIndex];

                    if (char.ToLower(latestChar.Value) == char.ToLower(correctChar))
                    {
                        OnLetterTyped?.Invoke(true);

                        if (!sizeAnimationTimers.ContainsKey(currentWordIndex))
                        {
                            sizeAnimationTimers[currentWordIndex] = new Dictionary<int, float>();
                        }
                        sizeAnimationTimers[currentWordIndex][currentLetterIndex] = sizeAnimationDuration;

                        if (phoneAudioManager != null) phoneAudioManager.PlayTypingSound();

                        if (currentWordIndex >= scrollStartAfterWords)
                        {
                            horizontalOffset -= charOffset; // Apply offset change
                            UpdateDisplayTextPosition(); // Update visual position
                        }

                        currentLetterIndex++;
                        currentInput = input; // Update correct state

                        if (currentLetterIndex >= currentWord.Length) // Word complete
                        {
                            OnWordCompleted?.Invoke();
                            if (isFirstWord)
                            {
                                isFirstWord = false;
                                OnFirstWordCompleted?.Invoke();
                            }

                            currentWordIndex++;
                            currentLetterIndex = 0;

                            if (currentWordIndex >= strippedWords.Count) // Sentence complete
                            {
                                sentenceComplete = true;
                                if (phoneAudioManager != null) phoneAudioManager.PlayMessageSentSound();

                                bool isPerfect = !hasErrorsInSentence;
                                OnSentenceCompleted?.Invoke(isPerfect);

                                // Don't clear here, let OnInputEnd handle next sentence
                            }
                            else
                            {
                                // Add space for next word
                                currentInput += " ";
                                ignoreInputChange = true;
                                playerInputField.text = currentInput;
                                playerInputField.caretPosition = currentInput.Length;
                                ignoreInputChange = false;
                            }
                        }
                    }
                    else // Incorrect letter
                    {
                        OnLetterTyped?.Invoke(false);

                        if (!errorCharTimers.ContainsKey(currentWordIndex))
                        {
                            errorCharTimers[currentWordIndex] = new Dictionary<int, float>();
                        }
                        if (currentLetterIndex < currentWord.Length)
                        {
                            errorCharTimers[currentWordIndex][currentLetterIndex] = errorFlashDuration;
                        }

                        hasErrorsInSentence = true;

                        // Reset input field
                        ignoreInputChange = true;
                        playerInputField.text = currentInput;
                        playerInputField.caretPosition = currentInput.Length;
                        ignoreInputChange = false;

                        if (phoneAudioManager != null) phoneAudioManager.PlayIncorrectLetterSound();
                        UpdateDisplayText(); // Update display to show error flash
                        return; // Stop processing this input
                    }
                }
                else
                {
                    // Typed past end of current word before space/completion logic
                    OnLetterTyped?.Invoke(false); // Treat as error
                    hasErrorsInSentence = true;
                    ignoreInputChange = true;
                    playerInputField.text = currentInput;
                    playerInputField.caretPosition = currentInput.Length;
                    ignoreInputChange = false;
                    if (phoneAudioManager != null) phoneAudioManager.PlayIncorrectLetterSound();
                    UpdateDisplayText();
                    return;
                }
            }
        }

        // UpdateDisplayText(); // Can call here or rely on Update loop
    }

    private void OnInputEnd(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (sentenceComplete)
            {
                SetRandomSentence();
            }
        }
    }

    private void UpdateDisplayText()
    {
        if (displayText == null) return;

        if (sentenceComplete)
        {
            displayText.text = "";
            return;
        }

        string displayString = "";

        for (int wordIndex = 0; wordIndex < sentenceWords.Count; wordIndex++)
        {
            string originalWord = sentenceWords[wordIndex];

            if (wordIndex < currentWordIndex)
            {
                displayString += GetColoredWord(originalWord, completeColor);
            }
            else if (wordIndex == currentWordIndex && !sentenceComplete)
            {
                displayString += GetCurrentWordDisplay(originalWord, wordIndex);
            }
            else
            {
                displayString += GetColoredWord(originalWord, incompleteColor);
            }

            if (wordIndex < sentenceWords.Count - 1)
            {
                displayString += " ";
            }
        }

        if (!sentenceComplete && blinkingManager != null)
        {
            blinkingManager.UpdateBlinking(displayText, currentWordIndex, currentLetterIndex);
        }
        else if (sentenceComplete && blinkingManager != null)
        {
            // blinkingManager might need a StopBlinking() method or handle this case
        }

        displayText.text = displayString;
    }

    private void UpdateDisplayTextPosition()
    {
        if (displayText == null) return;
        // No longer sets pivot
        float targetX = initialAnchoredPosition.x + horizontalOffset;
        displayText.rectTransform.anchoredPosition = new Vector2(targetX, initialAnchoredPosition.y);
    }

    private string GetColoredWord(string word, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{word}</color>";
    }

    private string GetCurrentWordDisplay(string word, int wordIndex)
    {
        string wordDisplay = "";
        for (int letterIndex = 0; letterIndex < word.Length; letterIndex++)
        {
            Color letterColor = GetLetterColor(wordIndex, letterIndex);
            float letterSize = GetLetterSize(wordIndex, letterIndex);
            wordDisplay += $"<color=#{ColorUtility.ToHtmlStringRGB(letterColor)}><size={letterSize}%>{word[letterIndex]}</size></color>";
        }
        return wordDisplay;
    }

    private Color GetLetterColor(int wordIndex, int letterIndex)
    {
        if (letterIndex < currentLetterIndex)
        {
            return completeColor;
        }
        else if (errorCharTimers.ContainsKey(wordIndex) && errorCharTimers[wordIndex].ContainsKey(letterIndex) && letterIndex == currentLetterIndex)
        {
            float timer = errorCharTimers[wordIndex][letterIndex];
            float blinkFrequency = 1.5f;
            bool isBlinkingRed = Mathf.PingPong(Time.time * 10f, blinkFrequency) > (blinkFrequency / 2); // Faster blink
            return isBlinkingRed ? errorColor : incompleteColor;
        }
        else
        {
            return incompleteColor;
        }
    }

    private float GetLetterSize(int wordIndex, int letterIndex)
    {
        if (sizeAnimationTimers.ContainsKey(wordIndex) && sizeAnimationTimers[wordIndex].ContainsKey(letterIndex))
        {
            float timer = sizeAnimationTimers[wordIndex][letterIndex];
            float normalizedTime = 1 - (timer / sizeAnimationDuration);

            float easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            float peakSize = sizeIncreaseMultiplier * 100f;
            // Smoothly lerp from peak size down to 100%
            return Mathf.Lerp(peakSize, 100f, easedTime);
        }
        return 100f;
    }

    private char PreventEnterKey(string text, int charIndex, char addedChar)
    {
        if (addedChar == '\n' || addedChar == '\r')
        {
            return '\0';
        }
        return addedChar;
    }

    public bool IsSentenceComplete() => sentenceComplete;

    public string GetCompletedSentence() => currentSentence;

    public void ClearInputAndDisplay()
    {
        ignoreInputChange = true;
        playerInputField.text = "";
        displayText.text = "";
        ignoreInputChange = false;
        horizontalOffset = 0f; // Reset scroll on manual clear
        UpdateDisplayTextPosition(); // Apply reset position
    }
}