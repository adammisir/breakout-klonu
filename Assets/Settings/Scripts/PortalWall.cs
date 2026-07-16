using UnityEngine;

public class PortalWall : MonoBehaviour
{
    public static bool isActive = false;
    private static float timer = 0f;
    private static float duration = 60f;

  

    public static PortalWall leftWall;
    public static PortalWall rightWall;

    public bool isLeftWall;

    private SpriteRenderer sr;
    private Color originalColor;

    [Header("Portal Settings")]
    public Color portalColor = new Color(0.3f, 0f, 0.6f, 1f);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        isLeftWall = transform.position.x < 0f;

        if (isLeftWall)
            leftWall = this;
        else
            rightWall = this;

        //Debug.Log($"{gameObject.name} - isLeftWall:{isLeftWall} leftWallX:{leftWallX} rightWallX:{rightWallX}");
    }

    void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;

        // Rengi güncelle
        if (sr != null) sr.color = portalColor;

        if (timer <= 0f)
        {
            Deactivate(); // Süre bittiğinde her şeyi temizleyen yeni fonksiyonumuz
        }
    } 

   public static void Activate()
    {
        isActive = true;
        timer = duration;

        // Sahnedeki tüm portal duvarlarını bul ve koyu portal rengine boya
        foreach (PortalWall pw in FindObjectsByType<PortalWall>())
        {
            if (pw.sr != null)
                pw.sr.color = pw.portalColor;
        }
    }

    // Portalı Kapatma (Orijinal Renge Döndür)
    public static void Deactivate()
    {
        isActive = false;
        timer = 0f;

        // Sahnedeki tüm portal duvarlarını bul ve orijinal renklerine geri döndür
        foreach (PortalWall pw in FindObjectsByType<PortalWall>())
        {
            if (pw.sr != null)
                pw.sr.color = pw.originalColor;
        }
    }
}