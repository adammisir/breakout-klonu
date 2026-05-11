using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public void NewGame()
    {
        GameManager.instance.ResetAll();
        GameManager.instance.ResetBallsForNewLevel();
        SceneManager.LoadScene("Level1");
    }

    public void ExitGame()
    {
        Application.Quit();
        //Debug.Log("Game Quit"); // Editor için
    }
    public void GoToLevelSelect()
    {
        GameManager.instance.ResetAll();
        GameManager.instance.ResetBallsForNewLevel();
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void HighScore()
    {
        SceneManager.LoadScene(2);
    }
    // Select Level ve High Score daha sonra buraya eklenecek
}
