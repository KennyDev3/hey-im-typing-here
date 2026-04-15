using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BlinkingManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float blinkDuration = 0.3f; // How long the blink lasts (for 3 blinks)
    private Dictionary<int, float> errorCharTimers = new Dictionary<int, float>();

    // Update the blinking effect on the character at a specific index
    public void UpdateBlinking(TMP_Text text, int wordIndex, int letterIndex)
    {
        if (errorCharTimers.ContainsKey(letterIndex))
        {
            errorCharTimers[letterIndex] -= Time.deltaTime;

            if (errorCharTimers[letterIndex] <= 0)
            {
                errorCharTimers.Remove(letterIndex);
                // Reset the text color to the normal one after blinking is done
                SetCharacterColor(text, wordIndex, letterIndex, Color.white);
            }
            else
            {
                // Alternate blinking effect
                bool isBlinking = Mathf.PingPong(Time.time * 2f, 1f) < 0.5f; // Blink effect
                SetCharacterColor(text, wordIndex, letterIndex, isBlinking ? Color.red : Color.white);
            }
        }
    }

    // Start blinking on a specific character (wrong letter)
    public void StartBlinking(int letterIndex)
    {
        if (!errorCharTimers.ContainsKey(letterIndex))
        {
            errorCharTimers[letterIndex] = blinkDuration;
        }
    }

    // Helper to set the color of a character
    private void SetCharacterColor(TMP_Text text, int wordIndex, int letterIndex, Color color)
    {
        // Assuming each word is a separate tag, we get the character at letterIndex and set its color
        string[] words = text.text.Split(' '); // Split text by spaces
        if (wordIndex < words.Length)
        {
            string word = words[wordIndex];
            text.text = text.text.Substring(0, text.text.IndexOf(word)) +
                        word.Substring(0, letterIndex) +
                        $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{word[letterIndex]}</color>" +
                        word.Substring(letterIndex + 1);
        }
    }
}