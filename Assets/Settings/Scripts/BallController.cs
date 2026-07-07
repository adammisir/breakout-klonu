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

    public AudioClip paddleSound;
    public AudioClip blockSound;
    public AudioClip ironBlockSound;
    public AudioClip WallSouns;

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
        rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;

        
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

        if (collision.gameObject.CompareTag("Wall") || (collision.gameObject.CompareTag("SideWall")))
        {
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
