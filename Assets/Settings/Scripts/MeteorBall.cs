using UnityEngine;

public class MeteorBall : MonoBehaviour
{
    public float lifetime = 10f;
    public float speed = 14f;
    public GameObject normalBallPrefab;

    private Rigidbody2D rb;
    private bool transforming = false;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Vector2 dir = new Vector2(Random.Range(-0.6f, 0.6f), 1f).normalized;
        rb.linearVelocity = dir * speed;

        Invoke(nameof(TransformToNormalBall), lifetime);

        GameManager.instance.activeBalls++;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Her türlü blok yok edilir — indestructible fark etmez
        if (col.CompareTag("Block"))
        {
            //Destroy(col.gameObject);
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

        float spawnOffset = 0.25f;

        float currentSpeed = rb.linearVelocity.magnitude;

        Vector2[] directions =
        {
        new Vector2(-0.6f, 1f).normalized, // ↖
        Vector2.up,                         // ↑
        new Vector2(0.6f, 1f).normalized    // ↗
        };



        for (int i = 0; i < 3; i++)
        {
            float xOffset = (i - 1) * spawnOffset;
            Vector3 spawnPos = center + new Vector3(xOffset, 0f, 0f);

            GameObject newBall = Instantiate(normalBallPrefab, spawnPos, Quaternion.identity);
            BallController bc = newBall.GetComponent<BallController>();

            bc.SetDirectionAndSpeed(directions[i], currentSpeed);
        }




        Destroy(gameObject);
    }

}
