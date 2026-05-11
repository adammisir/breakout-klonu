using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    void Update()
    {
        // Sahnedeki tüm "Block" tag'li objeleri say
        int kalanBlok = GameObject.FindGameObjectsWithTag("Block").Length;

        // Blok kalmadýysa sonraki sahneye geç
        if (kalanBlok == 0)
        {
            BirSonrakiLevel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }


    }

    void BirSonrakiLevel()
    {
        int mevcutIndex = SceneManager.GetActiveScene().buildIndex;
        int toplamSahne = SceneManager.sceneCountInBuildSettings;

        // Eðer son level deðilse bir sonraki level'e geç
        if (mevcutIndex + 1 < toplamSahne)
        {
            SceneManager.LoadScene(mevcutIndex + 1);
        }
        else
        {
           // Debug.Log("Tüm Level'lar bitti!");
            // Ýstersen burada ana menüye döndürebilirsin:
            // SceneManager.LoadScene("MainMenu");
        }
    }
}
