using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
    public abstract void Initialize();
    public abstract void StartMode();
    public abstract void UpdateMode();
    public abstract void EndMode();
    public abstract void AddTime(float time);
    public abstract float GetTotalTime(); // Add this method

}
