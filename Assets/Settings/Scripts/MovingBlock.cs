using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingBlock : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private bool canMove = false;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Sınır Ayarları")]
    [SerializeField] private float leftBoundary = -8f;
    [SerializeField] private float rightBoundary = 8f;

    private int moveDirection = 1;
    private bool isBlockedLeft = false;
    private bool isBlockedRight = false;
    private BoxCollider2D blockCollider;
    private Rigidbody2D rb;

    [Header("Katman Ayarları")]
    [SerializeField] private LayerMask blockLayerMask;
    [SerializeField] private LayerMask wallLayerMask;

    // Çarpışma önleme için
    private List<Collider2D> ignoredColliders = new List<Collider2D>();

    void Start()
    {
        blockCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

        if (canMove)
        {
            moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        }
        else
        {
            moveDirection = 0;
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            moveDirection = 0;
            return;
        }

        CheckSurroundings();
        MoveBlock();
    }

    void CheckSurroundings()
    {
        float blockWidth = blockCollider.bounds.size.x;
        float blockHeight = blockCollider.bounds.size.y;

        // Box overlap kullanarak daha güvenilir algılama
        Vector2 boxSize = new Vector2(0.05f, blockHeight * 0.9f);

        // Sol taraf kontrolü - bloğun DIŞINDAN başlat
        Vector2 leftCheckPos = (Vector2)transform.position + Vector2.left * (blockWidth / 2 + 0.1f);
        Collider2D leftHit = Physics2D.OverlapBox(leftCheckPos, boxSize, 0f, blockLayerMask | wallLayerMask);
        isBlockedLeft = (leftHit != null && leftHit.gameObject != gameObject);

        // Sağ taraf kontrolü - bloğun DIŞINDAN başlat
        Vector2 rightCheckPos = (Vector2)transform.position + Vector2.right * (blockWidth / 2 + 0.1f);
        Collider2D rightHit = Physics2D.OverlapBox(rightCheckPos, boxSize, 0f, blockLayerMask | wallLayerMask);
        isBlockedRight = (rightHit != null && rightHit.gameObject != gameObject);

        // Sol sınır kontrolü
        if (transform.position.x - blockWidth / 2 <= leftBoundary)
        {
            isBlockedLeft = true;
            if (moveDirection == -1)
            {
                moveDirection = 1;
            }
        }

        // Sağ sınır kontrolü
        if (transform.position.x + blockWidth / 2 >= rightBoundary)
        {
            isBlockedRight = true;
            if (moveDirection == 1)
            {
                moveDirection = -1;
            }
        }

        // Her iki taraf da bloklanmışsa dur
        if (isBlockedLeft && isBlockedRight)
        {
            moveDirection = 0;
        }
        // Sadece hareket yönü bloklanmışsa yön değiştir
        else if (moveDirection == -1 && isBlockedLeft)
        {
            moveDirection = isBlockedRight ? 0 : 1;
        }
        else if (moveDirection == 1 && isBlockedRight)
        {
            moveDirection = isBlockedLeft ? 0 : -1;
        }
    }

    void MoveBlock()
    {
        if (moveDirection != 0)
        {
            // Hareket etmeden önce bir sonraki pozisyonu kontrol et
            Vector2 nextPosition = rb.position + Vector2.right * moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Çarpışma kontrolü
            Vector2 boxSize = new Vector2(blockCollider.bounds.size.x * 0.95f, blockCollider.bounds.size.y * 0.95f);
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(nextPosition, boxSize, 0f, blockLayerMask);

            bool canMoveToPosition = true;
            foreach (var overlap in overlaps)
            {
                if (overlap.gameObject != gameObject)
                {
                    canMoveToPosition = false;
                    break;
                }
            }

            if (canMoveToPosition)
            {
                rb.MovePosition(nextPosition);
            }
            else
            {
                // Çarpışma var, yön değiştir
                moveDirection *= -1;
            }
        }
    }

    // Çarpışma algılama - hareketsiz bloklara çarpma kontrolü
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Eğer hareketsiz bir bloğa çarptıysak
        MovingBlock otherBlock = collision.gameObject.GetComponent<MovingBlock>();
        if (otherBlock != null && !otherBlock.canMove)
        {
            // Sadece yön değiştir, yok etme!
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.x > 0.5f)
            {
                moveDirection = 1;
            }
            else if (contact.normal.x < -0.5f)
            {
                moveDirection = -1;
            }
        }
    }

    void OnDestroy()
    {
        if (!Application.isPlaying) return;

        // Yakındaki blokları haberdar et
        Collider2D[] nearbyBlocks = Physics2D.OverlapCircleAll(transform.position, 2f, blockLayerMask);
        foreach (var block in nearbyBlocks)
        {
            if (block != null)
            {
                MovingBlock movingBlock = block.GetComponent<MovingBlock>();
                if (movingBlock != null && movingBlock.canMove)
                {
                    movingBlock.ReCheckMovement();
                }
            }
        }
    }

    public void ReCheckMovement()
    {
        if (!canMove) return;
        StartCoroutine(DelayedRecheck());
    }

    IEnumerator DelayedRecheck()
    {
        yield return new WaitForSeconds(0.1f);

        CheckSurroundings();

        if (moveDirection == 0)
        {
            if (!isBlockedLeft && !isBlockedRight)
            {
                moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;
            }
            else if (!isBlockedLeft)
            {
                moveDirection = -1;
            }
            else if (!isBlockedRight)
            {
                moveDirection = 1;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        float blockWidth = col.bounds.size.x;
        float blockHeight = col.bounds.size.y;
        Vector2 boxSize = new Vector2(0.05f, blockHeight * 0.9f);

        // Sol kontrol alanını göster
        Gizmos.color = isBlockedLeft ? Color.red : Color.green;
        Vector2 leftCheckPos = (Vector2)transform.position + Vector2.left * (blockWidth / 2 + 0.1f);
        Gizmos.DrawWireCube(leftCheckPos, boxSize);

        // Sağ kontrol alanını göster
        Gizmos.color = isBlockedRight ? Color.red : Color.green;
        Vector2 rightCheckPos = (Vector2)transform.position + Vector2.right * (blockWidth / 2 + 0.1f);
        Gizmos.DrawWireCube(rightCheckPos, boxSize);

        // Hareket yönünü göster
        if (canMove && moveDirection != 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 dir = moveDirection == 1 ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(transform.position, dir * 0.5f);
        }
    }
}