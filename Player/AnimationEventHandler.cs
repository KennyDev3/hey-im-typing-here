using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    // Call this method from animation events
    public void ReleaseAnimation()
    {
        PlayerController.ReleaseAnimationLock(); // Call the static method
        //Debug.Log("Animation is free");
    }
}
