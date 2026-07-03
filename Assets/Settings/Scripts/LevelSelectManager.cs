using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("UI (previewImage isteðe baðlý)")]
    public Image previewImage;            // yoksa býrakýn null
    public Sprite[] levelSprites;         // preview sprite'larý (index = level-1)
    public TextMeshProUGUI levelNameText; // "Level 1" göstergesi

    [Header("Seçim ayarlarý")]
    public int startLevel = 1;
    private int currentLevel;

    void Start()
    {
        // güvenlik: en az 1 level olsun
        if (levelSprites != null && levelSprites.Length > 0)
        {
            currentLevel = Mathf.Clamp(startLevel, 1, levelSprites.Length);
        }
        else
        {
            currentLevel = startLevel;
        }

        UpdateUI();
    }

    void Update()
    {
        // Ok tuþlarý ile seçim
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextLevel();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousLevel();
        }

        // Space ile baþlat
        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetKeyDown(KeyCode.Return)))
        {
            PlaySelectedLevel();
        }

        // ESC ile menüye dön
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMenu();
        }
    }

    public void NextLevel()
    {
        if (levelSprites != null && levelSprites.Length > 0)
        {
            currentLevel++;
            if (currentLevel > levelSprites.Length) currentLevel = 1;
        }
        else
        {
            currentLevel++;
        }
        UpdateUI();
    }

    public void PreviousLevel()
    {
        if (levelSprites != null && levelSprites.Length > 0)
        {
            currentLevel--;
            if (currentLevel < 1) currentLevel = levelSprites.Length;
        }
        else
        {
            currentLevel = Mathf.Max(1, currentLevel - 1);
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        // Level ismini güncelle
        if (levelNameText != null)
            levelNameText.text = "Level " + currentLevel;

        // Preview varsa göster (index hatalarýna karþý kontrol)
        if (previewImage != null && levelSprites != null && levelSprites.Length >= currentLevel && currentLevel > 0)
        {
            previewImage.sprite = levelSprites[currentLevel - 1];
            previewImage.enabled = true;
        }
        else if (previewImage != null)
        {
            previewImage.enabled = false;
        }
    }

    public void PlaySelectedLevel()
    {
        // Sahne adýný "Level{n}" varsayýyoruz
        string sceneName = "Level" + currentLevel;
        // opsiyonel: hata durumunda uyar
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
           // Debug.LogWarning("LevelSelect: Scene not found in Build Settings -> " + sceneName);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
