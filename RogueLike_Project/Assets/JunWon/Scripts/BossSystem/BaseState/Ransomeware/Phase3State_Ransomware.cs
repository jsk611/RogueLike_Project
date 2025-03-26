using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase3State_Ransomware : BossPhaseBase<Ransomware>
{
    private Phase2_DigitalShadow_State digitalShadowState;

    public Phase3State_Ransomware(Ransomware owner) : base(owner)
    {
        digitalShadowState = new Phase2_DigitalShadow_State(owner);
    }

    public override void Enter()
    {
        Debug.Log("랜섬웨어 보스 페이즈3 (발악 패턴) 시작");

        // 페이즈3 애니메이션 레이어 활성화 (필요한 경우)
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase2"), 1);
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase1"), 1);

        // 어빌리티 초기화
        InitializeAbility();

        // 발악 패턴 시작 - 즉시 분열 실행
        StartLastStandPattern();
    }

    private void InitializeAbility()
    {
        // 발악 패턴에서 사용할 능력 활성화
        owner.AbilityManager.SetAbilityActive("SummonShadow");

        // 다른 모든 능력 비활성화
        owner.AbilityManager.SetAbilityInactive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityInactive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityInactive("DataBlink");
        owner.AbilityManager.SetAbilityInactive("Lock");
    }

    private void StartLastStandPattern()
    {
        Debug.Log("랜섬웨어 보스 발악 패턴 (디지털 섀도우 분열) 시작");

        // 보스 이동 중지
        owner.NmAgent.isStopped = true;

        // 특수 효과/카메라 연출 등 추가 가능

        // 분열 상태 실행
        digitalShadowState.Enter();
        digitalShadowState.ActivateLastStandSplit();
    }

    public override void Update()
    {
        // 발악 패턴에서는 디지털 섀도우 상태만 업데이트
        digitalShadowState.Update();
    }

    public override void Exit()
    {
        // 발악 패턴 종료 - 디지털 섀도우 상태 종료
        digitalShadowState.Exit();
    }

    public override void Interrupt()
    {
        if (isInterrupted) return;
        isInterrupted = true;

        // 디지털 섀도우 상태 중단
        digitalShadowState.Interrupt();

        // 페이즈3 관련 정리 작업
        owner.AbilityManager.SetAbilityInactive("SummonShadow");

        // 보스 이동 중지 및 회전 잠금
        owner.NmAgent.isStopped = true;
        owner.SetRotationLock(true);
    }
}
