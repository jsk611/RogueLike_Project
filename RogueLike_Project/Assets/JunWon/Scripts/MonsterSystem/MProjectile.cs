using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MProjectile : MonoBehaviour
{
    
    private float damage; // ����ü�� ���ط�
   
    [Header("Settings")]
    [SerializeField] float lifetime = 5f; // ����ü�� ����
    [SerializeField] float speed = 0.05f; // ����ü�� �ӵ�

    void Start()
    {
        Destroy(gameObject, lifetime); // ���� �ð� �� ����ü �ı�
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

            Destroy(gameObject);
        }
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
