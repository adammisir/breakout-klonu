using UnityEngine;

public class Mov : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool canMove = true;

    [Tooltip("-1 = sol, 1 = sağ")]
    [SerializeField] private int defaultDirection = 1;

    [Header("Detection Layers")]
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private float checkDistance = 0.1f;

    private Rigidbody2D rb;
    private Collider2D generalCollider;

    private int moveDirection = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Objede Box veya Polygon ne varsa onu genel collider olarak alıyoruz
        generalCollider = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation |
            RigidbodyConstraints2D.FreezePositionY;
    }

    void FixedUpdate()
    {
        if (!canMove)
            return;

        DecideDirection();
        Move();
    }

    void DecideDirection()
    {
        bool leftBlocked = IsBlocked(Vector2.left);
        bool rightBlocked = IsBlocked(Vector2.right);

        // İki taraf dolu → dur
        if (leftBlocked && rightBlocked)
        {
            moveDirection = 0;
            return;
        }

        // Sadece sol dolu → sağa
        if (leftBlocked && !rightBlocked)
        {
            moveDirection = 1;
            return;
        }

        // Sadece sağ dolu → sola
        if (!leftBlocked && rightBlocked)
        {
            moveDirection = -1;
            return;
        }

        //  İKİ TARAF DA BOŞSA
        // Daha önce yön yoksa → varsayılan yönü ata
        if (moveDirection == 0)
        {
            moveDirection = Mathf.Clamp(defaultDirection, -1, 1);
        }
    }

    bool IsBlocked(Vector2 dir)
    {
        if (generalCollider == null)
            generalCollider = GetComponent<Collider2D>();

        if (generalCollider == null)
            return false;

        // L blokları (PolygonCollider2D)
        if (generalCollider is PolygonCollider2D)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(blockLayer | wallLayer);
            filter.useTriggers = false;

            RaycastHit2D[] results = new RaycastHit2D[5];

            int hitCount = generalCollider.Cast(dir, filter, results, checkDistance);

            for (int i = 0; i < hitCount; i++)
            {
                if (results[i].collider != null &&
                    results[i].collider.gameObject != gameObject)
                {
                    return true;
                }
            }

            return false;
        }

        // Normal bloklar (BoxCollider2D)
        Vector2 size = generalCollider.bounds.size;
        size.y *= 0.9f;

        Vector2 colliderCenter = generalCollider.bounds.center;

        RaycastHit2D hit = Physics2D.BoxCast(
            colliderCenter,
            size,
            0f,
            dir,
            checkDistance,
            blockLayer | wallLayer
        );

        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    void Move()
    {
        if (moveDirection == 0)
            return;

        Vector2 nextPos =
            rb.position +
            Vector2.right * moveDirection * speed * Time.fixedDeltaTime;

        rb.MovePosition(nextPos);
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (generalCollider == null) generalCollider = GetComponent<Collider2D>();
        if (generalCollider == null) return;

        Gizmos.color = Color.red;
        Vector2 size = generalCollider.bounds.size;
        size.y *= 0.9f;

        Vector2 center = generalCollider.bounds.center;

        // Sol kontrol kutusu çizimi (Tam olması gereken yerde görünecek)
        Gizmos.DrawWireCube(center + Vector2.left * checkDistance, size);

        // Sağ kontrol kutusu çizimi
        Gizmos.DrawWireCube(center + Vector2.right * checkDistance, size);
    }
    #endif
}
