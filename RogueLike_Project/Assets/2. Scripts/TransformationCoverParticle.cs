using System.Collections;
using UnityEngine;

/// <summary>
/// 변신 시 내부 오브젝트들을 자연스럽게 가려주는 사이버 파티클 효과
/// CyberDustParticle 셰이더를 활용하여 밀도 있는 커버 효과 제공
/// </summary>
public class TransformationCoverParticle : MonoBehaviour
{
    [Header("Particle System References")]
    [SerializeField] private ParticleSystem coverParticleSystem;
    [SerializeField] private ParticleSystem glitchParticleSystem;
    [SerializeField] private ParticleSystem energyParticleSystem;
    
    [Header("Materials")]
    [SerializeField] private Material cyberDustMaterial;
    
    [Header("Cover Settings")]
    [SerializeField] private Vector3 coverArea = new Vector3(3f, 4f, 3f);
    [SerializeField] private float transformationDuration = 3f;
    [SerializeField] private int densityMultiplier = 500; // 높은 밀도로 완전 커버
    [SerializeField] private bool instantCover = true; // 즉시 커버 여부
    
    [Header("Visual Effects")]
    [SerializeField] private Color[] cyberColors = {
        new Color(0, 1, 1, 1f),        // 시안 (불투명)
        new Color(1, 0, 1, 1f),        // 마젠타 (불투명)
        new Color(0, 1, 0.5f, 1f),     // 네온 그린 (불투명)
        new Color(0.5f, 0.8f, 1, 1f),  // 전기 블루 (불투명)
        new Color(1, 0.5f, 0, 1f)      // 오렌지 (불투명)
    };
    
    [Header("Shader Properties")]
    [SerializeField] private float glitchIntensity = 0.7f;
    [SerializeField] private float glitchSpeed = 15f;
    [SerializeField] private float hologramStrength = 1.5f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float emissionBoost = 3f;
    [SerializeField] private float dissolveAmount = 0f; // 변신 중에는 디졸브 끄기
    
    private bool isPlaying = false;
    private Coroutine transformationCoroutine;
    
    void Start()
    {
        InitializeParticleSystems();
    }
    
    private void InitializeParticleSystems()
    {
        // Cover 파티클 시스템 설정
        if (coverParticleSystem == null)
        {
            GameObject coverObj = new GameObject("CoverParticles");
            coverObj.transform.SetParent(transform);
            coverParticleSystem = coverObj.AddComponent<ParticleSystem>();
        }
        
        SetupCoverParticles();
        
        // 글리치와 에너지 파티클도 설정 (옵션)
        if (glitchParticleSystem != null) SetupGlitchParticles();
        if (energyParticleSystem != null) SetupEnergyParticles();
    }
    
    /// <summary>
    /// 변신 효과 시작 - 내부를 완전히 가려주는 파티클 생성
    /// </summary>
    public void StartTransformationCover()
    {
        if (isPlaying) return;
        
        isPlaying = true;
        
        // 셰이더 프로퍼티 설정
        UpdateShaderProperties();
        
        if (instantCover)
        {
            // 즉시 강력한 커버 시작
            coverParticleSystem.Play();
            if (energyParticleSystem != null) energyParticleSystem.Play();
            
            transformationCoroutine = StartCoroutine(TransformationSequence());
        }
        else
        {
            transformationCoroutine = StartCoroutine(GradualTransformationSequence());
        }
    }
    
    /// <summary>
    /// 변신 효과 중지
    /// </summary>
    public void StopTransformationCover()
    {
        if (!isPlaying) return;
        
        isPlaying = false;
        
        if (transformationCoroutine != null)
        {
            StopCoroutine(transformationCoroutine);
        }
        
        coverParticleSystem.Stop();
        if (glitchParticleSystem != null) glitchParticleSystem.Stop();
        if (energyParticleSystem != null) energyParticleSystem.Stop();
    }
    
    /// <summary>
    /// 커버 영역 크기 실시간 조정
    /// </summary>
    public void SetCoverArea(Vector3 newArea)
    {
        coverArea = newArea;
        
        if (coverParticleSystem != null)
        {
            var shape = coverParticleSystem.shape;
            shape.scale = coverArea;
        }
    }
    
    /// <summary>
    /// 변신 지속 시간 설정
    /// </summary>
    public void SetTransformationDuration(float duration)
    {
        transformationDuration = duration;
    }
    
