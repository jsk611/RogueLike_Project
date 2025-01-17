using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MProjectile : MonoBehaviour
{
    
    private float damage; // 투사체의 피해량
   
    [Header("Settings")]
    [SerializeField] float lifetime = 5f; // 투사체의 수명
    [SerializeField] float speed = 0.05f; // 투사체의 속도

    void Start()
    {
        Destroy(gameObject, lifetime); // 일정 시간 후 투사체 파괴
    }

    void Update()
    {
        UpdateBullet();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player takes damage " + damage);
            PlayerStatus playerHealth = other.GetComponent<PlayerStatus>();
            if (playerHealth != null)
            {
                playerHealth.DecreaseHealth(damage);
            }

            
        }
        Destroy(gameObject);
    }

    void UpdateBullet()
    {
        transform.Translate(Vector3.forward * speed);
    }

    public void SetBulletDamage(float attackDamage)
    {
        damage = attackDamage;
        Debug.Log("Bullet damage : "+ damage);
    }
}
