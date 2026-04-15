using UnityEngine;
using static SoundManager;

public class CarCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Trigger the collision event
        CollisionEventSystem.TriggerCollision();
    }
}
