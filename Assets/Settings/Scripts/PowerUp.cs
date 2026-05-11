using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public char type;                // 'F','B','S','G','T','H','M' vb.
    public float fallSpeed = 2f;

    // --- EKLEDİĞİM ALAN: Meteor prefab (inspector'dan ata) ---
    public GameObject meteorBallPrefab;

    void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Paddle"))
        {
            // Paddle referansı (collision'dan alıyoruz, GameManager'a da bakabilirdik)
            var paddleObj = collision.gameObject;

            if (type == 'F')
            {
                GameManager.instance?.AddScore(1000);
            }
            else if (type == 'G')
            {
                // örnek: paddle.GetComponent<PaddleController>().EnableTurret(30f);
                GameManager.instance?.AddScore(100);
                var p = paddleObj.GetComponent<PaddleController>();
                if (p != null) p.EnableTurret(35f);
            }
            else if (type == 'B')
            {
                GameManager.instance?.AddScore(100);
                var p = paddleObj.GetComponent<PaddleController>();
                if (p != null) p.ApplyPowerUp('B');
            }
            else if (type == 'S')
            {
                GameManager.instance?.AddScore(100);
                var p = paddleObj.GetComponent<PaddleController>();
                if (p != null) p.ApplyPowerUp('S');
            }
            else if (type == 'H')
            {
                GameManager.instance?.AddScore(100);
                GameManager.instance?.AddLife(1);
            }
            else if (type == 'T')
            {
                GameManager.instance?.AddScore(100);
                var p = paddleObj.GetComponent<PaddleController>();
                if (p != null) p.SpawnTripleBalls();
            }
            else if (type == 'M') // <-- METEOR
            {
                GameManager.instance?.AddScore(100);
                SpawnMeteorAtPaddle(paddleObj.transform);
            }

            Destroy(gameObject);
        }
        else if (collision.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
        }
    }

    // Meteor spawn fonksiyonu: paddle transformunu alır ve üstünden spawn eder
    void SpawnMeteorAtPaddle(Transform paddleTransform)
    {
        if (meteorBallPrefab == null)
        {
            
            return;
        }

        Vector3 spawnPos = paddleTransform.position + Vector3.up * 0.6f;
        Instantiate(meteorBallPrefab, spawnPos, Quaternion.identity);
    }
}
