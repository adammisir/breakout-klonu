using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Settings")]
    public bool isIndestructible = false;

    [Header("Power Up Settings")]
    public GameObject powerUpF;
    public GameObject powerUpB;
    public GameObject powerUpS;
    public GameObject powerUpG;
    public GameObject powerUpT;
    public GameObject powerUpH;
    public GameObject powerUpM;

    [Range(0f, 1f)]
    public float dropChance = 0.2f; // %20 ihtimalle powerup düþer

    // Eðer collider'lar trigger ise burasý çalýþýr
    void OnTriggerEnter2D(Collider2D col)
    {
        HandleHit(col.gameObject);
    }

    // Eðer collider'lar normal collision ise burasý çalýþýr
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject hitter)
    {
        // Sadece Ball veya Bullet ile tepki ver
        if (!hitter.CompareTag("Ball") && !hitter.CompareTag("Bullet") && !hitter.CompareTag("Meteor"))
            return;

        if (isIndestructible) return;


        

        DropPowerUp();
        
        Destroy(gameObject);

    }

    void DropPowerUp()
    {
        if (Random.value > dropChance)
            return;

        GameObject[] list = { powerUpF, powerUpB, powerUpS, powerUpG, powerUpT, powerUpH, powerUpM };
        GameObject chosen = list[Random.Range(0, list.Length)];

        Instantiate(chosen, transform.position, Quaternion.identity);
    }
}
