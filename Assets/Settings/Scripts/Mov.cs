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
    private BoxCollider2D col;

    private int moveDirection = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

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

        // 👇 İKİ TARAF DA BOŞSA
        // Daha önce yön yoksa → varsayılan yönü ata
        if (moveDirection == 0)
        {
            moveDirection = Mathf.Clamp(defaultDirection, -1, 1);
        }
    }

    bool IsBlocked(Vector2 dir)
    {
        Vector2 origin = rb.position;
        float halfWidth = col.bounds.extents.x;

        Vector2 checkPos = origin + dir * (halfWidth + checkDistance);
        Vector2 boxSize = new Vector2(0.05f, col.bounds.size.y * 0.9f);

        Collider2D hit = Physics2D.OverlapBox(
            checkPos,
            boxSize,
            0f,
            blockLayer | wallLayer
        );

        return hit != null && hit.gameObject != gameObject;
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
        if (col == null) col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        Gizmos.color = Color.red;
        Vector2 size = new Vector2(0.05f, col.bounds.size.y * 0.9f);

        Gizmos.DrawWireCube(
            (Vector2)transform.position +
            Vector2.left * (col.bounds.extents.x + checkDistance),
            size
        );

        Gizmos.DrawWireCube(
            (Vector2)transform.position +
            Vector2.right * (col.bounds.extents.x + checkDistance),
            size
        );
    }
#endif
}
