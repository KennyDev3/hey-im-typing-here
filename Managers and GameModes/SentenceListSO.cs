using UnityEngine;
using System.Collections.Generic;

// This attribute allows you to create instances of this object from the Assets menu
[CreateAssetMenu(fileName = "NewSentenceList", menuName = "TypingGame/Sentence List")]
public class SentenceListSO : ScriptableObject
{
    // This is the list of sentences that will be editable in the Inspector
    [TextArea(3, 10)] // Makes the string fields larger in the Inspector
    public List<string> sentences = new List<string>();
}