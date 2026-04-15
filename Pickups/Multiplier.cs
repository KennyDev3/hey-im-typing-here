using TMPro;
using UnityEngine;
public class Multiplier : Pickups
{
    [SerializeField] GameObject FloatingTextPrefab;
    [SerializeField] AudioSource audioSource;

    public float multiplierIncreaseAmount = 10;

    private ScoreManager scoreManager;

    public void Init(ScoreManager sm)
    {
        scoreManager = sm;
    }

    protected override void OnPickup()
    {
        // Instantiate Text
        ShowFloatingText();
        scoreManager.IncreaseMultiplierBy10();
        audioSource.Play();

    }

  

    void ShowFloatingText()
    {
        var go = Instantiate(FloatingTextPrefab, transform.position, Quaternion.identity);
        go.GetComponent<TextMeshPro>().text = "+" + multiplierIncreaseAmount.ToString() + "X";
    }
}