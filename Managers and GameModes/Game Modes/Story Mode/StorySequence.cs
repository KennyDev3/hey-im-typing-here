using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorySequence", menuName = "Story/StorySequence")]
public class StorySequence : ScriptableObject
{
    public List<StoryEvent> storyEvents;
}
