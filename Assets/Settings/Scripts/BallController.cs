using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 direction;

    public float initialSpeed = 3f;
    public float speedIncreasePerHit = 0.3f;
    public float maxSpeed = 12f;
    [SerializeField] float minSpeed = 3f;

    private bool hasLaunched = false;

    public bool launched = false;
    public float autoLaunchTime = 10f;
    public float autoLaunchTimer = 0f;  // sayaç (bunu eklemeliyiz)

    private float lastSpawnTime = -10f;
    public float spawnCooldown = 2f; // 2 saniyede bir spawn olabilir

    public AudioClip paddleSound;
    public AudioClip blockSound;
    public AudioClip ironBlockSound;
    public AudioClip WallSouns;

    private static bool isMuted = false;

    private Collider2D lastIronBlock;
    
    private float lastIronHitTime = 0f;

    
    public float ironHitCooldown = 0.2f;
    public int maxRescueSpawns = 1;

   // private int rescueSpawnCount = 0;

    Rigidbody2D rb;
    AudioSource audioSource;


    [Header("Paddle & Starting")]
    public Transform paddle; // inspector'dan ata
    public float paddleOffset = 0.7f;
    public bool attachedToPaddle = true; // -- artık public

    [Header("Small Ball (spawn)")]
    public GameObject smallBallPrefab;  // küçük top prefab'ı
    private int blockHitCount = 0;
    public int hitsToSpawnSmall = 7;

    [Header("Rescue")]
    public float stuckCheckInterval = 0.5f; // her 0.5 saniyede kontrol et
    public float stuckThreshold = 0.5f; // bu kadar hareket etmediyse sıkışmış say

    private Vector2 lastPosition;
    private float stuckTimer;

    private bool touchingWall = false;
    private bool touchingBlock = false;

    private float lastTeleportTime = 0f;
    private float teleportCooldown = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
        audioSource = GetComponent<AudioSource>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        direction = new Vector2(1f, 0.8f).normalized;


        // Eğer inspector'dan atanmamışsa otomatik bulmaya çalış
        if (paddle == null)
        {
            // 1) Önce GameManager üzerinden bak
            if (GameManager.instance != null && GameManager.instance.paddle != null)
            {
                paddle = GameManager.instance.paddle;
            }
            else
            {
                // 2) Son çare: sahnede "Paddle" tag'li obje ara
                GameObject pObj = GameObject.FindWithTag("Paddle");
                if (pObj != null) paddle = pObj.transform;
            }
        }

        autoLaunchTimer = 0f;

        // Başlangıçta paddle'a yapışıksa kinematik yap
        rb.bodyType = attachedToPaddle ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

        // Her top doğduğunda GameManager'e kaydolur
        if (GameManager.instance != null)
            GameManager.instance.RegisterBall();


        rb = GetComponent<Rigidbody2D>();
        //rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;

        lastPosition = transform.position;
        stuckTimer = 0f;
    }

    void Update()
    {
        if (attachedToPaddle)   // Top rakete bağlıysa
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            transform.position = paddle.position + new Vector3(0, paddleOffset, 0);

            // Manuel fırlatma
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Launch();
            }

            // Otomatik fırlatma
            autoLaunchTimer += Time.deltaTime;
            if (autoLaunchTimer >= autoLaunchTime)
            {
                Launch();
            }
        } 
            CheckIfStuck();
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMute();
    }

    void Launch()
    {
        if (hasLaunched) return;

        attachedToPaddle = false;
        hasLaunched = true;

        rb.bodyType = RigidbodyType2D.Dynamic;

        speed = initialSpeed; //  ÖNEMLİ


        float randomX = Random.Range(-1f, 1f);
        direction = new Vector2(randomX, 1f).normalized;
    }

    // Eğer başka yerden attachedToPaddle'ı setliyorsan, timer'ı sıfırlamak için bu metodu kullan
    public void SetAttachedToPaddle(bool attached)
    {
        attachedToPaddle = attached;
        if (attached)
        {
            hasLaunched = false;
            autoLaunchTimer = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void CheckIfStuck()
    {
        if (attachedToPaddle) return;

        stuckTimer += Time.deltaTime;

        if (stuckTimer >= stuckCheckInterval)
        {
            float movedDistance = Vector2.Distance(rb.position, lastPosition);
            bool isStuck = (touchingWall && touchingBlock) || movedDistance < stuckThreshold;

            // Sınır dışına çıktı mı?
            bool isOutOfBounds = false;
            if (PortalWall.leftWall != null && PortalWall.rightWall != null)
            {
                float leftX = PortalWall.leftWall.GetComponent<Collider2D>().bounds.max.x;
                float rightX = PortalWall.rightWall.GetComponent<Collider2D>().bounds.min.x;
                isOutOfBounds = transform.position.x < leftX || transform.position.x > rightX;
            }

            if (isStuck || isOutOfBounds)
            {
                if (paddle != null)
                    transform.position = paddle.position + new Vector3(Random.Range(-0.5f, 0.5f), paddleOffset, 0f);

                float randomX = Random.Range(-1f, 1f);
                direction = new Vector2(randomX, 1f).normalized;
                speed = initialSpeed;
            }

            lastPosition = rb.position;
            stuckTimer = 0f;
        }
    }
    void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;

        MuteButton muteButton = FindAnyObjectByType<MuteButton>();
        if (muteButton != null)
            muteButton.UpdateSprite();
    }
    void FixedUpdate()
    {
        if (!attachedToPaddle)
        {
            rb.linearVelocity = direction * speed; 
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }


        void PreventSticking()
        {
            Vector2 v = rb.linearVelocity;

            if (Mathf.Abs(v.x) < 0.4f)
                v.x = Mathf.Sign(v.x) * 0.4f;

            if (Mathf.Abs(v.y) < 0.4f)
                v.y = Mathf.Sign(v.y) * 0.4f;

            rb.linearVelocity = v.normalized * speed;
        }

        if (hasLaunched)
        {
            PreventSticking();
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (attachedToPaddle) return;

        if (collision.contacts == null || collision.contacts.Length == 0) return;

        Vector2 contactNormal = collision.contacts[0].normal;
        direction = Vector2.Reflect(direction, contactNormal);
        FixDirection();



        if (collision.gameObject.CompareTag("Iron"))
        {
            blockHitCount++;
            audioSource.PlayOneShot(ironBlockSound);

            if (Time.time - lastIronHitTime < ironHitCooldown)
                return;

         

            lastIronHitTime = Time.time;

           
        }

        if (collision.gameObject.CompareTag("Wall")) 
        {
            audioSource.PlayOneShot(WallSouns);

            IncreaseSpeed();
        }

        if (collision.gameObject.CompareTag("SideWall"))
        {
            if (PortalWall.isActive)
            {
                // 1. COOLDOWN KONTROLÜ: Eğer çok yeni ışınlandıysa işlemi pas geç
                if (Time.time - lastTeleportTime < teleportCooldown) return;

                // 2. DOĞRU DUVAR TESPİTİ: Çarptığımız objenin isminde "Left" geçiyor mu veya pozisyonu solda mı?
                // PortalWall scriptindeki 'isLeftWall' değişkenini doğrudan kullanıyoruz.
                PortalWall hitWall = collision.gameObject.GetComponent<PortalWall>();
                if (hitWall == null) return;

                float newX;

                // Eğer sol duvara çarptıysak -> Sağ duvara ışınla
                if (hitWall.isLeftWall && PortalWall.rightWall != null)
                {
                    Collider2D rightCol = PortalWall.rightWall.GetComponent<Collider2D>();
                    // Sağ duvarın sol sınırından sola doğru (içeriye) ofset veriyoruz
                    newX = rightCol.bounds.min.x - 0.5f;
                }
                // Eğer sağ duvara çarptıysak -> Sol duvara ışınla
                else if (!hitWall.isLeftWall && PortalWall.leftWall != null)
                {
                    Collider2D leftCol = PortalWall.leftWall.GetComponent<Collider2D>();
                    // Sol duvarın sağ sınırından sağa doğru (içeriye) ofset veriyoruz
                    newX = leftCol.bounds.max.x + 0.5f;
                }
                else
                {
                    return;
                }

                // Işınlanma zamanını güncelle
                lastTeleportTime = Time.time;

                // Topu yeni pozisyona ışınla
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
                return;
            }

            // Portal aktif değilse normal sekme ve hızlanma
            audioSource.PlayOneShot(WallSouns);
            IncreaseSpeed();
        }

        if (collision.collider.CompareTag("Block"))
        {
            blockHitCount++;
            audioSource.PlayOneShot(blockSound);
            GameManager.instance?.AddScore(100);

            if (blockHitCount >= hitsToSpawnSmall)
            {
                SpawnSmallBall();
                blockHitCount = 0;
                IncreaseSpeed();
            }
        }

        if (collision.collider.CompareTag("Paddle"))
        {
            blockHitCount = 0;
            GameManager.instance?.AddScore(10);
            audioSource.PlayOneShot(paddleSound);

            float offset = transform.position.x - collision.transform.position.x;
            float width = collision.collider.bounds.size.x / 2f;
            float normalized = Mathf.Clamp(offset / width, -1f, 1f);
            direction = new Vector2(normalized, Mathf.Abs(direction.y)).normalized;
            
        }

        //void SpawnRescueBalls()
        //{
        //    Vector2[] rescueDirs =
        //    {
        //        new Vector2(-1f, 1f).normalized,
        //        new Vector2(1f, 1f).normalized
        //    };

        //    foreach (var dir in rescueDirs)
        //    {
        //        GameObject b = Instantiate(smallBallPrefab, transform.position, Quaternion.identity);
        //        BallController bc = b.GetComponent<BallController>();
        //        bc.SetDirectionAndSpeed(dir, speed);
        //    }
        //}

        void IncreaseSpeed()
        {
            speed = Mathf.Min(speed + speedIncreasePerHit, maxSpeed);

        }

        if (hasLaunched)
        {
            IncreaseSpeed();
        }


    }


    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SideWall") || collision.gameObject.CompareTag("Wall"))
            touchingWall = true;

        if (collision.gameObject.CompareTag("Iron"))
            touchingBlock = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SideWall") || collision.gameObject.CompareTag("Wall"))
            touchingWall = false;

        if (collision.gameObject.CompareTag("Iron"))
            touchingBlock = false;
    }

    void FixDirection()
    {
        float minY = 0.35f; // daha güçlü sınır

        if (Mathf.Abs(direction.y) < minY)
        {
            direction.y = direction.y < 0 ? -minY : minY;
        }

        direction = direction.normalized;
    }

    void SpawnSmallBall()
    {
        if (Time.time - lastSpawnTime < spawnCooldown) return; // cooldown kontrolü
        lastSpawnTime = Time.time;

        if (smallBallPrefab == null)
        {
            
            return;
        }

        GameObject newBall = Instantiate(smallBallPrefab, transform.position, Quaternion.identity);

        BallController smallController = newBall.GetComponent<BallController>();
        if (smallController != null)
        {
            smallController.SetAsSmallBall();
            smallController.attachedToPaddle = false;
            // small prefab'ın Rigidbody2D'si varsa dinamik yap
            Rigidbody2D rbSmall = newBall.GetComponent<Rigidbody2D>();
            if (rbSmall != null) rbSmall.bodyType = RigidbodyType2D.Dynamic;
            // NOT: GameManager kaydı için burada çağrı yoktur çünkü newBall.Start() bunu yapacaktır.
        }
    }

    public void SetAsSmallBall()
    {
        transform.localScale = Vector3.one * 0.5f;
        speed = speed * 1.2f; 
    }


    public void SetDirectionAndSpeed(Vector2 dir, float startSpeed)
    {
        direction = dir.normalized;
        speed = Mathf.Clamp(startSpeed, minSpeed, maxSpeed);
        attachedToPaddle = false;
        hasLaunched = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.linearVelocity = direction * speed;
      
    }

    public void SplitBall()
    {
        if (attachedToPaddle) return;

        Vector2 oppositeDir = -rb.linearVelocity.normalized;
        Vector3 spawnPos = transform.position + (Vector3)oppositeDir * 0.3f;

        GameObject newBall = Instantiate(gameObject, spawnPos, Quaternion.identity);
        BallController newBc = newBall.GetComponent<BallController>();
        newBc.SetDirectionAndSpeed(oppositeDir, speed);
    }

    // Top ölüm yönetimi (DeathZone tetiklediğinde çağır)
    public void Kill()
    {
        if (GameManager.instance != null)
            GameManager.instance.UnregisterBall();

        Destroy(gameObject);
    }

    // DeathZone için trigger örneği:
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("DeathZone") && !attachedToPaddle)
        {
            Kill();
        }
    }

}
