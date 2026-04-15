using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class ChatInputHandler : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField inputField; // Reference to the input field
    public ChatManager chatManager; // Reference to the ChatManager


    void Start()
    {
        // Subscribe to the onValidateInput event
        inputField.onValidateInput += OnValidateInput;


        Debug.Log("ChatInputHandler: Start called");

        // Automatically focus the input field when the game starts
        FocusInputField();
    }

    void Update()
    {
        // Ensure the input field is always focused
        if (!inputField.isFocused)
        {
            //Debug.Log("Input field lost focus, refocusing");
            FocusInputField();
        }
    }

    public void SendMessageFromInputField()
    {
        Debug.Log("SendMessageFromInputField called");

        // Get the message from the input field
        string message = inputField.text.Trim();

        // If the message is not empty, send it
        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log($"Sending message: {message}");
            chatManager.SendPlayerMessage(message); // Call SendPlayerMessage on ChatManager
            inputField.text = ""; // Clear the input field
        }
        else
        {
            Debug.Log("Message is empty, not sending");
        }

        // Refocus the input field after sending the message
        FocusInputField();
    }

    void FocusInputField()
    {
        //Debug.Log("FocusInputField called");

        // Activate and focus the input field
        inputField.ActivateInputField();
        inputField.Select();
    }

    private char OnValidateInput(string text, int charIndex, char addedChar)
    {
        // Block Enter (Return) and Backspace keys
        if (addedChar == (char)KeyCode.Return || addedChar == (char)KeyCode.Backspace )
        {
            Debug.Log($"Blocked character: {addedChar}");
            inputField.text = "";
            return '\0'; // Return null character to block the input
        }

        // Allow all other characters (including spacebar)
        return addedChar;
    }

    
}