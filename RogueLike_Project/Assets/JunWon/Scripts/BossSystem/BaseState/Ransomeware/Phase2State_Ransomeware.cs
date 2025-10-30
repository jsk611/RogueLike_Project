using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2State_Ransomeware : BossPhaseBase<Ransomware>
{
    public Phase2State_Ransomeware(Ransomware owner) : base(owner)
    { }

    private StateMachine<Ransomware> subFsm;

    private void InitializeAbility()
    {
        owner.AbilityManager.SetAbilityActive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityActive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityActive("DataBlink");
        owner.AbilityManager.SetMaxCoolTime("DataBlink");
        owner.AbilityManager.SetAbilityActive("Lock");
        owner.AbilityManager.SetMaxCoolTime("Lock");
    }
    private void InitializeStats()
    {
    }
    private void InitializeSubFSM()
    {
        var idleState = new Phase1_Idle_State(owner);
        var attackState = new Phase2_MeleeAttackState(owner);
        var rangedAttackState = new Phase2_RangedAttackState(owner);
        var chaseState = new Phase1_Chase_State(owner);
        var blinkState = new Phase2_DataBlink_State(owner);
        var lockState = new Phase2_RansomLock_State(owner);

        subFsm = new StateMachine<Ransomware>(idleState);

        // Idle에서 Chase로 전환
        subFsm.AddTransition(new Transition<Ransomware>(
           idleState,
           chaseState,
           () => true
        ));

        // 특수 기술 전환 (우선순위가 높음)
        // 1. DataBlink 전환
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            blinkState,
            () => owner.AbilityManager.GetAbilityRemainingCooldown("DataBlink") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            blinkState,
            chaseState,
            () => blinkState.IsAnimationFinished()
        ));

        // 2. Lock 전환
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            lockState,
            () => owner.AbilityManager.GetAbilityRemainingCooldown("Lock") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            lockState,
            chaseState,
            () => lockState.IsAnimationFinished() // 수정: 이전에는 blinkState를 사용하고 있었음
        ));

        // 기본 근접 공격 전환 (우선순위가 낮음)
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            attackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= owner.MeleeAttackRange &&
             owner.AbilityManager.GetAbilityRemainingCooldown("BasicMeeleAttack") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            attackState,
            chaseState,
            () => attackState.IsAnimationFinished()
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            rangedAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) > owner.MeleeAttackRange &&
                  Vector3.Distance(owner.transform.position, owner.Player.position) <= owner.RangedAttackRange &&
                    owner.AbilityManager.GetAbilityRemainingCooldown("BasicRangedAttack") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            rangedAttackState,
            chaseState,
            () => rangedAttackState.IsAnimationFinished()
        ));

    }



    public override void Enter()
    {
        Debug.Log("랜섬웨어 보스 페이즈2 시작");
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase2"), 1);
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase1"), 1);
        InitializeStats();
        InitializeAbility();
        InitializeSubFSM();

    }

    public override void Update()
    {
        subFsm.Update();
    }

    public override void Exit()
    {
        // 페이즈 종료 시 서브FSM도 정리
        if (subFsm != null && subFsm.CurrentState != null)
        {
            subFsm.CurrentState.Exit();
        }
        subFsm = null;

    }

    public override void Interrupt()
    {
        if (isInterrupted) return;
        isInterrupted = true;

        // 현재 실행 중인 서브 스테이트의 Interrupt 호출
        if (subFsm != null && subFsm.CurrentState != null)
        {
            subFsm.CurrentState.Interrupt();
        }

        // 페이즈1 관련 정리 작업
        owner.AbilityManager.SetAbilityInactive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityInactive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityInactive("DataBlink");
        owner.AbilityManager.SetAbilityInactive("SummonShadows");
        owner.AbilityManager.SetAbilityInactive("Lock");

        owner.NmAgent.isStopped = true;
        owner.SetRotationLock(true);
    }
}
