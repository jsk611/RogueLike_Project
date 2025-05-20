using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class RansomwareCombatState_UnknownVirus : BaseState_UnknownVirus
{
    // 생성자에서 owner(UnknownVirusBoss) 할당
    public RansomwareCombatState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        // 1) 본체(UnknownVirusBoss) 네비메시 에이전트 정지
        owner.NmAgent.isStopped = true;
        // 2) 본체 이동 애니메이션 끔
        owner.Animator.SetBool("IsMoving", false);

       
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        // 폼 변경 전환 시 본체 복귀를 위해 네비메시 에이전트 재활성화
        owner.NmAgent.isStopped = false;
    }

    public override void Interrupt()
    {
        // 필요 시 인터럽트 로직 추가
        base.Interrupt();
    }
}
