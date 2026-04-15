using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ChatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerBubblePrefab;
    [SerializeField] private GameObject friendBubblePrefab;
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TypeAlongTest typeAlongTest;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PhoneAudioManager phoneAudioManager;

    [Header("Layout Settings")]
    [SerializeField] private float bubbleSpacing = 10f;
    [SerializeField] private float bottomPadding = 20f;
    [SerializeField] private float leftRightPadding = 20f;

    [Header("Destruction Settings")]
    [SerializeField] private float maxHeight = 1250f;

    [Header("Text Formatting")]
    [SerializeField] private int maxCharsPerLine = 35;

    [Header("Inspector Input")]
    [SerializeField] private string inspectorMessage;
    [SerializeField] private bool sendMessage;

    private List<RectTransform> chatBubbles = new List<RectTransform>();

    // Dictionary to store message callbacks
    private Dictionary<string, Action> messageCallbacks = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

    // Flag to track if a sentence has been processed
    private bool isSentenceProcessed = false;

    private void Start()
    {
        VerticalLayoutGroup verticalLayout = contentContainer.gameObject.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
        {
            verticalLayout = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        verticalLayout.spacing = bubbleSpacing;
        verticalLayout.padding = new RectOffset((int)leftRightPadding, (int)leftRightPadding, 0, (int)bottomPadding);
        verticalLayout.childAlignment = TextAnchor.LowerCenter;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;

        ContentSizeFitter contentSizeFitter = contentContainer.gameObject.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void Update()
    {
        // Check if the current game mode is StoryMode
        bool isStoryMode = GameController.Instance != null && GameController.Instance.GetCurrentGameMode() == "Story";

        if (typeAlongTest.IsSentenceComplete() && !isSentenceProcessed)
        {
            // Mark the sentence as processed
            isSentenceProcessed = true;

            SendPlayerMessage(typeAlongTest.GetCompletedSentence());
            typeAlongTest.ClearInputAndDisplay();

            // Only set a random sentence if not in StoryMode
            if (!isStoryMode)
            {
                typeAlongTest.SetRandomSentence();
            }
        }
        else if (!typeAlongTest.IsSentenceComplete())
        {
            // Reset the flag when the sentence is no longer complete
            isSentenceProcessed = false;
        }

        if (sendMessage)
        {
            if (!string.IsNullOrEmpty(inspectorMessage))
            {
                SendFriendMessage(inspectorMessage);
                inspectorMessage = "";
            }
            sendMessage = false;
        }
    }

    public void RegisterMessageCallback(string message, Action callback)
    {
        if (messageCallbacks.ContainsKey(message))
        {
            Debug.LogWarning($"Message '{message}' is already registered. Overwriting the existing callback.");
        }

        messageCallbacks[message] = callback;
    }

    // Unregister a callback for a specific message
    public void UnregisterMessageCallback(string message)
    {
        if (messageCallbacks.ContainsKey(message))
        {
            messageCallbacks.Remove(message);
        }
    }

    public void SendPlayerMessage(string message)
    {

        GameObject bubble = Instantiate(playerBubblePrefab, contentContainer);
        ConfigureBubble(bubble, message, true);
        ScrollToBottom();
        DestroyBubblesIfNeeded();

    }

    public void OnPlayerMessageSent(string message)
    {
        SendPlayerMessage(message);

        // Check if the message matches any registered callback
        if (messageCallbacks.TryGetValue(message.Trim(), out Action callback))
        {
            callback.Invoke(); // Trigger the callback
        }
        else
        {
            // Only set a random sentence if not in StoryMode
            bool isStoryMode = GameController.Instance != null && GameController.Instance.GetCurrentGameMode() == "Story";
            if (!isStoryMode)
            {
                typeAlongTest.SetRandomSentence();
            }
        }
    }

    public void SendFriendMessage(string message)
    {
        GameObject bubble = Instantiate(friendBubblePrefab, contentContainer);
        ConfigureBubble(bubble, message, false);
        phoneAudioManager.PlayMessageReceived();

        ScrollToBottom();
        DestroyBubblesIfNeeded();
    }

    private void ConfigureBubble(GameObject bubble, string message, bool isPlayer)
    {
        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        if (bubbleRect == null) return;

        chatBubbles.Add(bubbleRect);

        TMPro.TextMeshProUGUI textComponent = bubble.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = SplitTextIntoLines(message, maxCharsPerLine);
        }
    }

    private string SplitTextIntoLines(string text, int maxCharsPerLine)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string[] words = text.Split(' ');
        string result = "";
        string currentLine = "";

        foreach (string word in words)
        {
            if ((currentLine + " " + word).Length > maxCharsPerLine)
            {
                result += currentLine + "\n";
                currentLine = word;
            }
            else
            {
                currentLine += (currentLine.Length > 0 ? " " : "") + word;
            }
        }

        result += currentLine;
        return result;
    }

    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private System.Collections.IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    private void DestroyBubblesIfNeeded()
    {
        while (contentContainer.rect.height > maxHeight && chatBubbles.Count > 0)
        {
            RectTransform oldestBubble = chatBubbles[0];
            if (oldestBubble == null)
            {
                chatBubbles.RemoveAt(0);
                continue;
            }

            Destroy(oldestBubble.gameObject);
            chatBubbles.RemoveAt(0);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        }
    }

    public void ClearChat()
    {
        foreach (RectTransform bubble in chatBubbles)
        {
            if (bubble != null)
            {
                Destroy(bubble.gameObject);
            }
        }
        chatBubbles.Clear();
    }
}