    private void SetupCoverParticles()
    {
        var main = coverParticleSystem.main;
        var emission = coverParticleSystem.emission;
        var shape = coverParticleSystem.shape;
        var velocityOverLifetime = coverParticleSystem.velocityOverLifetime;
        var colorOverLifetime = coverParticleSystem.colorOverLifetime;
        
        // 메인 설정 - 최대 밀도로 완전 커버
        main.startLifetime = transformationDuration + 1f; // 여유 시간
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.8f); // 느린 움직임
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.5f); // 큰 파티클들
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360);
        main.startColor = cyberColors[Random.Range(0, cyberColors.Length)];
        main.maxParticles = densityMultiplier * 4; // 매우 높은 밀도
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // 초당 방출량 최대화
        emission.enabled = true;
        emission.rateOverTime = densityMultiplier * 2f;
        
        // 박스 형태로 변신 영역 완전 덮기
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = coverArea;
        
        // 최소한의 움직임 (가려주는 것이 목적)
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.1f); // 매우 느린 확산
        
        // 색상 변화 - 불투명하게 유지
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(cyberColors[0], 0.0f),
                new GradientColorKey(cyberColors[1], 0.25f),
                new GradientColorKey(cyberColors[2], 0.5f),
                new GradientColorKey(cyberColors[3], 0.75f),
                new GradientColorKey(cyberColors[4], 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),    // 시작은 투명
                new GradientAlphaKey(1.0f, 0.1f),    // 빠르게 불투명
                new GradientAlphaKey(1.0f, 0.8f),    // 대부분 시간 불투명 유지
                new GradientAlphaKey(0.5f, 0.95f),   // 마지막에 천천히 페이드
                new GradientAlphaKey(0.0f, 1.0f)     // 최종 투명
            }
        );
        colorOverLifetime.color = gradient;
        
        // 머티리얼 적용
        if (cyberDustMaterial != null)
        {
            var renderer = coverParticleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.material = cyberDustMaterial;
            renderer.sortingOrder = 15; // 최상위 렌더링
        }
    }
    
    private void SetupGlitchParticles()
    {
        var main = glitchParticleSystem.main;
        var emission = glitchParticleSystem.emission;
        var shape = glitchParticleSystem.shape;
        
        main.startLifetime = 0.3f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = new Color(1, 0, 1, 0.9f); // 강한 마젠타
        main.maxParticles = 150;
        
        emission.enabled = false; // 버스트만 사용
        
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = coverArea.magnitude * 0.6f;
    }
    
    private void SetupEnergyParticles()
    {
        var main = energyParticleSystem.main;
        var emission = energyParticleSystem.emission;
        var shape = energyParticleSystem.shape;
        var velocityOverLifetime = energyParticleSystem.velocityOverLifetime;
        
        main.startLifetime = transformationDuration * 0.7f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startColor = cyberColors[4]; // 오렌지 에너지
        main.maxParticles = 100;
        
        emission.rateOverTime = 40;
        
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = coverArea.x * 0.8f;
        
        // 위로 올라가는 에너지 스트림
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(2f, 4f);
    }
    
    private void UpdateShaderProperties()
    {
        if (cyberDustMaterial == null) return;
        
        // 셰이더 프로퍼티 설정으로 사이버틱한 효과 강화
        cyberDustMaterial.SetFloat("_GlitchIntensity", glitchIntensity);
        cyberDustMaterial.SetFloat("_GlitchSpeed", glitchSpeed);
        cyberDustMaterial.SetFloat("_DissolveAmount", dissolveAmount); // 변신 중에는 디졸브 비활성화
        cyberDustMaterial.SetFloat("_HoloStrength", hologramStrength);
        cyberDustMaterial.SetFloat("_PulseSpeed", pulseSpeed);
        cyberDustMaterial.SetFloat("_EmissionBoost", emissionBoost);
    }
    
    private IEnumerator TransformationSequence()
    {
        // 변신 중간중간 글리치 효과 추가
        if (glitchParticleSystem != null)
        {
            float glitchInterval = transformationDuration / 8f;
            
            for (int i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(glitchInterval);
                glitchParticleSystem.Emit(Random.Range(20, 40));
            }
        }
        else
        {
            yield return new WaitForSeconds(transformationDuration);
        }
        
        // 변신 완료 후 점진적 페이드아웃
        yield return StartCoroutine(FadeOutEffect());
        
        isPlaying = false;
    }
    
    private IEnumerator GradualTransformationSequence()
    {
        // 점진적으로 강해지는 커버 효과
        float buildUpTime = transformationDuration * 0.3f;
        float mainTime = transformationDuration * 0.4f;
        float fadeTime = transformationDuration * 0.3f;
        
        // 1단계: 빌드업
        coverParticleSystem.Play();
        var emission = coverParticleSystem.emission;
        float maxRate = emission.rateOverTime.constant;
        
        float timer = 0f;
        while (timer < buildUpTime)
        {
            float progress = timer / buildUpTime;
            emission.rateOverTime = maxRate * progress;
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 2단계: 메인 커버 단계
        emission.rateOverTime = maxRate;
        if (energyParticleSystem != null) energyParticleSystem.Play();
        
        yield return new WaitForSeconds(mainTime);
        
        // 3단계: 페이드아웃
        yield return StartCoroutine(FadeOutEffect());
        
        isPlaying = false;
    }
    
    private IEnumerator FadeOutEffect()
    {
        float fadeTime = 1.5f;
        var coverEmission = coverParticleSystem.emission;
        var energyEmission = energyParticleSystem?.emission;
        
        float startCoverRate = coverEmission.rateOverTime.constant;
        float startEnergyRate = energyEmission?.rateOverTime.constant ?? 0f;
        
        float timer = 0f;
        while (timer < fadeTime)
        {
            float progress = timer / fadeTime;
            float fadeAmount = Mathf.Lerp(1f, 0f, progress);
            
            coverEmission.rateOverTime = startCoverRate * fadeAmount;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        StopTransformationCover();
    }
    
    void OnDrawGizmosSelected()
    {
        // 커버 영역 시각화
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, coverArea);
        
        // 글리치 영역 시각화
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, coverArea.magnitude * 0.6f);
    }
} 