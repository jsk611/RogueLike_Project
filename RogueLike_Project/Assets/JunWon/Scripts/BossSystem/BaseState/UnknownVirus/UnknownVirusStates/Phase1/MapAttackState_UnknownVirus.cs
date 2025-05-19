using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttackState_UnknownVirus : BossPhaseBase<UnknownVirusBoss>
{
    private float stateTimer = 0f;
    private const float maxStateDuration = 12f;
    private bool isAttackFinished = false;

    public MapAttackState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        // 보스가 자신의 맵 공격 상태를 알게 함
        owner.SetMapAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Map Attack State 진입");
        stateTimer = 0f;
        isAttackFinished = false;

        // 이동 멈춤
        owner.NmAgent.isStopped = true;
        owner.Animator.SetBool("IsMoving", false);

        if (owner.AbilityManager.UseAbility("MapAttack"))
        {
            owner.TriggerMapAttack();

        }


        // 공격 애니메이션 & 효과
        owner.Animator.SetTrigger("MapAttack");
    }

    public override void Update()
    {
        stateTimer += Time.deltaTime;

        // 애니메이션 이벤트나 타이머로 완료 처리
        if (stateTimer >= maxStateDuration)
        {
        }
        // (추가) 애니메이션 이벤트에서 직접 호출해도 좋습니다
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Map Attack State 종료");
        // 이동 재개
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("MapAttack");
    }

    /// <summary>애니메이션 이벤트나 강제 타이머 종료 시 호출</summary>
    public void OnAttackFinished()
    {
        if (isAttackFinished) return;
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
