using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private float startTime = 0f;
    private float transformationTime = 1.0f;
    private BossForm targetForm;

    bool isTransforming = false;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Transform State 진입");
        startTime = Time.time;
        isTransforming = true;

        // 다음 폼 결정 (현재 Basic이면 다른 폼으로, 아니면 Basic으로)
        if (owner.CurrentForm == BossForm.Basic)
            targetForm = DecideNextForm();

        // 변신 요청 - 이펙트 활성화 등
        owner.RequestFormChange(targetForm);

        Debug.Log($"[TransformState] {targetForm} 폼으로 변신 시작");
    }

    public override void Update()
    {
        // 변신 완료
        if (isTransforming && Time.time - startTime >= transformationTime)
        {
            CompleteTransformation();
        }
    }

    private void CompleteTransformation()
    {
        // 폼 적용
        owner.ApplyForm(targetForm);

        // 변신 완료 설정
        isTransforming = false;

        Debug.Log($"[TransformState] {targetForm} 폼으로 변신 완료");
    }
    public override void Exit()
    {
        targetForm = BossForm.Basic;
        CompleteTransformation();
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
            return BossForm.Basic;

        return availableForms[UnityEngine.Random.Range(0, availableForms.Count)];
    }
}