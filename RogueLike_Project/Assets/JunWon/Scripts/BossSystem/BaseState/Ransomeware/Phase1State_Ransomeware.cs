using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1State_Ransomware : BossPhaseBase<Ransomware>
{
    private StateMachine<Ransomware> subFsm;

    public Phase1State_Ransomware(Ransomware owner) : base(owner)
    {
    }

    float attackRange = 5.0f;
    float rangedRange = 20.0f;
    float specialAttackChance = 0.2f; // 20% 확률
    float basicRangedAttackChance = 0.4f; // 40% 확률
    float basicMeeleAttackChance = 0.4f; // 40% 확률


    private void InitializeAbility()
    {
        owner.AbilityManager.SetAbilityActive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityActive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityActive("DataExplode");
        owner.AbilityManager.SetMaxCoolTime("DataExplode");

    }
    private void InitializeStats()
    {
        owner.MonsterStatus.SetMovementSpeed(5.0f);
    }


    private void InitializeSubFSM()
    {
       
        // 각 상태 초기화 (각 상태 클래스는 생성자에서 owner를 받습니다)
        var idleState = new Phase1_Idle_State(owner);
        var chaseState = new Phase1_Chase_State(owner);
        var meleeAttackState = new Phase1_Attack_State(owner);                // 근접 공격
        var rangedAttackState = new Phase1_BasicRangedAttack_State(owner);      // 원거리 공격
        var specialAttackState = new Phase1_SpeacialAttack_State(owner);        // 특수 공격

        subFsm = new StateMachine<Ransomware>(idleState);

        subFsm.AddTransition(new Transition<Ransomware>(
            idleState,
            chaseState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) > attackRange
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            specialAttackState,
            () => owner.AbilityManager.GetAbilityRemainingCooldown("DataExplode") == 0
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            rangedAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= rangedRange &&
            owner.AbilityManager.GetAbilityRemainingCooldown("BasicRangedAttack") == 0
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            meleeAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange &&
            owner.AbilityManager.GetAbilityRemainingCooldown("BasicMeeleAttack") == 0
        ));


        subFsm.AddTransition(new Transition<Ransomware>(
            specialAttackState,
            chaseState,
            () => specialAttackState.IsAnimationFinished()
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            meleeAttackState,
            chaseState,
            () => meleeAttackState.IsAnimationFinished()
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            rangedAttackState,
            chaseState,
            () => rangedAttackState.IsAnimationFinished()
        ));
    }



    public override void Enter()
    {
        Debug.Log("랜섬웨어 보스 페이즈1 시작");
        InitializeStats();
        InitializeAbility();
        InitializeSubFSM();
    }

    public override void Update()
    {
        if (isInterrupted) return;
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
        owner.AbilityManager.SetAbilityInactive("DataExplode");

        owner.NmAgent.isStopped = true;
        owner.SetRotationLock(true);
    }

}
