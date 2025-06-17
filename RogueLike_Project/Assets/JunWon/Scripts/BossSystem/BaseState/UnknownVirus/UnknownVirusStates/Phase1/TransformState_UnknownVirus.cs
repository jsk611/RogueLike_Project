using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;
using DG.Tweening;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private float startTime = 0f;
    private float transformationTime = 3.5f; // 시간을 늘려서 더 자연스럽게
    private BossForm targetForm;
    private bool isTransforming = false;
    private bool hasTransformed = false;
    
    // 변신 단계별 연출을 위한 변수들
    private TransformPhase currentPhase = TransformPhase.None;
    private float phaseTimer = 0f;
    
    // 연출 효과 관련
    private AudioSource audioSource;
    private ParticleSystem transformEffect;
    private Light transformLight;
    
    public enum TransformPhase
    {
        None,
        Preparation,     // 준비 단계 (에너지 집중)
        Dissolution,     // 해체 단계
        Transformation,  // 변신 과정
        Stabilization,   // 안정화 단계
        Complete         // 완료
    }

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
        startTime = Time.time;
        isTransforming = true;
        hasTransformed = false;
        currentPhase = TransformPhase.Preparation;
        phaseTimer = 0f;

        if (owner.AbilityManager.UseAbility("Transform"))
        {
            owner.ResetFormTimer();
            targetForm = DecideNextForm();
            
            // 자연스러운 변신 시작
            StartNaturalTransformation();

            Debug.Log($"[TransformState] {owner.CurrentForm} 에서 {targetForm} 형태로 변신 시작");
        }
    }

    private void StartNaturalTransformation()
    {
        // 1단계: 준비 - 에너지 집중 효과
        owner.StartCoroutine(PreparationPhase());
    }
    
    private IEnumerator PreparationPhase()
    {
        currentPhase = TransformPhase.Preparation;
        
        // 라이트 효과 서서히 강화
        transformLight.DOIntensity(2f, 0.8f).SetEase(Ease.InOutSine);
        
        // 미묘한 진동 효과
        owner.transform.DOShakePosition(0.8f, 0.1f, 20, 90, false, true);
        
        // 에너지 집중 사운드
        if (audioSource != null)
        {
            // 여기에 에너지 집중 사운드 추가 가능
        }
        
        yield return new WaitForSeconds(0.8f);
        
        // 2단계: 해체 시작
        owner.StartCoroutine(DissolutionPhase());
    }
    
    private IEnumerator DissolutionPhase()
    {
        currentPhase = TransformPhase.Dissolution;
        
        // 더 드라마틱한 패턴으로 변경 - 부드러운 변신 사용
        owner.TRANSFORMDIRECTOR.SetTransformPattern(CubeTransformationDirector.TransformPattern.Implosion);
        owner.TRANSFORMDIRECTOR.StartCubeTransformation();
        
        // 라이트 색상 변화 (준비 -> 해체)
        transformLight.DOColor(Color.red, 0.5f);
        
        yield return new WaitForSeconds(1.2f);
        
        // 3단계: 실제 변신 과정
        owner.StartCoroutine(TransformationPhase());
    }
    
    private IEnumerator TransformationPhase()
    {
        currentPhase = TransformPhase.Transformation;
        
        // 변신 요청
        owner.RequestFormChange(targetForm);
        
        // 라이트 효과를 목표 형태에 맞게 조정
        Color targetColor = GetFormColor(targetForm);
        transformLight.DOColor(targetColor, 0.8f);
        transformLight.DOIntensity(3f, 0.4f).SetEase(Ease.OutCubic);
        
        yield return new WaitForSeconds(1.0f);
        
                 // 4단계: 안정화
         owner.StartCoroutine(StabilizationPhase());
    }
    
    private IEnumerator StabilizationPhase()
    {
        currentPhase = TransformPhase.Stabilization;
        
        // 변신 완료
        if (targetForm != BossForm.Basic)
            owner.ApplyForm(targetForm);
        
        // 라이트 서서히 안정화
        transformLight.DOIntensity(1f, 0.5f).SetEase(Ease.OutQuart);
        
        // 최종 완성 효과
        if (transformEffect != null)
            transformEffect.Play();
            
        yield return new WaitForSeconds(0.5f);
        
        // 완료
        currentPhase = TransformPhase.Complete;
        CompleteTransformation();
    }
    
    private Color GetFormColor(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: return Color.green;
            case BossForm.Trojan: return Color.yellow;
            case BossForm.Ransomware: return Color.magenta;
            default: return Color.cyan;
        }
    }

    public override void Update()
    {
        phaseTimer += Time.deltaTime;
        
        // 변신 애니메이션 완료 체크 (더 관대한 시간으로 조정)
        if (isTransforming && !hasTransformed && Time.time - startTime >= transformationTime)
        {
            if (currentPhase != TransformPhase.Complete)
            {
                // 아직 완료되지 않았다면 강제로 완료
                ForceCompleteTransformation();
            }
        }
        
        // 각 단계별 추가 효과들
        UpdatePhaseEffects();
    }
    
    private void UpdatePhaseEffects()
    {
        switch (currentPhase)
        {
            case TransformPhase.Preparation:
                // 준비 단계에서 미묘한 파티클 효과
                break;
            case TransformPhase.Dissolution:
                // 해체 단계에서 흩어지는 효과
                break;
            case TransformPhase.Transformation:
                // 변신 중 에너지 파동 효과
                break;
        }
    }
    
    private void ForceCompleteTransformation()
    {
        if (targetForm != BossForm.Basic)
            owner.ApplyForm(targetForm);
            
        currentPhase = TransformPhase.Complete;
        CompleteTransformation();
    }

    private void CompleteTransformation()
    {
        // 변신 완료 처리
        isTransforming = false;
        hasTransformed = true;
        
        // 라이트 효과 서서히 페이드아웃
        transformLight.DOIntensity(0f, 1f).SetEase(Ease.OutQuad);

        Debug.Log($"[TransformState] {targetForm} 형태로 변신 완료");
        Debug.Log($"[TransformState] formTimer: {owner.GetFormTimer()}, 지속시간: {owner.GetStayDuration()}초");
    }

    public override void Exit()
    {
        if (targetForm == BossForm.Basic) return;

        // 자연스러운 복귀 연출
        owner.StartCoroutine(NaturalReversion());
    }
    
    private IEnumerator NaturalReversion()
    {
        // 복귀 준비 효과
        transformLight.DOIntensity(1.5f, 0.3f);
        transformLight.DOColor(Color.white, 0.3f);
        
        yield return new WaitForSeconds(0.3f);
        
        // Transform State에서 나갈 때 항상 Basic으로 돌아감
        owner.ApplyForm(BossForm.Basic);
        owner.TRANSFORMDIRECTOR.RevertToOriginal();
        
        // 라이트 완전히 끄기
        transformLight.DOIntensity(0f, 0.5f);
        
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
            Debug.LogWarning("[TransformState] 사용 가능한 변신 형태 없음 - Basic 유지");
            return BossForm.Basic;
        }

        return availableForms[UnityEngine.Random.Range(0, availableForms.Count)];
    }
}