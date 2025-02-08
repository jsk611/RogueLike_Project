using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1State_Ransomware : BossPhaseBase<Ransomware>
{
    private enum Phase1SubState
    {
        Idle,
        Approach,
        Attack,
        Special
    }

    // 서브 FSM
    private StateMachine<Ransomware> subFsm;

    // 서브 상태


    public Phase1State_Ransomware(Ransomware owner) : base(owner)
    {
    }

    float attackRange = 5.0f;
    float rangedRange = 20.0f;
    float specialAttackChance = 0.2f; // 20% 확률
    float basicRangedAttackChance = 0.4f; // 40% 확률
    float basicMeeleAttackChance = 0.4f; // 40% 확률


    private void InitializeStats()
    {
        owner.MonsterStatus.SetMovementSpeed(5.0f);
    }


    private void InitializeSubFSM()
    {
        owner.AbilityManger.SetAbilityActive("BasicRangedAttack");
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
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange 
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            rangedAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= rangedRange
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            meleeAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange 
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            specialAttackState,
            chaseState,
            () => true
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            meleeAttackState,
            chaseState,
            () => true
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
        InitializeSubFSM();
    }

    public override void Update()
    {
        subFsm.Update();
    }
}
