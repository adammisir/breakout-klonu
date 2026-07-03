using System.Collections;
using UnityEngine;
public class PaddleController : MonoBehaviour
{
    public float speed = 10f;

    // Yeni: Raketin hareket edeceði sýnýrlar
    public float minX;
    public float maxX;

    public enum PaddleSize { Normal, Big, Small, Gun }
    public PaddleSize currentSize = PaddleSize.Normal;

    [Header("Scales")]
    public Vector3 normalScale = new Vector3(1f, 1f, 1f);
    public Vector3 bigScale = new Vector3(1.9f, 1f, 1f);
    public Vector3 smallScale = new Vector3(0.5f, 1f, 1f);


    public bool isTurret = false;
    public float fireRate = 0.25f;
    private float fireTimer = 0f;

    [Header("Turret")]
    public Sprite normalSprite;
    public Sprite turretSprite;

    public GameObject bulletPrefab;
    public Transform leftGun;
    public Transform rightGun;

    private Coroutine sizeCoroutine = null;
    private SpriteRenderer sr;


    public GameObject smallBallPrefab; // inspector'dan atayacaksýn
    public float smallBallSpeed = 6f;
    public int tripleCount = 3;
    public float spreadAngle = 35f; // her top arasýndaki açý farký

    public Vector3 defaultScale = new Vector3(1.5f, 0.3f, 1f);
    public bool isBig = false;
    public bool isSmall = false;



    private Rigidbody2D rb;

    void Awake()
    {
        // Eðer inspector'da normalScale boþ býrakýlmýþsa, baþlangýç deðerini kaydet
        if (normalScale == Vector3.zero)
            normalScale = transform.localScale;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // rb = GetComponent<Rigidbody2D>(); yerine rb = GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        ApplyScale();

        CalculateBounds();
    }

    void CalculateBounds()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("SideWall");

        if (walls.Length == 0)
        {
           
            return;
        }

        float? leftWallInnerEdge = null;
        float? rightWallInnerEdge = null;

        Vector3 paddlePos = transform.position;

        foreach (GameObject wall in walls)
        {
            Collider2D col = wall.GetComponent<Collider2D>();
            if (col == null) continue;

            Bounds b = col.bounds;

            if (b.center.x < paddlePos.x)
            {
                // Bu duvar solda -> iç kenarý (sað tarafý) bizim minX'imiz olacak
                float innerEdge = b.max.x;
                if (leftWallInnerEdge == null || innerEdge > leftWallInnerEdge)
                    leftWallInnerEdge = innerEdge;
            }
            else
            {
                // Bu duvar saðda -> iç kenarý (sol tarafý) bizim maxX'imiz olacak
                float innerEdge = b.min.x;
                if (rightWallInnerEdge == null || innerEdge < rightWallInnerEdge)
                    rightWallInnerEdge = innerEdge;
            }
        }

        if (leftWallInnerEdge.HasValue)
            minX = leftWallInnerEdge.Value;

