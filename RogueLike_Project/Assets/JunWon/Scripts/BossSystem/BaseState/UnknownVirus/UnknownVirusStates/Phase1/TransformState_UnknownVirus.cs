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
        
        // 새로운 CyberTransformationSpace 시스템 사용
        var cyberSpace = owner.GetComponent<CyberTransformationSpace>();
        if (cyberSpace != null)
        {
            // 몬스터별 캡슐 데이터 생성
            MonsterCapsuleData customCapsule = new MonsterCapsuleData()
            {
                radius = GetFormRadius(targetForm),
                height = GetFormHeight(targetForm),
                scale = GetFormScale(targetForm),
                transformTime = 1.5f,
                direction = GetFormDirection(targetForm),
                forwardAxis = GetFormForwardAxis(targetForm),
                
                // 안개 효과 설정
                enableFogEffect = true,
                fogColor = GetFormFogColor(targetForm),
                fogDensity = GetFormFogDensity(targetForm),
                fogFadeTime = GetFormFogFadeTime(targetForm)
            };
            
            cyberSpace.StartTransformation(customCapsule, GetFormMonster(targetForm));
        }
        else
        {
            // 기존 시스템 백업용
        }
        
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
        // owner.TRANSFORMDIRECTOR.RevertToOriginal(); // CyberTransformationSpace 사용시 비활성화
        
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
    
    // 몬스터별 캡슐 설정 헬퍼 메소드들
    private float GetFormRadius(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: return 4f;      // 웜은 좀 더 슬림
            case BossForm.Trojan: return 5f;    // 트로이안은 중간 크기
            case BossForm.Ransomware: return 6f; // 랜섬웨어는 크고 위협적
            default: return 3f;
        }
    }
    
    private float GetFormHeight(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: return 10f;     // 웜은 길고 높게
            case BossForm.Trojan: return 7f;    // 트로이안은 균형잡힌 높이
            case BossForm.Ransomware: return 8f; // 랜섬웨어는 묵직하게
            default: return 6f;
        }
    }
    
    private Vector3 GetFormScale(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: return new Vector3(0.8f, 1.5f, 0.8f);      // 세로로 길게
            case BossForm.Trojan: return new Vector3(1.2f, 1f, 1.2f);      // 옆으로 넓게
            case BossForm.Ransomware: return new Vector3(1.3f, 0.9f, 1.3f); // 크고 낮게
            default: return Vector3.one;
        }
    }
    
    private GameObject GetFormMonster(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: return owner.Worm;
            case BossForm.Trojan: return owner.Troy;
            case BossForm.Ransomware: return owner.Ransomware;
            default: return null;
        }
    }
    
    private Vector3 GetFormDirection(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: 
                // 웜: 수평으로 길게 누워있는 느낌 (뱀처럼 유연함)
                return new Vector3(0.3f, 0.7f, 0f).normalized;
                
            case BossForm.Trojan: 
                // 트로이안: 전통적이고 견고한 수직 방향
                return Vector3.up;
                
            case BossForm.Ransomware: 
                // 랜섬웨어: 불안정하고 위협적인 비스듬한 각도
                return new Vector3(-0.4f, 0.8f, 0.2f).normalized;
                
            default: 
                return Vector3.up;
        }
    }
    
    private Vector3 GetFormForwardAxis(BossForm form)
    {
        switch (form)
        {
            case BossForm.Worm: 
                // 웜: 앞으로 기어가는 방향
                return Vector3.forward;
                
            case BossForm.Trojan: 
                // 트로이안: 정면을 향한 안정적인 방향
                return Vector3.forward;
                
            case BossForm.Ransomware: 
                // 랜섬웨어: 비틀어진 불안정한 축
                return new Vector3(0.2f, 0f, 0.8f).normalized;
                
            default: 
                                 return Vector3.forward;
         }
     }
     
     private Color GetFormFogColor(BossForm form)
     {
         switch (form)
         {
             case BossForm.Worm: 
                 // 웜: 독성 녹색 안개 (생물학적 위험)
                 return new Color(0.2f, 0.8f, 0.3f, 0.6f);
                 
             case BossForm.Trojan: 
                 // 트로이안: 경고 노란색 안개 (시스템 침입)
                 return new Color(1f, 0.8f, 0.2f, 0.6f);
                 
             case BossForm.Ransomware: 
                 // 랜섬웨어: 위협적인 빨간색 안개 (데이터 암호화)
                 return new Color(0.9f, 0.2f, 0.3f, 0.7f);
                 
             default: 
                 return new Color(0f, 0.7f, 1f, 0.5f); // 기본 사이안
         }
     }
     
     private float GetFormFogDensity(BossForm form)
     {
         switch (form)
         {
             case BossForm.Worm: 
                 // 웜: 중간 밀도 (은밀함)
                 return 0.4f;
                 
             case BossForm.Trojan: 
                 // 트로이안: 낮은 밀도 (정직한 침입?)
                 return 0.3f;
                 
             case BossForm.Ransomware: 
                 // 랜섬웨어: 높은 밀도 (완전한 은폐)
                 return 0.7f;
                 
             default: 
                 return 0.5f;
         }
     }
     
     private float GetFormFogFadeTime(BossForm form)
     {
         switch (form)
         {
             case BossForm.Worm: 
                 // 웜: 빠른 페이드 (재빠른 등장)
                 return 0.8f;
                 
             case BossForm.Trojan: 
                 // 트로이안: 중간 페이드 (안정적 등장)
                 return 1.2f;
                 
             case BossForm.Ransomware: 
                 // 랜섬웨어: 느린 페이드 (극적인 등장)
                 return 1.8f;
                 
             default: 
                 return 1.2f;
         }
     }
}