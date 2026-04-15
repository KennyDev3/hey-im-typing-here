using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PhoneChatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerBubblePrefab;
    [SerializeField] private GameObject friendBubblePrefab;
    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Layout Settings")]
    [SerializeField] private float bubbleSpacing = 10f;
    [SerializeField] private float bottomPadding = 20f;
    [SerializeField] private float leftRightPadding = 20f;

    [Header("Destruction Settings")]
    [SerializeField] private float destroyYThreshold = 1270f; // Y position at which bubbles are destroyed

    private List<RectTransform> chatBubbles = new List<RectTransform>();

    private void Start()
    {
        // Ensure the content container uses vertical layout
        VerticalLayoutGroup verticalLayout = contentContainer.gameObject.GetComponent<VerticalLayoutGroup>();
        if (verticalLayout == null)
        {
            verticalLayout = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        // Configure the vertical layout
        verticalLayout.spacing = bubbleSpacing;
        verticalLayout.padding = new RectOffset((int)leftRightPadding, (int)leftRightPadding, 0, (int)bottomPadding);
        verticalLayout.childAlignment = TextAnchor.LowerCenter;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;

        // Add content size fitter to handle dynamic content
        ContentSizeFitter contentSizeFitter = contentContainer.gameObject.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void SendPlayerMessage(string message)
    {
        GameObject bubble = Instantiate(playerBubblePrefab, contentContainer);
        ConfigureBubble(bubble, message, true);
        ScrollToBottom();
        DestroyBubblesAboveThreshold();
    }

    public void SendFriendMessage(string message)
    {
        GameObject bubble = Instantiate(friendBubblePrefab, contentContainer);
        ConfigureBubble(bubble, message, false);
        ScrollToBottom();
        DestroyBubblesAboveThreshold();
    }

    private void ConfigureBubble(GameObject bubble, string message, bool isPlayer)
    {
        // Get the bubble's RectTransform
        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        chatBubbles.Add(bubbleRect);

        // Set horizontal anchoring based on whether it's player or friend message
        if (isPlayer)
        {
            bubbleRect.anchorMin = new Vector2(1, 0);
            bubbleRect.anchorMax = new Vector2(1, 0);
            bubbleRect.pivot = new Vector2(1, 0);
        }
        else
        {
            bubbleRect.anchorMin = new Vector2(0, 0);
            bubbleRect.anchorMax = new Vector2(0, 0);
            bubbleRect.pivot = new Vector2(0, 0);
        }

        // Set the message text
        TMPro.TextMeshProUGUI textComponent = bubble.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }

        // Force layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);

        // Debug: Print the bubble index and Y position
        Debug.Log($"Bubble {chatBubbles.Count - 1} instantiated at Y = {bubbleRect.anchoredPosition.y}");
    }

    private void ScrollToBottom()
    {
        // Wait for end of frame to ensure layout has been updated
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private System.Collections.IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    private void DestroyBubblesAboveThreshold()
    {
        // Iterate through all chat bubbles
        for (int i = chatBubbles.Count - 1; i >= 0; i--)
        {
            RectTransform bubble = chatBubbles[i];
            if (bubble == null)
            {
                chatBubbles.RemoveAt(i);
                continue;
            }

            // Debug: Print the bubble index and Y position
            Debug.Log($"Checking Bubble {i} at Y = {bubble.anchoredPosition.y}");

            // Check if the bubble's Y position is above the threshold
            if (bubble.anchoredPosition.y >= destroyYThreshold)
            {
                // Debug: Print that the bubble is being destroyed
                Debug.Log($"Destroying Bubble {i} at Y = {bubble.anchoredPosition.y}");

                // Destroy the bubble and remove it from the list
                Destroy(bubble.gameObject);
                chatBubbles.RemoveAt(i);
            }
        }
    }

    // Optional: Method to clear all messages
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