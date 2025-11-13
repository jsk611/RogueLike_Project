using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SniperMonster : RangedMonster
{
    private bool isFired = false;

    protected override void Start()
    {
        base.Start();
        isHitScan = true; // 스나이퍼는 즉시 타격 방식 사용
    }

    protected override void UpdateIdle()
    {
        nmAgent.isStopped = true; // 정지 상태
    }

    protected override void UpdateChase()
    {
        // 스나이퍼는 추적하지 않음
    }

    protected override void UpdateAttack()
    {
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.IDLE); // 공격 범위를 벗어나면 대기 상태로 전환
            ResetAttackTimer();
            return;
        }

        nmAgent.isStopped = true; // 이동 정지
        attackTimer += Time.deltaTime; // 공격 타이머 진행

        float aimTime = attackCooldown * 0.7f; // 공격 쿨타임의 70%를 조준 시간으로 사용
        float fireTime = attackCooldown * 0.8f; // 공격 쿨타임의 80%를 발사 시간으로 사용

        if (attackTimer <= aimTime)
        {
            HandleAim(); // 조준 처리
        }
        else if (attackTimer <= fireTime)
        {
            HandleFire(); // 발사 처리
        }

        if (attackTimer >= attackCooldown)
        {
            ResetAttackTimer(); // 쿨타임 완료 후 초기화
        }
    }

    public void HandleAim()
    {
        SetAnimatorState(State.AIM); // 조준 상태 애니메이션
        gun?.AimReady(); // 무기 조준 준비
    }

    private void HandleFire()
    {
        if (!isFired)
        {
            Debug.Log("FireTime: " + attackTimer);
            SetAnimatorState(State.ATTACK); // 발사 애니메이션
            isFired = true;
        }
    }

    private void ResetAttackTimer()
    {
        attackTimer = 0f;
        isFired = false;
    }

    protected override void CheckPlayer()
    {
        if (fov.VisibleTargets.Count > 0)
        {
            target = fov.VisibleTargets[0];
            ChangeState(State.ATTACK);
            // `State.CHASE`로 상태를 변경하지 않음
            Debug.Log($"{name} has detected a player, but won't chase.");
        }
    }

}
