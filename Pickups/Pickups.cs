using System.Collections;
using UnityEngine;

public abstract class Pickups : MonoBehaviour
{
    const string PLAYER_TAG = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            OnPickup();
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        DestroyModel();

        // Destroy the model (if allowed)

        // Wait for 2 seconds before destroying the entire GameObject
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    protected virtual void DestroyModel()
    {
        Transform model = transform.Find("Model"); // Replace "Model" with the name of your model child object
        if (model != null)
        {
            Destroy(model.gameObject);
        }
    }

    protected abstract void OnPickup();
}