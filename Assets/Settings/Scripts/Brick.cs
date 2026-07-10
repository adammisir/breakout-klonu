using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Settings")]
    public bool isIndestructible = false;

    [Header("Power Up Settings")]
    public GameObject powerUpF;
    public GameObject powerUpB;
    public GameObject powerUpS;
    public GameObject powerUpG;
    public GameObject powerUpT;
    public GameObject powerUpH;
    public GameObject powerUpM;

    [Range(0f, 1f)]
    public float dropChance = 0.2f; // %20 ihtimalle powerup düþer

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public Sprite IndestructibleSprite;
    private SpriteRenderer sr;
    private bool isFlashing = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (isIndestructible && IndestructibleSprite != null)
        {
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
                // Polygon collider'ý yeni L þeklindeki kýrýlamaz sprite'ýn sýnýrlarýna göre otomatik yeniden çizer
                Destroy(polygonCollider);

                gameObject.AddComponent<PolygonCollider2D>();
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
        Destroy(gameObject);
    }

    void DropPowerUp()
    {
        if (Random.value > dropChance)
            return;

        GameObject[] list = { powerUpF, powerUpB, powerUpS, powerUpG, powerUpT, powerUpH, powerUpM };
        GameObject chosen = list[Random.Range(0, list.Length)];

        Instantiate(chosen, transform.position, Quaternion.identity);
    }
}
