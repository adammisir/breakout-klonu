using UnityEngine;

public class BlockHealth : MonoBehaviour
{
    public int hitsToBreak = 2;                 // bu blogun kac vurusta kırılacagı
    public Sprite damagedSprite;                // 1. vurustan sonra degisecek sprite
    private SpriteRenderer sr;

    private float lastMeteorHitTime = -10f;
    public float meteorHitCooldown = 0.15f;


    [Header("Power Up Settings")]
    public GameObject powerUpF;
    public GameObject powerUpB;
    public GameObject powerUpS;
    public GameObject powerUpG;
    public GameObject powerUpT;
    public GameObject powerUpH;
    public GameObject powerUpM;

    [Range(0f, 1f)]
    public float dropChance = 0.2f; // %20 ihtimalle powerup düşer

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
        if (
        !hitter.CompareTag("Ball") && !hitter.CompareTag("Bullet") && !hitter.CompareTag("Meteor"))
    
            return;

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
            DropPowerUp();
            Destroy(gameObject);

            
        }

        // kırılmaz bloklara yanlışlıkla düşmesin diye:
        // GameManager.instance.CheckLevelComplete();  // Eğer kullanıyorsan buraya ekleyebilirsin
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
