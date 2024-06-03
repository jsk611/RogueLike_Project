using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeeleMonster : MonoBehaviour
{
    public float detectionRadius = 10f; // 몬스터가 플레이어를 감지하는 거리
    public float attackRange = 2f; // 공격 거리
    public float attackCooldown = 1.5f; // 공격 간격
    public int damage = 10; // 공격력

    private Transform player;
    private PlayerControl playerData;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private float lastAttackTime;

    public enum State
    {
        None,
        Idle,
        Run,
        Attack,
    }

    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerData = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {

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
            // navMeshAgent.SetDestination(transform.position); // 몬스터를 현재 위치에 멈추게 함
        }
    }

    void Attack()
    {
        // 공격 애니메이션 트리거
        animator.SetTrigger("Attack");

        // 실제 공격 로직은 애니메이션 이벤트나 충돌 검사를 통해 구현
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            playerData.TakeDamage(damage);
            // 플레이어에게 피해를 줌
        }
    }

    void Chase()
    {

    }
}
