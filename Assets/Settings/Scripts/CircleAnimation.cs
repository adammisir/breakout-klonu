using System.Collections;
using UnityEngine;

public class CircleAnimation : MonoBehaviour
{
    [Header("Sprite Ayarları")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite tamDaireSprite;
    [SerializeField] private Sprite ortasiYokSprite;
    [SerializeField] private Sprite altiYokSprite;

    [Header("Dönme Ayarları")]
    [SerializeField] private float donmeHizi = 470f; // Saniyede kaç derece dönecek?
    [SerializeField] private float donmeSuresi = 1f; // Kaç saniye boyunca dönecek?

    private void Start()
    {
        // Eğer SpriteRenderer elle atanmadıysa otomatik bulmaya çalış
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Animasyon sürecini başlat
        StartCoroutine(AnimasyonSureci());
    }

    private IEnumerator AnimasyonSureci()
    {
        // 1. Aşama: İlk başta tam daire olarak başla
        spriteRenderer.sprite = tamDaireSprite;
        yield return new WaitForSeconds(0.07f);

        // 2. Aşama: Ortası yok olan sprite'a geç
        spriteRenderer.sprite = ortasiYokSprite;
        yield return new WaitForSeconds(0.07f);

        // 3. Aşama: Altı yok olan sprite'a geç
        spriteRenderer.sprite = altiYokSprite;
        yield return new WaitForSeconds(0.07f);

        // 4. Aşama: Kendi etrafında Z ekseninde dönme
        float gecenSure = 0f;
        while (gecenSure < donmeSuresi)
        {
            // Z ekseninde sürekli döndürür
            transform.Rotate(0, 0, donmeHizi * Time.deltaTime);
            gecenSure += Time.deltaTime;
            yield return null; // Bir sonraki frame'e kadar bekle
        }

        // 5. Aşama: Yok et (Objeyi sahneden siler)
        Destroy(gameObject);
    }
}