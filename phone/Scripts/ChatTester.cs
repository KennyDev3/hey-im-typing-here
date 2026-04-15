using UnityEngine;

public class ChatTester : MonoBehaviour
{
    public PhoneChatManager phoneChatManager; // Reference to the PhoneChatManager

    void Start()
    {
        phoneChatManager.SendPlayerMessage("Hello!");
        phoneChatManager.SendFriendMessage("Hi there!");

    }
}
