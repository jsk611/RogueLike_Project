using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedMonster : MonoBehaviour
{

    [Header("Preset Fields")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject splashFx;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Settings")]
    public float detectionRadius = 15f; 
    public float attackRange = 10f; 
    public float attackCooldown = 2f;
    public GameObject projectilePrefab; 
    public Transform firePoint;
    public float projectileSpeed = 20f; 

    private Transform player;
    private float lastAttackTime;

    public enum State
    {
        None,
        Idle,
        Attack
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

   

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            navMeshAgent.SetDestination(player.position);

            if (distanceToPlayer <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            navMeshAgent.SetDestination(transform.position); 
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = (player.position - firePoint.position).normalized * projectileSpeed;
    }
}
