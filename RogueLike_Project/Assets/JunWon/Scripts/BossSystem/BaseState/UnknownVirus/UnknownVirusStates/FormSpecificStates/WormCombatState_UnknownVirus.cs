using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class WormCombatState_UnknownVirus : BaseState_UnknownVirus
{
    // 생성자에서 owner(UnknownVirusBoss) 할당
    public WormCombatState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        // 1) 본체(UnknownVirusBoss) 네비메시 에이전트 정지
        owner.NmAgent.isStopped = true;
        // 2) 본체 이동 애니메이션 끔
        owner.Animator.SetBool("IsMoving", false);

        // 3) 웜 폼 인스턴스 활성화는 TransformState에서 이미 수행되었으므로,
        //    여기선 별도 작업 없이 웜 보스의 FSM이 자체 Update()로 돌아가도록 함.
    }

    public override void Update()
    {
        // 웜 폼이 붙어있는 자식 오브젝트(WormBossPrime)의 Update() → fsm.Update() 자동 호출
        // 따라서 여긴 비워두어도 무방합니다.
        // 만약 강제 제어가 필요하다면 아래처럼 호출할 수도 있습니다:
        // var worm = owner.GetCurrentActiveBoss() as WormBossPrime;
        // worm?.ManualUpdate(); 
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
