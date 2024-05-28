using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Settings")]
    public int damage = 20; // 투사체의 피해량
    public float lifetime = 5f; // 투사체의 수명

    void Start()
    {
        Destroy(gameObject, lifetime); // 일정 시간 후 투사체 파괴
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
           /* Player playerHealth = collision.gameObject.GetComponent<Player>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }*/
        }
        Destroy(gameObject); // 충돌 시 투사체 파괴
    }
}
