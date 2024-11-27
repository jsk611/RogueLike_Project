using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("settings")]
    [SerializeField] float firerate = 1.5f;
    [SerializeField] protected bool isHitScan = false;

    public EnemyWeapon gun;
    
   

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;
        nmAgent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK);
        }

    }

    protected override void UpdateAttack()
    {
        // 타겟이 없거나 공격 범위를 벗어난 경우 상태 전환
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE); // 추적 상태로 전환
            attackTimer = 0f;         // 타이머 초기화
            return;
        }

        nmAgent.isStopped = true; // 이동 정지

        // 공격 타이머 진행
        attackTimer += Time.deltaTime;

        // 조준 시간 설정 (공격 간격의 일부를 조준 시간으로 사용)
        float aimTime = attackCooldown * 0.6f; // 쿨타임의 30%를 조준 시간으로 사용
        float attackTime = attackCooldown * 0.8f;
        if (attackTimer <= aimTime)
        {
            // 조준 동작
            SetAnimatorState(State.ATTACK); // ATTACK 상태에서 조준 애니메이션 실행
            return; // 아직 발사하지 않음
        }
        else if (attackTimer <= attackTime)
        {
            SetAnimatorState(State.COOLDOWN);
        }
        else
        {
            SetAnimatorState(State.AIM);
        }

        // 공격 쿨타임 완료 후 초기화
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f; // 타이머 초기화
        }
    }

    public void FireEvent()
    {
        if (gun == null)
        {
            return;
        }

        if (!isHitScan)
        {
            gun.Fire(); // 총 발사
            Debug.Log("Gun fired via Animation Event!");
        }
        else
        {
            gun.FireLaser();
            Debug.Log("Hit scan Activated");
        }

        
    }
}
