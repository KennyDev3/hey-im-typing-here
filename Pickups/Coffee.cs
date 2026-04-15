using UnityEngine;

public class Coffee : Pickups
{
    [SerializeField] float adjustChangeMoveSpeedAmount = 1f;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;

    LevelGenerator levelGenerator;

    public void Init(LevelGenerator lg)
    {
        levelGenerator = lg;
    }

    protected override void OnPickup()
    {
        animator.SetTrigger("PickUp");
        audioSource.Play();
        levelGenerator.ChangeChunkMoveSpeed(adjustChangeMoveSpeedAmount);
        Debug.Log("You are healthy");
    }

    protected override void DestroyModel()
    {
        // Do nothing to prevent model destruction
    }
}