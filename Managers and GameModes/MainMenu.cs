using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //public void OnScriptedLevelSelected()
    //{
    //    GameManager.Instance.StartGameMode(new ScriptedLevelMode());
    //}

    public void OnTimeAttackSelected()
    {
        SceneManager.LoadScene("TimeAttack");  // Make sure the scene is in your build settings

        // Get the existing TimeAttackMode component from the Managers GameObject
        TimeAttackMode timeAttackMode = GameManager.Instance.GetComponent<TimeAttackMode>();

        if (timeAttackMode != null)
        {
            // Start the game mode
            GameManager.Instance.StartGameMode(timeAttackMode);
        }
        else
        {
            Debug.LogError("TimeAttackMode component not found on the Managers GameObject!");
        }
    }
}