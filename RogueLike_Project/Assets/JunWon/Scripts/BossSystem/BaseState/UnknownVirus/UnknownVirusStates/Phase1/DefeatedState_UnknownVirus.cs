using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DefeatedState_UnknownVirus : BaseState_UnknownVirus
{
    [Header("사망 연출")]
    [SerializeField] private DeathFragmentSystem fragmentSystem;

    public DefeatedState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        fragmentSystem = owner.basic.GetComponent<DeathFragmentSystem>();

        Debug.Log("UnknownVirus: Dead State 진입");

        // 현재 진행 중인 상태 강제 중단
        InterruptCurrentState();

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

        owner.StartCoroutine(ExecuteSequentialDeath());
    }

    public override void Update()
    {
        // 기본 업데이트 로직
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Dead State 종료");
    }

    /// <summary>
    /// 현재 진행 중인 상태를 강제로 중단
    /// </summary>
    private void InterruptCurrentState()
    {
        // FSM의 현재 상태가 MapAttackState인지 확인하고 중단
        var currentState = owner.Fsm.CurrentState;

        if (currentState is MapAttackState_UnknownVirus mapAttackState)
        {
            Debug.Log("MapAttack 진행 중 사망 - 상태 강제 중단");
            mapAttackState.Interrupt();
        }
        else if (currentState is TransformState_UnknownVirus transformState)
        {
            Debug.Log("Transform 진행 중 사망 - 상태 강제 중단");
            transformState.Interrupt();
        }

        // 모든 코루틴 중단
        owner.StopAllCoroutines();

        // 진행 중인 큐브 공격 효과 즉시 중단
        StopAllActiveEffects();
    }

    /// <summary>
    /// 모든 활성화된 효과 중단
    /// </summary>
    private void StopAllActiveEffects()
    {
        if (owner.basic != null)
        {
            // 큐브 공격 효과 중단 - 현재 위치 유지
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx != null)
            {
                vfx.SetReturnMode(false); // 원래 위치로 돌아가지 않음
                vfx.StopEffect();
                Debug.Log("[DeathState] 큐브 효과 중단 - 현재 위치 유지");
            }
        }

        // 플로팅 효과 중단
        if (owner.FLOATINGEFFECT != null)
        {
            owner.FLOATINGEFFECT.SetPaused(true);
        }
    }

    private void HandleDeath()
    {
        // 조각 떨어뜨리기 연출 시작
        if (fragmentSystem != null)
        {
            fragmentSystem.TriggerDeathFragmentation();
        }
        else
        {
            Debug.LogWarning("DeathFragmentSystem이 없습니다.");
        }

        Debug.Log("[UnknownVirusBoss] 사망 - 조각 떨어뜨리기 연출 시작");
    }

    private IEnumerator ExecuteSequentialDeath()
    {
        HandleDeath();
        yield return new WaitForSeconds(2.0f);
        owner.gameObject.SetActive(false);
    }
}