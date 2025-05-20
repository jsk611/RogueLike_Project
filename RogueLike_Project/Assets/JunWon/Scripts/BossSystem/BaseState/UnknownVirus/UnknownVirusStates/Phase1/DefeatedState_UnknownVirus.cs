using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatedState_UnknownVirus : BaseState_UnknownVirus
{
    private float timer = 0f;
    private const float deathDuration = 5f;
    private bool deathEffectSpawned = false;

    public DefeatedState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Dead State 진입");
        timer = 0f;
        deathEffectSpawned = false;

        // 모든 보스 형태 비활성화
        owner.ApplyForm(UnknownVirusBoss.BossForm.Basic);

        // 컴포넌트 비활성화
        if (owner.NmAgent != null)
        {
            owner.NmAgent.isStopped = true;
            owner.NmAgent.enabled = false;
        }

        // 사망 이벤트 발생
        EventManager.Instance.TriggerMonsterKilledEvent(true);

        // 사망 애니메이션
        if (owner.Animator != null)
        {
            owner.Animator.SetTrigger("Death");
        }
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Dead State 종료");
    }
}
