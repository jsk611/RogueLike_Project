using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 사이버틱한 먼지 파티클 - 디지털 해체 효과
/// </summary>
public class CyberDustParticle : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem dustParticleSystem;
    [SerializeField] private Material cyberDustMaterial;
    [SerializeField] private bool autoStart = false;
    
    [Header("Cyber Effects")]
    [SerializeField] private Color[] cyberColors = { 
        Color.cyan, 
        Color.magenta, 
        new Color(0, 1, 0.5f, 1), // 네온 그린
        new Color(1, 0, 1, 1),    // 네온 핑크
        new Color(0.5f, 0.8f, 1, 1) // 전기 블루
    };
    [SerializeField] private AnimationCurve sizeOverLifetime = AnimationCurve.EaseInOut(0, 0.1f, 1, 0);
    [SerializeField] private AnimationCurve velocityOverLifetime = AnimationCurve.Linear(0, 1, 1, 0.3f);
    
    [Header("Dissolve Settings")]
    [SerializeField] private float particleLifetime = 3f;
    [SerializeField] private int maxParticles = 200;
    [SerializeField] private float emissionRate = 50f;
    [SerializeField] private Vector3 emissionArea = new Vector3(2f, 0.5f, 2f);
    
    [Header("Movement")]
    [SerializeField] private float driftSpeed = 1f;
    [SerializeField] private float randomForce = 2f;
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravityStrength = -0.5f;
    
    [Header("Digital Effects")]
    [SerializeField] private bool enableGlitch = true;
    [SerializeField] private float glitchIntensity = 0.3f;
    [SerializeField] private float glitchFrequency = 2f;
    [SerializeField] private bool enableDataStream = true;
    [SerializeField] private int dataStreamCount = 5;
    
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private ParticleSystem.ColorOverLifetimeModule colorModule;
    private ParticleSystem.SizeOverLifetimeModule sizeModule;
    private ParticleSystem.ForceOverLifetimeModule forceModule;
    
    // 디지털 글리치용
    private Coroutine glitchCoroutine;
    private bool isGlitching = false;
    
    void Start()
    {
        InitializeParticleSystem();
        if (autoStart)
        {
            PlayCyberDust();
        }
    }
    
    #region Initialization
    
    private void InitializeParticleSystem()
    {
        // 파티클 시스템이 없으면 생성
        if (dustParticleSystem == null)
        {
            GameObject particleObj = new GameObject("CyberDustParticles");
            particleObj.transform.SetParent(transform);
            dustParticleSystem = particleObj.AddComponent<ParticleSystem>();
        }
        
        SetupParticleModules();
        ApplyCyberSettings();
    }
    
    private void SetupParticleModules()
    {
        mainModule = dustParticleSystem.main;
        emissionModule = dustParticleSystem.emission;
        shapeModule = dustParticleSystem.shape;
        velocityModule = dustParticleSystem.velocityOverLifetime;
        colorModule = dustParticleSystem.colorOverLifetime;
        sizeModule = dustParticleSystem.sizeOverLifetime;
        forceModule = dustParticleSystem.forceOverLifetime;
        
        // 기본 설정
        mainModule.startLifetime = particleLifetime;
        mainModule.maxParticles = maxParticles;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 머티리얼 적용
        if (cyberDustMaterial != null)
        {
            var renderer = dustParticleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.material = cyberDustMaterial;
        }
    }
    
    private void ApplyCyberSettings()
    {
        // 메인 모듈
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        mainModule.startRotation = new ParticleSystem.MinMaxCurve(0, 360);
        mainModule.startColor = GetRandomCyberColor();
        
        // 방출 설정
        emissionModule.enabled = true;
        emissionModule.rateOverTime = emissionRate;
        
        // 모양 설정 (박스 형태로 영역에서 방출)
        shapeModule.enabled = true;
        shapeModule.shapeType = ParticleSystemShapeType.Box;
        shapeModule.scale = emissionArea;
        
        // 속도 변화
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.Local;
        velocityModule.radial = new ParticleSystem.MinMaxCurve(randomForce);
        
        // 색상 변화 (페이드 아웃)
        colorModule.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.white, 0.8f),
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorModule.color = gradient;
        
        // 크기 변화
        sizeModule.enabled = true;
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeOverLifetime);
        
        // 중력 및 힘
        if (useGravity)
        {
            forceModule.enabled = true;
            forceModule.space = ParticleSystemSimulationSpace.World;
            forceModule.y = new ParticleSystem.MinMaxCurve(gravityStrength);
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// 사이버 더스트 파티클 재생
    /// </summary>
    public void PlayCyberDust()
    {
        if (dustParticleSystem != null)
        {
            dustParticleSystem.Play();
            
            if (enableGlitch)
            {
                StartGlitchEffect();
            }
            
            if (enableDataStream)
            {
                StartCoroutine(CreateDataStreams());
            }
        }
    }
    
    /// <summary>
    /// 파티클 정지
    /// </summary>
    public void StopCyberDust()
    {
        if (dustParticleSystem != null)
        {
            dustParticleSystem.Stop();
        }
        
        StopGlitchEffect();
    }
    
    /// <summary>
    /// 즉시 버스트 방출
    /// </summary>
    public void BurstEmission(int count = 50)
    {
        if (dustParticleSystem != null)
        {
            dustParticleSystem.Emit(count);
        }
    }
    
    /// <summary>
    /// 색상 테마 변경
    /// </summary>
    public void SetColorTheme(Color[] newColors)
    {
        cyberColors = newColors;
        mainModule.startColor = GetRandomCyberColor();
    }
    
    /// <summary>
    /// 특정 위치에서 폭발적 방출
    /// </summary>
    public void ExplodeAt(Vector3 position, int particleCount = 30)
    {
        transform.position = position;
        
        // 임시로 방출 영역을 줄여서 집중된 효과
        Vector3 originalScale = shapeModule.scale;
        shapeModule.scale = Vector3.one * 0.1f;
        
        BurstEmission(particleCount);
        
        // 원래 크기로 복원
        DOVirtual.DelayedCall(0.1f, () => {
            shapeModule.scale = originalScale;
        });
    }
    
    #endregion
    
    #region Digital Effects
    
    private void StartGlitchEffect()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }
        glitchCoroutine = StartCoroutine(GlitchEffectLoop());
    }
    
    private void StopGlitchEffect()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }
        isGlitching = false;
    }
    
    private IEnumerator GlitchEffectLoop()
    {
        while (dustParticleSystem.isPlaying)
        {
            yield return new WaitForSeconds(1f / glitchFrequency);
            
            if (Random.value < glitchIntensity)
            {
                yield return StartCoroutine(ExecuteGlitch());
            }
        }
    }
    
    private IEnumerator ExecuteGlitch()
    {
        isGlitching = true;
        
        // 색상 글리치
        Color originalColor = mainModule.startColor.color;
        Color glitchColor = cyberColors[Random.Range(0, cyberColors.Length)];
        glitchColor.a = Random.Range(0.3f, 1f);
        
        mainModule.startColor = glitchColor;
        
        // 크기 글리치
        float originalSize = mainModule.startSize.constant;
        mainModule.startSize = originalSize * Random.Range(0.5f, 2f);
        
        // 방출률 글리치
        float originalEmission = emissionModule.rateOverTime.constant;
        emissionModule.rateOverTime = originalEmission * Random.Range(0.2f, 3f);
        
        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        
        // 원래 설정으로 복원
        mainModule.startColor = originalColor;
        mainModule.startSize = originalSize;
        emissionModule.rateOverTime = originalEmission;
        
        isGlitching = false;
    }
    
    private IEnumerator CreateDataStreams()
    {
        for (int i = 0; i < dataStreamCount; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            
            Vector3 streamStart = transform.position + Random.insideUnitSphere * 2f;
            Vector3 streamEnd = transform.position + Random.insideUnitSphere * 5f;
            
            StartCoroutine(CreateSingleDataStream(streamStart, streamEnd));
        }
    }
    
    private IEnumerator CreateSingleDataStream(Vector3 start, Vector3 end)
    {
        GameObject streamObj = new GameObject("DataStream");
        streamObj.transform.position = start;
        
        ParticleSystem stream = streamObj.AddComponent<ParticleSystem>();
        var streamMain = stream.main;
        streamMain.startLifetime = 1f;
        streamMain.startSpeed = 0f;
        streamMain.maxParticles = 20;
        streamMain.startSize = 0.02f;
        streamMain.startColor = cyberColors[Random.Range(0, cyberColors.Length)];
        
        var streamShape = stream.shape;
        streamShape.enabled = true;
        streamShape.shapeType = ParticleSystemShapeType.BoxEdge;
        
        // 스트림을 목표 지점으로 이동
        float duration = Vector3.Distance(start, end) / 3f;
        streamObj.transform.DOMove(end, duration).SetEase(Ease.Linear);
        
        yield return new WaitForSeconds(duration + 1f);
        
        Destroy(streamObj);
    }
    
    #endregion
    
    #region Utility Methods
    
    private Color GetRandomCyberColor()
    {
        if (cyberColors.Length == 0) return Color.cyan;
        return cyberColors[Random.Range(0, cyberColors.Length)];
    }
    
    /// <summary>
    /// 실시간 파라미터 조정
    /// </summary>
    public void AdjustIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        
        emissionModule.rateOverTime = emissionRate * intensity;
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(0.5f * intensity, 2f * intensity);
        
        if (intensity > 0.7f)
        {
            glitchFrequency = 5f;
            glitchIntensity = 0.6f;
        }
        else
        {
            glitchFrequency = 2f;
            glitchIntensity = 0.3f;
        }
    }
    
    /// <summary>
    /// 디버그용 파티클 정보
    /// </summary>
    public void DebugParticleInfo()
    {
        if (dustParticleSystem != null)
        {
            Debug.Log($"[CyberDust] Particles: {dustParticleSystem.particleCount}/{maxParticles}");
            Debug.Log($"[CyberDust] IsPlaying: {dustParticleSystem.isPlaying}");
            Debug.Log($"[CyberDust] Glitching: {isGlitching}");
        }
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        // 방출 영역 표시
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, emissionArea);
        
        // 중심점 표시
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.zero, 0.1f);
    }
    
    #endregion
} 