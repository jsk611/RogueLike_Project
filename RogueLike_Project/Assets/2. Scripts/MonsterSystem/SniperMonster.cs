using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SniperMonster : RangedMonster
{

    protected override void UpdateAttack()
    {
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.IDLE); // 추적 상태로 전환
            isFired = false;          // 발사 상태 초기화
            attackTimer = 0f;         // 타이머 초기화
            return;
        }

        nmAgent.isStopped = true; // 이동 정지

        // 공격 타이머 진행
        attackTimer += Time.deltaTime;

        // 조준 시간 설정 (공격 간격의 일부를 조준 시간으로 사용)
        float aimTime = attackCooldown * 0.7f; // 쿨타임의 30%를 조준 시간으로 사용
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
            isFired = false;  // 발사 상태 초기화
        }

    }

    protected override void CheckPlayer()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            if (state != State.ATTACK || state != State.HIT) ChangeState(State.ATTACK);
        }
    }

}
