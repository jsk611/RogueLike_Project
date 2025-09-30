using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MProjectile : MonoBehaviour
{
    protected float damage;

    [Header("Settings")]
    [SerializeField] private float lifetime = 20f;
    [SerializeField] protected float speed = 0.05f;
    [SerializeField] protected Vector3 dir = Vector3.forward;

    void Start()
    {
        SetDirection(transform.forward);
        Destroy(gameObject, lifetime);
    }

    protected void Update()
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
        Debug.Log("Bullet damage : " + damage);
    }

    public void SetDirection(Vector3 direction)
    {
        dir = direction;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }

    public float GetSpeed()
    {
        return speed;
    }
}
