using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Global değerler (sahneden sahneye kaybolmaz)
    public int currentLives = 5;
    public int currentScore = 0;
    public int currentLevel = 1;

    // Sahne içi referanslar (her level sahnesinde yeniden bulunur)
    private TextMeshProUGUI livesText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI gameOverText;
    private TextMeshProUGUI levelText;
    //private bool gameOverTextCached = false;
    public Transform paddle;
    public GameObject ballPrefab;
   
    public int activeBalls = 0;

    void Awake()
    {
        // Tek GameManager oluştur
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Sahne değişince UI'yı yeniden bul
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
   
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Her sahne açıldığında UI ve Paddle’ı tekrar bulur
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneObjects();
        UpdateUI();
        SpawnBallIfNone();

        if (gameOverText != null)
            return;

        GameObject go = GameObject.Find("GameOver_text");

        if (go != null)
        {
            gameOverText = go.GetComponent<TextMeshProUGUI>();
            gameOverText.gameObject.SetActive(false);
            //Debug.Log("GameOverText bulundu ve kapatıldı");
        }
        else
        {
           // Debug.Log("Bu sahnede GameOverText yok (şimdilik)");
        }

        GameObject levelObj = GameObject.Find("LevelText");
        if (levelObj != null)
        {
            levelText = levelObj.GetComponent<TextMeshProUGUI>();
            levelText.gameObject.SetActive(false);

            
        }

        if (scene.name.StartsWith("Level"))
        {
            string number = scene.name.Replace("Level", "");
            if (int.TryParse(number, out int lvl))
            {
                currentLevel = lvl;
            }
        }
            StartCoroutine(ShowLevelText());

    }

    // O sahnedeki UI objelerini otomatik bulur
    void FindSceneObjects()
    {
        livesText = GameObject.Find("LivesText")?.GetComponent<TextMeshProUGUI>();
        scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        //gameOverText = GameObject.Find("GameOver_text")?.GetComponent<TextMeshProUGUI>();
        GameObject levelObj = GameObject.Find("LevelText");

        GameObject paddleObj = GameObject.Find("Paddle");
        if (paddleObj != null) paddle = paddleObj.transform;
    }

    // ---------------------- SCORE ----------------------
    public void AddScore(int amount)
    {
        currentScore += amount;
        if (currentScore < 0) currentScore = 0;
        UpdateUI();
    }

    // ---------------------- LIVES ----------------------
    public void AddLife(int amount)
    {
        currentLives += amount;
        UpdateUI();
    }

    public void RegisterBall() => activeBalls++;

    public void UnregisterBall()
    {
        activeBalls--;
        if (activeBalls <= 0) LoseLife();
    }

    public void ResetBallsForNewLevel()
    {
        activeBalls = 0;
    }

    public void LoseLife()
    {
        currentLives--;
        
        if (paddle != null)
        {
            var pc = paddle.GetComponent<PaddleController>();
            if (pc != null) pc.ResetSize();
        }
        FindAnyObjectByType<PaddleController>().OnLoseLife();
        UpdateUI();

        // can bitti mi?
        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        SpawnNewBallOnPaddle();
    }

    // ---------------------- BALL SPAWN ----------------------
    void SpawnBallIfNone()
    {
        var existingBalls = Object.FindObjectsByType<BallController>(FindObjectsSortMode.None);

        if (existingBalls.Length == 0) SpawnNewBallOnPaddle();
    }

    void SpawnNewBallOnPaddle()
    {
        if (ballPrefab == null || paddle == null) return;

        GameObject b = Instantiate(ballPrefab);
        BallController bc = b.GetComponent<BallController>();
        bc.attachedToPaddle = true;
        bc.paddle = paddle;
    }


    // ---------------------- LEVEL MANAGEMENT ----------------------
    void Update()
    {
        int blocks = GameObject.FindGameObjectsWithTag("Block").Length;

        if (blocks == 0)
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }


        
    }

    void LoadNextLevel()
    {
        currentLevel++;
        GameManager.instance.ResetBallsForNewLevel();
        


        int nextIndex = SceneManager.GetActiveScene().buildIndex;

        if (nextIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex + 1);
        }
        else
        {
            //Debug.Log("Tüm seviyeler bitti.");
            HighScoreManager.instance.AddScore(currentScore);
            SceneManager.LoadScene(4);
        }
    }

    // ---------------------- GAME OVER ----------------------
    


    void GameOver()
    {
        //Debug.Log("GAME OVER ÇAĞRILDI");

        if (gameOverText != null)
        {

            gameOverText.gameObject.SetActive(true);
        }
        
        HighScoreManager.instance.AddScore(currentScore);
        Invoke(nameof(ReturnToMenu), 4f);
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
       
    }

    IEnumerator ShowLevelText()
    {
        if (levelText == null) yield break;

        levelText.text = "LEVEL " + currentLevel;
        levelText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.75f);

        levelText.gameObject.SetActive(false);
    }

    
    public void NewGame()
    {
        GameManager.instance.ResetAll();
        SceneManager.LoadScene("Level1");
    }

    // ---------------------- RESET SYSTEM ----------------------
    public void ResetAll()
    {
        currentLives = 5;
        currentScore = 0;
        currentLevel = 1;
    }

    // ---------------------- UI ----------------------
    void UpdateUI()
    {
        if (livesText != null) livesText.text = "X - " + currentLives;
        if (scoreText != null) scoreText.text = "SCORE: " + currentScore.ToString("D7");
    }
}

//Object.FindObjectsByType<BallController>(FindObjectsSortMode.None);
