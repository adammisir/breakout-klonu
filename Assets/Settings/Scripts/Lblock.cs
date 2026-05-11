using UnityEngine;

public class LBlock : MonoBehaviour
{
   
    


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
        if (!hitter.CompareTag("Ball") && !hitter.CompareTag("Bullet"))
            return;

  

        Destroy(gameObject);

        
    }

    
}
