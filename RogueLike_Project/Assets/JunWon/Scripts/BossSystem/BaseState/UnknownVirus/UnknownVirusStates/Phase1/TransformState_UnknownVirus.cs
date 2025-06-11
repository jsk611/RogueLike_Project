using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private float startTime = 0f;
    private float transformationTime = 2.5f;
    private BossForm targetForm;
    private bool isTransforming = false;
    private bool hasTransformed = false;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Transform State 진입");
        startTime = Time.time;
        isTransforming = true;
        hasTransformed = false;

        if (owner.AbilityManager.UseAbility("Transform"))
        {
            owner.TRANSFORMDIRECTOR.SetTransformPattern(CubeTransformationDirector.TransformPattern.Implosion);
            owner.TRANSFORMDIRECTOR.SetTransformDuration(transformationTime);
            owner.TRANSFORMDIRECTOR.StartCubeTransformation();

            owner.ResetFormTimer();
            // 항상 Basic이 아닌 다른 폼으로 변신
            targetForm = DecideNextForm();

            // 변신 요청 - 이펙트 활성화 등
            owner.RequestFormChange(targetForm);

            Debug.Log($"[TransformState] {owner.CurrentForm} → {targetForm} 폼으로 변신 시작");
        }
    }

    public override void Update()
    {
        // 변신 애니메이션 완료 체크
        if (isTransforming && !hasTransformed && Time.time - startTime >= transformationTime)
        {
            CompleteTransformation();
        }
    }

    private void CompleteTransformation()
    {
        if (targetForm == BossForm.Basic) return;
        // 폼 적용 (여기서 formTimer가 설정됨)
        owner.ApplyForm(targetForm);

        // 변신 완료 설정
        isTransforming = false;
        hasTransformed = true;

        Debug.Log($"[TransformState] {targetForm} 폼으로 변신 완료");
        Debug.Log($"[TransformState] formTimer: {owner.GetFormTimer()}, 지속시간: {owner.GetStayDuration()}초");
    }

    public override void Exit()
    {
        if (targetForm == BossForm.Basic) return;

        // Transform State에서 나갈 때는 항상 Basic으로 돌아감
        owner.ApplyForm(BossForm.Basic);
        owner.TRANSFORMDIRECTOR.RevertToOriginal();
        Debug.Log($"[TransformState] Exit - {owner.CurrentForm}에서 Basic으로 복귀");
    }

    private BossForm DecideNextForm()
    {
        List<BossForm> availableForms = new List<BossForm>();

        if (owner.Worm != null)
            availableForms.Add(BossForm.Worm);
        if (owner.Troy != null)
            availableForms.Add(BossForm.Trojan);
        if (owner.Ransomware != null)
            availableForms.Add(BossForm.Ransomware);

        if (availableForms.Count == 0)
        {
            Debug.LogWarning("[TransformState] 사용 가능한 변신 폼이 없음 - Basic 유지");
            return BossForm.Basic;
        }

        return availableForms[UnityEngine.Random.Range(0, availableForms.Count)];
    }
}