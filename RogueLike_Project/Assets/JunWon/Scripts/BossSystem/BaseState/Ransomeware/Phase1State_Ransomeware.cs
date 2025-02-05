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

    private void InitializeStats()
    {
        owner.MonsterStatus.SetMovementSpeed(5.0f);
    }


    private void InitializeSubFSM()
    {
        // 페이즈 1의 서브 상태들 초기화
        var idleState = new Phase1_Idle_State(owner);
        var chaseState = new Phase1_Chase_State(owner);
        var attackState = new Phase1_Attack_State(owner);

        subFsm = new StateMachine<Ransomware>(idleState);

        // 서브 상태 전환 조건들 설정
        subFsm.AddTransition(new Transition<Ransomware>(
            idleState,
            chaseState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) > attackRange
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            attackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange
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
