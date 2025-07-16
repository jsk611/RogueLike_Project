using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;
using DG.Tweening;
using Unity.VisualScripting;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private float startTime = 0f;
    private float transformationTime = 5.0f; // 변신 연출 시간
    private float formDuration = 5.0f; // 변신 상태 유지 시간
    private BossForm targetForm;
    
    // 상태 관리
    private bool isTransformationComplete = false;
    private bool isReversionStarted = false;
    private bool isReversionComplete = false;
    
    // 연출 효과 관련
    private AudioSource audioSource;
    private Light transformLight;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
        InitializeEffects();
    }
    
    private void InitializeEffects()
    {
        // 오디오 소스 초기화
        audioSource = owner.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = owner.gameObject.AddComponent<AudioSource>();
            
        // 변신 라이트 효과 찾기/생성
        transformLight = owner.GetComponentInChildren<Light>();
        if (transformLight == null)
        {
            GameObject lightObj = new GameObject("TransformLight");
            lightObj.transform.SetParent(owner.transform);
            lightObj.transform.localPosition = Vector3.up * 2f;
            transformLight = lightObj.AddComponent<Light>();
            transformLight.type = LightType.Point;
            transformLight.color = Color.cyan;
            transformLight.intensity = 0f;
            transformLight.range = 10f;
        }
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Transform State 진입");

        // 상태 초기화
        SetAttackFinished(false);
        startTime = Time.time;
        isTransformationComplete = false;
        isReversionStarted = false;
        isReversionComplete = false;

        if (owner.AbilityManager.UseAbility("Transform"))
        {
            owner.ResetFormTimer();
            targetForm = DecideNextForm();

            Debug.Log($"[TransformState] {owner.CurrentForm} 에서 {targetForm} 형태로 변신 시작");

            // 변신 연출 시작
            owner.StartCoroutine(TransformationSequence());
        }
    }

    /// <summary>
    /// 완전한 변신 시퀀스 (드롭 → 변신 → 완료)
    /// </summary>
    private IEnumerator TransformationSequence()
    {
        Debug.Log("[TransformState] 변신 시퀀스 시작");

        // 1단계: 드롭 연출 (완료까지 대기)
        yield return owner.StartCoroutine(owner.FLOATINGEFFECT.DropSequence());

        // 2단계: 폼 변경 요청
        owner.RequestFormChange(targetForm);
        yield return new WaitForSeconds(0.5f);

        // 3단계: 폼 적용
        owner.ApplyForm(targetForm);
        VirusDissolveEffect VDE = owner.GetCurrentActiveBoss().GetComponent<VirusDissolveEffect>();
        // yield return owner.StartCoroutine(VDE.AppearSequence(1.5f));

        // 4단계: 변신 완료 처리
        CompleteTransformation();

        Debug.Log("[TransformState] 변신 시퀀스 완료");
    }

    public override void Update()
    {
        // 복귀 연출이 완료되면 상태 전환 허용
        if (isReversionComplete)
        {
            return;
        }

        // 복귀 연출이 시작되었으면 다른 로직 무시
        if (isReversionStarted)
        {
            return;
        }

        // 변신이 완료되고 지정된 시간이 지나면 복귀 시작
        if (isTransformationComplete && 
            Time.time - startTime >= transformationTime + formDuration)
        {
            GetStateInfo();
            StartReversion();
        }
        
    }

    private void CompleteTransformation()
    {
        isTransformationComplete = true;
        
        // 라이트 효과 서서히 페이드아웃
        transformLight.DOIntensity(0f, 1f).SetEase(Ease.OutQuad);

        Debug.Log($"[TransformState] {targetForm} 형태로 변신 완료");
        Debug.Log($"[TransformState] {formDuration}초 후 복귀 예정");
    }

    private void ForceCompleteTransformation()
    {
        if (targetForm != BossForm.Basic)
            owner.ApplyForm(targetForm);
            
        CompleteTransformation();
    }

    /// <summary>
    /// 복귀 연출 시작
    /// </summary>
    private void StartReversion()
    {
        if (isReversionStarted) return;

        Debug.Log("[TransformState] 복귀 연출 시작");
        isReversionStarted = true;
        
        owner.StartCoroutine(ReversionSequence());
    }

    /// <summary>
    /// 완전한 복귀 시퀀스 (Basic 변환 → 라이즈 연출 → 완료)
    /// </summary>
    private IEnumerator ReversionSequence()
    {
        Debug.Log("[TransformState] 복귀 시퀀스 시작");

        VirusDissolveEffect VDE = owner.GetCurrentActiveBoss().GetComponent<VirusDissolveEffect>();
        // yield return owner.StartCoroutine(VDE.DissolveSequence(1.5f));

        // 1단계: Basic 폼으로 전환
        owner.ApplyForm(BossForm.Basic);
        yield return new WaitForSeconds(0.3f);

        // 2단계: 라이즈 연출 (완료까지 대기)
        yield return owner.StartCoroutine(owner.FLOATINGEFFECT.EpicGroupRise());

        // 3단계: 복귀 완료 처리
        CompleteReversion();
        yield return new WaitForSeconds(0.3f);


        Debug.Log("[TransformState] 복귀 시퀀스 완료");
    }

    private void CompleteReversion()
    {
        isReversionComplete = true;
        SetAttackFinished(true);

        Debug.Log("[TransformState] 복귀 완료 - 상태 전환 허용");
    }

    public override void Exit()
    {
        Debug.Log("[TransformState] Exit 완료");
    }

    /// <summary>
    /// 애니메이션이 완료되었는지 확인 (외부에서 상태 전환 체크용)
    /// </summary>
    public bool IsAnimationFinished() => isAttackFinished && isReversionComplete;

    /// <summary>
    /// 현재 상태 정보 (디버깅용)
    /// </summary>
    public string GetStateInfo()
    {
        return $"Transform: {isTransformationComplete}, Reversion: {isReversionStarted}/{isReversionComplete}, Elapsed: {Time.time - startTime:F1}s";
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
            Debug.LogWarning("[TransformState] 사용 가능한 변신 형태 없음 - Basic 유지");
            return BossForm.Basic;
        }

        return availableForms[UnityEngine.Random.Range(0, availableForms.Count)];
    }
}