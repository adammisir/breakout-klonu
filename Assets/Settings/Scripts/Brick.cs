using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Settings")]
    public bool isIndestructible = false;

    [Header("Power Up Settings")]
    [Range(0f, 1f)]
    public float globalDropChance = 0.3f; // %30 ihtimalle powerUp düþer
    public PowerUpEntry[] powerUps;


    [System.Serializable]
    public class PowerUpEntry
    {
        public GameObject prefab;
        [Range(0f, 10f)]
        public float weight; // aðýrlýk, yüksek olursa daha sýk seçilir
    }

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public Sprite IndestructibleSprite;
    private SpriteRenderer sr;
    private bool isFlashing = false;
    private bool isDestroyed = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        GameManager.instance?.RegisterBlock();
        


        if (isIndestructible && IndestructibleSprite != null)
        {
            GameManager.instance.blockCount--;
            // 1. ADIM: Eski sprite'ýn o seviyedeki GERÇEK dünya boyutunu (Renderer Bounds) kaydet
            // Bu sayede o seviyede bloðu ne kadar esnettiysen esnet, tam o kutu boyutunu cebimize koyuyoruz.
            Vector3 eskiGercekBoyut = sr.bounds.size;

            // 2. ADIM: Yeni kýrýlamaz sprite'ýný ata
            sr.sprite = IndestructibleSprite;

            // 3. ADIM: Scale deðerini 1,1,1 yapýp sýfýrlýyoruz ki temiz bir hesaplama yapalým
            transform.localScale = Vector3.one;

            // 4. ADIM: Yeni sprite'ýn ham boyutunu alýp, eski gerçek boyuta ulaþmak için gereken yeni scale'i hesapla
            Vector3 yeniSpriteHamBoyutu = sr.bounds.size;

            transform.localScale = new Vector3(
                eskiGercekBoyut.x / yeniSpriteHamBoyutu.x,
                eskiGercekBoyut.y / yeniSpriteHamBoyutu.y,
                1f
            );
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                // Bu iki satýr collider'ý tamamen silip yeniden eklemiþ gibi 
                // yeni sprite'ýn sýnýrlarýna (bounds) otomatik olarak sýfýrlar ve eþitler.
                boxCollider.size = sr.sprite.bounds.size;
                boxCollider.offset = sr.sprite.bounds.center;
            }
            PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
            if (polygonCollider != null)
            {
                // Sprite'dan fizik þeklini uygula
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    polygonCollider.pathCount = sr.sprite.GetPhysicsShapeCount();
                    for (int i = 0; i < polygonCollider.pathCount; i++)
                    {
                        var path = new System.Collections.Generic.List<Vector2>();
                        sr.sprite.GetPhysicsShape(i, path);
                        polygonCollider.SetPath(i, path);
                    }
                }
            }
        }
    }

    // Eðer collider'lar trigger ise burasý çalýþýr
    void OnTriggerEnter2D(Collider2D col)
    {

        HandleHit(col.gameObject);
    }

    // Eðer collider'lar normal collision ise burasý çalýþýr
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject hitter)
    {
        if (isDestroyed) return;
        // Sadece Ball veya Bullet ile tepki ver
        if (!hitter.CompareTag("Ball") && !hitter.CompareTag("Bullet") && !hitter.CompareTag("Meteor"))
            return;


        
        if (isIndestructible)
        {
            StartCoroutine(ParlamaEfekti());
            return;
        }   
        
        StartCoroutine(KirilmaVeEfektSureci());

        DropPowerUp();
        isDestroyed = true;
    }

    IEnumerator ParlamaEfekti()
    {
        if (isFlashing) yield break;

        isFlashing = true;
        // Bloðun senin panelde ayarladýðýn güncel rengini hafýzaya alýyoruz
        Color eskiRenk = sr.color;

        // Bloðu görünmeyecek kadar bembeyaz yap (RGB deðerlerini patlatýyoruz)
        float parlaklik = 5f;
        sr.color = new Color(parlaklik, parlaklik, parlaklik, 1f);

        // Ne kadar süre beyaz kalacaðýný burada belirliyoruz
        yield return new WaitForSeconds(0.15f);

        // Süre bitince hafýzaya aldýðýmýz kendi rengine geri döndürüyoruz
        if (sr != null)
        {
            sr.color = eskiRenk;
        }
        isFlashing = false;
    }
    IEnumerator KirilmaVeEfektSureci()
    {
        // 1. AÞAMA: Bloðu bembeyaz parlat
        Color eskiRenk = sr.color;
        float parlaklik = 5f;
        sr.color = new Color(parlaklik, parlaklik, parlaklik, 1f);

        // 2. AÞAMA: BEKLEMEDEN AYNI ANDA patlama efektlerini oluþtur
        int explosionCount = transform.localScale.x > 4f ? 3 : 1;
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

        // 3. AÞAMA: Ýki efekt ayný anda oynarken parlama süresi kadar (0.15s) bekle
        yield return new WaitForSeconds(0.15f);

        // 4. AÞAMA: Parlama süresi bittiðinde bloðun görselini gizle (Patlama arkada oynamaya devam eder)
        sr.enabled = false;

        // 5. AÞAMA: Patlama efektinin tamamen bitmesi için kalan süreyi bekle ve objeyi yok et
        yield return new WaitForSeconds(0.15f);
        GameManager.instance?.CheckLevelComplete();
        Destroy(gameObject);
    }

    void DropPowerUp()
    {
        // Önce genel þans kontrolü
        if (Random.value > globalDropChance) return;

        // Toplam aðýrlýðý hesapla
        float totalWeight = 0f;
        foreach (var entry in powerUps)
            if (entry.prefab != null)
                totalWeight += entry.weight;

        if (totalWeight == 0f) return;

        // Aðýrlýða göre rastgele seç
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
