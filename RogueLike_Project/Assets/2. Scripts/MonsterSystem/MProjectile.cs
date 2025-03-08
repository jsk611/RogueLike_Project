using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MProjectile : MonoBehaviour
{
    
    protected float damage; // 투사체의 피해량
   
    [Header("Settings")]
    [SerializeField] float lifetime = 20f; // 투사체의 수명
    [SerializeField] float speed = 0.05f; // 투사체의 속도
    [SerializeField] Vector3 dir = Vector3.forward;

    void Start()
    {
        SetDirection(transform.forward);
        Destroy(gameObject, lifetime); // 일정 시간 후 투사체 파괴
    }

    void Update()
    {
        Move();
    }

    void OnTriggerEnter(Collider other)
    {
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
        else if (other.CompareTag("Floor"))
        {
            Destroy(gameObject);
        }
    }

    private void Move()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    public void SetBulletDamage(float attackDamage)
    {
        damage = attackDamage;
        Debug.Log("Bullet damage : "+ damage);
    }

    public void SetDirection(Vector3 dir)
    {
        this.dir = dir; 
    }
}
