using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttackState_UnknownVirus : BossPhaseBase<UnknownVirusBoss>
{
    private bool isAttackFinished = false;

    public MapAttackState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        // 보스가 자신의 맵 공격 상태를 알게 함
        owner.SetMapAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Map Attack State 진입");
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
