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
        VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
        isAttackFinished = false;

        // 이동 멈춤
        owner.NmAgent.isStopped = true;
        owner.Animator.SetBool("IsMoving", false);

        owner.StartCoroutine(ExecuteSequentialAttack());


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

    private IEnumerator ExecuteSequentialAttack()
    {
        if (owner.basic != null)
        {
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx == null)
                vfx = owner.basic.AddComponent<VirusCubeAttackEffect>();

            // 1. 바이러스 큐브 레이저 연출 시작
            vfx.StartLaserAttack();

            // 2. 연출 완료까지 대기 (3.6초)
            yield return new WaitForSeconds(3.2f);

            // 3. 맵 공격 실행
            if (owner.AbilityManager.UseAbility("MapAttack"))
            {
                owner.TriggerMapAttack();
            }
        }

        // 4. 공격 완료
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
