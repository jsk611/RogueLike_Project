using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private bool isTransformationComplete = false;
    private float stateEntryTime;
    private float transformDecisionDelay = 1.0f;
    private BossForm targetForm;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
    }

    #region LifeCycle
    public override void Enter()
    {
        Debug.Log("[TransformState_UnknownVirus] Enter");

        // 상태 초기화
        isTransformationComplete = false;
        stateEntryTime = Time.time;

        // 애니메이션 트리거 설정
        if (owner.Animator != null)
        {
            owner.Animator.SetTrigger("Transform");
        }

        // 변형 중 효과 적용 (선택적)
        ApplyTransformationEffects();
    }

    public override void Update()
    {
        // 변형이 완료되었으면 아무것도 하지 않음
        if (isTransformationComplete) return;

        // 폼 변형은 TransformRoutine 코루틴에서 처리
        // 여기서는 추가 로직만 처리
    }

    public override void Exit()
    {
        // 애니메이션 트리거 리셋
        if (owner.Animator != null)
        {
            owner.Animator.ResetTrigger("Transform");
        }

        // 변형 효과 정리
        CleanupTransformationEffects();
    }
    #endregion

    #region TransfomrFunc
    public void OnTransformationComplete()
    {
        isTransformationComplete = true;
    }

    
    private void ApplyTransformationEffects()
    {
        // 예: 파티클 효과, 사운드 등
    }

    private void CleanupTransformationEffects()
    {
        // 변형 관련 효과 정리
    }

    public bool IsTransformationComplete()
    {
        return isTransformationComplete;
    }
    #endregion
}