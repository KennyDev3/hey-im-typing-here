using TMPro;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
   [SerializeField] TMP_Text tmp_text;
   GameManager gameManager;

    const string Player = "Player";
    float textToDisplay = 0f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Player))
        {
            gameManager.AddTimeCheckpoint();
            Debug.Log("Youve ran through you homo");

        }

    }

    private void Awake()
    {
        gameManager = GameManager.Instance;
        textToDisplay = gameManager.checkpointAddedTime;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmp_text.text = textToDisplay.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
