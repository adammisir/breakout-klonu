using UnityEngine;

public class MeteorBall : MonoBehaviour
{
    public float lifetime = 10f;
    public float speed = 14f;
    public GameObject normalBallPrefab;

    private Rigidbody2D rb;
    //private bool transforming = false;

    private float lastTeleportTime = 0f;
    private float teleportCooldown = 0.2f;

    [SerializeField] private Transform trailBall;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Vector2 dir = new Vector2(Random.Range(-0.6f, 0.6f), 1f).normalized;
        rb.linearVelocity = dir * speed;

        Invoke(nameof(TransformToNormalBall), lifetime);

        GameManager.instance.RegisterBall();
    }

    void LateUpdate()
    {
        if (rb == null) return;

        Vector2 dir = rb.linearVelocity.normalized;

        trailBall.localPosition = -dir * 0.7f;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        
        if (col.CompareTag("Block"))
        {
            
            GameManager.instance?.AddScore(100);
            return;
        }

        // Paddle sekmesi (normal top fiziği gibi)
        if (col.CompareTag("Paddle"))
        {
            float paddleX = col.transform.position.x;
            float hitX = transform.position.x;
            float offset = (hitX - paddleX) * 3f; // kenarlarda daha yana seker

            Vector2 newDir = new Vector2(offset, 1).normalized;
            rb.linearVelocity = newDir * speed;
            return;
        }

        // Sol - sağ duvar
       if (col.CompareTag("SideWall"))
        {
            // Eğer portal aktifse ve cooldown süresi geçmişse ışınla
            if (PortalWall.isActive)
            {
                if (Time.time - lastTeleportTime < teleportCooldown) return;

                PortalWall hitWall = col.gameObject.GetComponent<PortalWall>();
                if (hitWall != null)
                {
                    float newX;

                    // Sol duvara çarptıysak -> Sağ duvara ışınla
                    if (hitWall.isLeftWall && PortalWall.rightWall != null)
                    {
                        Collider2D rightCol = PortalWall.rightWall.GetComponent<Collider2D>();
                        newX = rightCol.bounds.min.x - 0.5f; // Sağ duvarın solundan içeriye ofset
                    }
                    // Sağ duvara çarptıysak -> Sol duvara ışınla
                    else if (!hitWall.isLeftWall && PortalWall.leftWall != null)
                    {
                        Collider2D leftCol = PortalWall.leftWall.GetComponent<Collider2D>();
                        newX = leftCol.bounds.max.x + 0.5f; // Sol duvarın sağından içeriye ofset
                    }
                    else
                    {
                        return;
                    }

                    // Işınlanma zamanını kaydet
                    lastTeleportTime = Time.time;

                    // Meteor topunu yeni pozisyona aktar (hızını ve yönünü bozmadan)
                    transform.position = new Vector3(newX, transform.position.y, transform.position.z);
                    return;
                }
            }

            // Portal aktif değilse normal sekme davranışı
            rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
            return;
        }

        // Üst duvar
        if (col.CompareTag("Wall"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -rb.linearVelocity.y);
            return;
        }

        // DeathZone'u görmezden gelir (meteor asla ölmez)
        if (col.CompareTag("DeathZone"))
        {
            GameManager.instance.activeBalls--;
            Destroy(gameObject);
            return;
        }
    }



    void TransformToNormalBall()
    {
        GameManager.instance.activeBalls--;

        Vector3 center = transform.position;

       // float spawnOffset = 0.7f;

        float currentSpeed = rb.linearVelocity.magnitude;

        Vector2[] directions =
        {
        new Vector2(-1.7f, 1f).normalized, // ↖
        Vector2.up,                         // ↑
        new Vector2(1.7f, 1f).normalized    // ↗
        };



        for (int i = 0; i < 3; i++)
        {
            Vector2[] spawnOffsets =
            {
             new Vector2(-1.5f, 0.9f),
             new Vector2( 0f, 1f),
             new Vector2( 1.5f, 0.9f)
            };

            Vector3 spawnPos = center + (Vector3)spawnOffsets[i];

            GameObject newBall = Instantiate(normalBallPrefab, spawnPos, Quaternion.identity);
            BallController bc = newBall.GetComponent<BallController>();

            bc.SetDirectionAndSpeed(directions[i], currentSpeed);
        }

        Destroy(gameObject);
    }

}
