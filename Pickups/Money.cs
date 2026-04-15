using TMPro;
using UnityEngine;

public class Money : Pickups
{
    [SerializeField] private ParticleSystem moneyParticleSystem;
    [SerializeField] GameObject FloatingTextPrefab;
    private Animator animator;

 


    private ScoreManager scoreManager;
    public static float moneyIncreaseAmount = 500f; // Your static variable



    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void Init(ScoreManager sm)
    {
        scoreManager = sm;
    }




    protected override void OnPickup()
    {
        // Instantiate Text
        ShowFloatingText();
        scoreManager.IncreaseMoneyScore(moneyIncreaseAmount);

        animator.SetTrigger("PickUp");
        moneyParticleSystem.Play();
        MoneyAudioPitchManager.Instance.PlayCoinSound();




    }

    protected override void DestroyModel()
    {
        // Do nothing to prevent model destruction
    }

    void ShowFloatingText()
    {
        var go = Instantiate(FloatingTextPrefab, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMeshPro>().text = "+" + moneyIncreaseAmount.ToString();

    }





}