using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;

    private int blockHitCount = 0;

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Block") || (collision.collider.CompareTag("Damm")))
        {
            blockHitCount++;
            GameManager.instance?.AddScore(100);


            //Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        if (collision.collider.CompareTag("Iron"))
        {
            Destroy(gameObject);
        }

    }
}
