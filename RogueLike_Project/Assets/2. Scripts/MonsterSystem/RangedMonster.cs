using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("Settings")]
    [SerializeField] private float fireRate = 1.5f; // 발사 속도
    [SerializeField] protected bool isHitScan = false; // 히트 스캔 여부

    [SerializeField] protected float aimTime; // 조준 시간: 쿨타임의 60%
    [SerializeField] protected float attackTime;
    public EnemyWeapon gun;

    protected override void Start()
    {
        base.Start();
        aimTime = attackCooldown * 0.6f;
        attackTime = attackCooldown * 0.8f;
    }
    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE); // 타겟이 없으면 대기 상태로 전환
            return;
        }

        nmAgent.isStopped = false; // 이동 활성화
        nmAgent.speed = chaseSpeed;
        nmAgent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK); // 공격 범위 안에 들어오면 공격 상태로 전환
        }
    }

    protected override void UpdateAttack()
    {
        if (!IsTargetValid())
        {
            ChangeState(State.CHASE); // 타겟이 없거나 범위를 벗어나면 추적 상태로 전환
            ResetAttackTimer();
            return;
        }

        nmAgent.isStopped = true; // 이동 정지
        attackTimer += Time.deltaTime; // 공격 타이머 진행

        HandleAttackPhases();

        if (attackTimer >= attackCooldown)
        {
            ResetAttackTimer(); // 쿨타임 완료 후 타이머 초기화
        }
    }

    private void HandleAttackPhases()
    {
        float aimTime = attackCooldown * 0.6f; // 조준 시간: 쿨타임의 60%
        float fireTime = attackCooldown * 0.8f; // 발사 시간: 쿨타임의 80%

        if (attackTimer <= aimTime)
        {
            HandleAim(); // 조준 처리
        }
        else if (attackTimer <= fireTime)
        {
            HandleFirePreparation(); // 발사 준비
        }
        else
        {
            HandleFire(); // 실제 발사
        }
    }

    private void HandleAim()
    {
        SetAnimatorState(State.AIM); // AIM 상태 애니메이션
    }

    private void HandleFirePreparation()
    {
        SetAnimatorState(State.COOLDOWN); // 발사 준비 애니메이션
    }

    private void HandleFire()
    {
        SetAnimatorState(State.ATTACK); // ATTACK 상태 애니메이션
    }

    public virtual void FireEvent()
    {
        if (gun == null) return; // 무기가 없으면 실행하지 않음

        if (!isHitScan)
        {
            gun.Fire(); // 총알 발사
            Debug.Log("Gun fired via Animation Event!");
        }
        else
        {
            gun.FireLaser(); // 히트 스캔 방식으로 발사
            Debug.Log("Hit scan Activated");
        }
    }

    private bool IsTargetValid()
    {
        return target != null && Vector3.Distance(transform.position, target.position) <= attackRange && DetectedPlayer();
    }

    private void ResetAttackTimer()
    {
        attackTimer = 0f;
    }
}
