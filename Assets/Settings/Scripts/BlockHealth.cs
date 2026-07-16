using System.Collections;
using UnityEngine;

public class BlockHealth : MonoBehaviour
{
    public int hitsToBreak = 2;                 // bu blogun kac vurusta kırılacagı
    public Sprite damagedSprite;                // 1. vurustan sonra degisecek sprite
    private SpriteRenderer sr;

    private float lastMeteorHitTime = -10f;
    public float meteorHitCooldown = 0.15f;

    [Header("Power Up Settings")]
    [Range(0f, 1f)]
    public float globalDropChance = 0.3f; // %30 ihtimalle powerUp düşer
    public PowerUpEntry[] powerUps;

    [System.Serializable]
    public class PowerUpEntry
    {
        public GameObject prefab;
        [Range(0f, 10f)]
        public float weight; // ağırlık, yüksek olursa daha sık seçilir
    }


    [Header("Explosion")]
    public GameObject explosionPrefab;


    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        HandleHit(col.gameObject);
    }

    // Eğer collider'lar normal collision ise burası çalışır
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject hitter)
    {
        // Sadece Ball ve Bullet çarptığında işlem yap
        if ( !hitter.CompareTag("Ball") && !hitter.CompareTag("Bullet") && !hitter.CompareTag("Meteor"))
    
            return;

        StartCoroutine(ParlamaEfekti());
        hitsToBreak--;

        

        // 1. vuruş sonrası görünüş değişsin
        if (hitsToBreak == 1 && damagedSprite != null)
        {
            sr.sprite = damagedSprite;
        }

        if (hitter.CompareTag("Meteor"))
        {
            if (Time.time - lastMeteorHitTime < meteorHitCooldown)
                return;

            lastMeteorHitTime = Time.time;
        }

        // kırılma anı
        if (hitsToBreak <= 0)
        {

            StartCoroutine(KirilmaVeEfektSureci());
            DropPowerUp();
        }

        // kırılmaz bloklara yanlışlıkla düşmesin diye:
        // GameManager.instance.CheckLevelComplete();  // Eğer kullanıyorsan buraya ekleyebilirsin
    }
    IEnumerator ParlamaEfekti()
    {
        // Bloğun senin panelde ayarladığın güncel rengini hafızaya alıyoruz
        Color eskiRenk = sr.color;

        // Bloğu görünmeyecek kadar bembeyaz yap (RGB değerlerini patlatıyoruz)
        float parlaklik = 5f;
        sr.color = new Color(parlaklik, parlaklik, parlaklik, 1f);

        // Ne kadar süre beyaz kalacağını burada belirliyoruz
        yield return new WaitForSeconds(0.15f);

        // Süre bitince hafızaya aldığımız kendi rengine geri döndürüyoruz
        if (sr != null)
        {
            sr.color = eskiRenk;
        }
    }
    IEnumerator KirilmaVeEfektSureci()
    {
        // 1. AŞAMA: Bloğu bembeyaz parlat
        Color eskiRenk = sr.color;
        float parlaklik = 5f;
        sr.color = new Color(parlaklik, parlaklik, parlaklik, 1f);

        // 2. AŞAMA: BEKLEMEDEN AYNI ANDA patlama efektlerini oluştur
        int explosionCount = transform.localScale.y > 0.7f ? 3 : 1;
        float longestDuration = 0f;

        if (explosionPrefab != null)
        {
            for (int i = 0; i < explosionCount; i++)
            {
                Vector3 offset = Vector3.zero;

                if (explosionCount == 3)
                {
                    float xPos = (i - 1) * 0.5f;
                    offset = new Vector3(xPos, 0f, 0f);
                }

                GameObject fx = Instantiate(explosionPrefab, transform.position + offset, Quaternion.identity); 

                ParticleSystem ps = fx.GetComponent<ParticleSystem>(); 
               if (ps != null) 
               {
                    float duration = ps.main.duration + ps.main.startLifetime.constantMax;
                if (duration > longestDuration) 
                    longestDuration = duration;
               }
            }
        }

        // 3. AŞAMA: İki efekt aynı anda oynarken parlama süresi kadar (0.15s) bekle
        yield return new WaitForSeconds(0.15f);

        // 4. AŞAMA: Parlama süresi bittiğinde bloğun görselini gizle (Patlama arkada oynamaya devam eder)
        sr.enabled = false;

        // 5. AŞAMA: Patlama efektinin tamamen bitmesi için kalan süreyi bekle ve objeyi yok et
        yield return new WaitForSeconds(0.15f);
        Destroy(gameObject); 
    }

    void DropPowerUp()
    {
        // Önce genel şans kontrolü
        if (Random.value > globalDropChance) return;

        // Toplam ağırlığı hesapla
        float totalWeight = 0f;
        foreach (var entry in powerUps)
            if (entry.prefab != null)
                totalWeight += entry.weight;

        if (totalWeight == 0f) return;

        // Ağırlığa göre rastgele seç
        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in powerUps)
        {
            if (entry.prefab == null) continue;
            cumulative += entry.weight;
            if (roll <= cumulative)
            {
                Instantiate(entry.prefab, transform.position, Quaternion.identity);
                return;
            }
        }
    }
}
