using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttackState_UnknownVirus : BossPhaseBase<UnknownVirusBoss>
{
    private bool isAttackFinished = false;
    private Coroutine attackCoroutine; // 코루틴 참조 변수

    public MapAttackState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        // 생성자 자신을 맵 공격 상태로 알려 줌
        owner.SetMapAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Map Attack State 진입");
        VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
        if (vfx == null) Debug.Log("Can't find vfx");
        isAttackFinished = false;

        // 이동 정지
        owner.NmAgent.isStopped = true;
        owner.Animator.SetBool("IsMoving", false);

        attackCoroutine = owner.StartCoroutine(ExecuteSequentialAttack());

        // 공격 애니메이션 & 효과
        owner.Animator.SetTrigger("MapAttack");
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Map Attack State Exit");

        // 진행 중인 공격 코루틴 중단
        if (attackCoroutine != null)
        {
            owner.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // 큐브 공격 효과 중단
        StopCubeAttackEffect();

        owner.NmAgent.isStopped = false;

        owner.Animator.ResetTrigger("MapAttack");
    }

    public override void Interrupt()
    {
        base.Interrupt();

        Debug.Log("MapAttackState: 강제 중단됨");

        // 진행 중인 공격 모든 중단
        if (attackCoroutine != null)
        {
            owner.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // 큐브 공격 효과 모두 중단
        StopCubeAttackEffect();

        // 공격 완료로 표시하여 상태 전환 허용
        isAttackFinished = true;
    }

    /// <summary>
    /// 큐브 공격 효과 중단
    /// </summary>
    private void StopCubeAttackEffect()
    {
        if (owner.basic != null)
        {
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx != null)
            {
                vfx.StopEffect(); // 큐브 효과 중단
            }
        }
    }
    /// <summary>애니메이션 이벤트를 통해 타이밍 맞춰 호출</summary>
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

            // 1. 레이저 큐브 공격을 먼저 시작
            vfx.StartLaserAttack();

            // 2. 공격 완료까지 대기 (3.6초)
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
