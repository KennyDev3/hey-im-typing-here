[System.Serializable]
public class StoryEvent
{
    public string friendMessage; // Message from the friend
    public string sentence;      // Sentence for the player to type
    public bool waitForCompletion; // Wait for the player to finish typing
    public float delayBeforeNextEvent; // Delay before the next event

    // Adaptive music 
    public bool changeMusicState;
    public int musicStateIndex = -1;

    public GameplayChange gameplayChange; // Optional gameplay change


}