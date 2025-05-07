using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCombatState_UnknownVirus : State<UnknownVirusBoss>
{
    public BasicCombatState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        // NavMeshAgent 활성화 및 이동 속도 설정
        owner.NmAgent.isStopped = false;
        owner.NmAgent.speed = owner.BossStatus.GetMovementSpeed();
        // 이동 애니메이션 시작
        owner.Animator.SetBool("IsMoving", true);
    }

    public override void Update()
    {
        // 플레이어를 향해 이동
        if (owner.NmAgent.isOnNavMesh && owner.Player != null)
        {
            owner.NmAgent.SetDestination(owner.Player.position);

            // 현재 속도에 따라 애니메이션 파라미터 조절
            float currentSpeed = owner.NmAgent.velocity.magnitude;
            owner.Animator.SetFloat("MoveSpeed", currentSpeed);
        }

        // 기본 공격, 맵 공격, 폼 변경 타이머 로직은
        // UnknownVirusBoss.Update() 내부에서 fsm.CurrentState == basicCombatState 일 때 자동 호출됩니다.
    }

    public override void Exit()
    {
        // 이동 애니메이션 종료
        owner.Animator.SetBool("IsMoving", false);
        owner.Animator.SetFloat("MoveSpeed", 0f);
        // NavMeshAgent 정지
        owner.NmAgent.isStopped = true;
    }
}
