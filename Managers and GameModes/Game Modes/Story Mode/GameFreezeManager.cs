using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameFreezeManager : MonoBehaviour
{
    public static GameFreezeManager Instance; // Singleton pattern

    public LevelGenerator levelGenerator;
    public PlayerController playerController;
    public PlayerInput playerInput;
    public Animator gensuitAnimator;
    public TMP_InputField playerChatInput; // Add reference to the TMP_InputField


    private float originalMoveSpeed;
    private bool isFrozen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FreezeGame()
    {
        if (isFrozen) return; // Prevent multiple freezes

        if (levelGenerator != null)
        {
            originalMoveSpeed = levelGenerator.GetMoveSpeed();
            levelGenerator.SetMoveSpeed(0);
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        if (gensuitAnimator != null)
        {
            gensuitAnimator.enabled = false;
        }

        if (playerChatInput != null) // Disable TMP_InputField
        {
            playerChatInput.enabled = false;
        }

        isFrozen = true;
    }

    public void UnfreezeGame()
    {
        if (!isFrozen) return; // Prevent unfreezing if not frozen

        if (levelGenerator != null)
        {
            levelGenerator.SetMoveSpeed(originalMoveSpeed);
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (gensuitAnimator != null)
        {
            gensuitAnimator.enabled = true;
        }

        isFrozen = false;
    }

    public bool IsGameFrozen()
    {
        return isFrozen;
    }
}