        if (rightWallInnerEdge.HasValue)
            maxX = rightWallInnerEdge.Value;
    }

    public void ResetSize()
    {
        currentSize = PaddleSize.Normal;   // asýl sistemde normal moda al
        isBig = false;
        isSmall = false;

        ApplyScale(); // doðru scale’i uygular

        // turret varsa kapat
        if (isTurret)
        {
            isTurret = false;
            GetComponent<SpriteRenderer>().sprite = normalSprite;
        }
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Horizontal");

        // 1. Hýzý ayarla (Mevcut hareket kodumuz)
        // Yeni Unity sürümü için linearVelocity kullandýðýnýzý varsayýyorum:
        rb.linearVelocity = new Vector2(moveInput * speed, 0f);

        // 2. Yeni: Pozisyonu Sýnýrlandýrma
        Vector3 currentPosition = transform.position;

        float halfWidth = 0f;
        if (sr != null && sr.sprite != null)
            halfWidth = sr.bounds.extents.x;  // sprite'ýn gerçek dünya geniþliði
        else
            halfWidth = transform.localScale.x * 0.5f;  // fallback

        // Sýnýrlarý yarý geniþliðe göre daralt
        float clampedX = Mathf.Clamp(currentPosition.x, minX + halfWidth, maxX - halfWidth);


        // Raketin pozisyonunu güncellenmiþ X deðeriyle ayarla (Y ve Z deðiþmez).
        transform.position = new Vector3(clampedX, currentPosition.y, currentPosition.z);
    }



    private void Update()
    {
        if (isTurret)
        {
            fireTimer -= Time.deltaTime;

            if ((Input.GetKeyDown(KeyCode.G) || (Input.GetKey(KeyCode.LeftShift)) && fireTimer <= 0f))
            {
                FireGuns();
                fireTimer = fireRate;
            }
        }

    }

    void FireGuns()
    {
        Instantiate(bulletPrefab, leftGun.position, Quaternion.identity);
        Instantiate(bulletPrefab, rightGun.position, Quaternion.identity);
    }


    // Power-up çarptýðýnda bu metod çaðrýlacak
    public void ApplyPowerUp(char type)
    {
        if (type == 'B')
        {
            if (currentSize == PaddleSize.Normal)
                currentSize = PaddleSize.Big;

            else if (currentSize == PaddleSize.Small)
                currentSize = PaddleSize.Normal;


            if (isTurret)
            {
                DisableTurret();
            }

        }
        else if (type == 'S')
        {
            if (currentSize == PaddleSize.Normal)
                currentSize = PaddleSize.Small;

            else if (currentSize == PaddleSize.Big)
                currentSize = PaddleSize.Normal;

            if (isTurret)
            {
                DisableTurret();
            }

        }

        ApplyScale();
    }

    void ApplyScale()
    {
        if (currentSize == PaddleSize.Normal)
            transform.localScale = normalScale;

        else if (currentSize == PaddleSize.Big)
            transform.localScale = bigScale;

        else if (currentSize == PaddleSize.Small)
            transform.localScale = smallScale;
    }

    private Coroutine turretRoutine;

    public void EnableTurret(float duration)
    {
        if (currentSize != PaddleSize.Normal)
        {
            currentSize = PaddleSize.Normal;
            ApplyScale();
        }


        isTurret = true;
        GetComponent<SpriteRenderer>().sprite = turretSprite;

        if (turretRoutine != null)
            StopCoroutine(turretRoutine);

        turretRoutine = StartCoroutine(TurretTimer(duration));
    }

    IEnumerator TurretTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        DisableTurret();
    }

    public void DisableTurret()
    {
        isTurret = false;
        GetComponent<SpriteRenderer>().sprite = normalSprite;
    }

    public void SpawnTripleBalls()
    {
        if (smallBallPrefab == null)
        {
            
            return;
        }

        Vector3 basePos = transform.position + Vector3.up * 0.4f;

        float[] angles = { -6f, 0f, 6f };
        float offset = 0.25f; // toplar birbirine girmesin diye

        for (int i = 0; i < angles.Length; i++)
        {
            // Her topu biraz sað/sol kaydýrýyoruz
            Vector3 spawnPos = basePos + new Vector3((i - 1) * offset, 0, 0);

            GameObject newBall = Instantiate(
                smallBallPrefab,
                spawnPos,
                Quaternion.identity
            );

            BallController bc = newBall.GetComponent<BallController>();
            if (bc != null)
            {
                bc.attachedToPaddle = false;  // küçük toplar paddle’a baðlý baþlamasýn
            }


            
            Rigidbody2D r = newBall.GetComponent<Rigidbody2D>();

            if (r != null)
            {
                r.bodyType = RigidbodyType2D.Dynamic;

                float rad = angles[i] * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;

                r.linearVelocity = dir * smallBallSpeed;
            }

           
        }
    }
   

}