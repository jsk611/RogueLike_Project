using System.Collections;
using UnityEngine;

/// <summary>
/// 오로라 이펙트로 몬스터 변신을 가리는 연출 시스템
/// </summary>
public class AuroraTransformationEffect : MonoBehaviour
{
    [Header("Aurora Particle Systems")]
    [SerializeField] private ParticleSystem auroraWaves;        // 메인 오로라 파도
    [SerializeField] private ParticleSystem auroraCurtain;      // 커튼 효과
    [SerializeField] private ParticleSystem auroraSparkles;     // 반짝임
    
    [Header("Aurora Materials")]
    [SerializeField] private Material auroraMaterial;          // 오로라 머티리얼
    
    [Header("Transformation Settings")]
    [SerializeField] private Vector3 effectArea = new Vector3(5f, 6f, 5f);
    [SerializeField] private float effectDuration = 5f;
    [SerializeField] private float coverDuration = 3f;         // 완전히 가려지는 시간
    [SerializeField] private float transformDelay = 2f;        // 가려진 후 변신까지의 시간
    
    [Header("Aurora Colors")]
    [SerializeField] private Color[] auroraColors = {
        new Color(0.2f, 1f, 0.8f, 0.8f),    // 청록색
        new Color(0.8f, 0.2f, 1f, 0.8f),    // 보라색
        new Color(0.2f, 0.8f, 1f, 0.8f),    // 하늘색
        new Color(1f, 0.4f, 0.8f, 0.8f),    // 분홍색
        new Color(0.6f, 1f, 0.2f, 0.8f)     // 연두색
    };
    
    [Header("Visual Settings")]
    [SerializeField] private int waveDensity = 300;            // 오로라 파도 밀도
    [SerializeField] private float waveSpeed = 2f;             // 파도 속도
    [SerializeField] private float curtainOpacity = 1f;        // 커튼 불투명도
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip auroraRiseSound;        // 오로라 상승 소리
    [SerializeField] private AudioClip transformCompleteSound; // 변신 완료 소리
    
    private bool isTransforming = false;
    private Coroutine transformCoroutine;
    
    void Start()
    {
        InitializeParticleSystems();
    }
    
    private void InitializeParticleSystems()
    {
        // 오로라 파도 파티클 시스템 생성
        if (auroraWaves == null)
        {
            GameObject wavesObj = new GameObject("AuroraWaves");
            wavesObj.transform.SetParent(transform);
            auroraWaves = wavesObj.AddComponent<ParticleSystem>();
        }
        
        // 오로라 커튼 파티클 시스템 생성
        if (auroraCurtain == null)
        {
            GameObject curtainObj = new GameObject("AuroraCurtain");
            curtainObj.transform.SetParent(transform);
            auroraCurtain = curtainObj.AddComponent<ParticleSystem>();
        }
        
        // 반짝임 파티클 시스템 생성
        if (auroraSparkles == null)
        {
            GameObject sparklesObj = new GameObject("AuroraSparkles");
            sparklesObj.transform.SetParent(transform);
            auroraSparkles = sparklesObj.AddComponent<ParticleSystem>();
        }
        
        SetupAuroraWaveParticles();
        SetupAuroraCurtainParticles();
        SetupAuroraSparkleParticles();
    }
    
    /// <summary>
    /// 메인 오로라 파도 설정
    /// </summary>
    private void SetupAuroraWaveParticles()
    {
        var main = auroraWaves.main;
        var emission = auroraWaves.emission;
        var shape = auroraWaves.shape;
        var velocityOverLifetime = auroraWaves.velocityOverLifetime;
        var sizeOverLifetime = auroraWaves.sizeOverLifetime;
        var colorOverLifetime = auroraWaves.colorOverLifetime;
        
        // 메인 설정
        main.startLifetime = effectDuration * 0.8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, waveSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360);
        main.startColor = GetRandomAuroraColor();
        main.maxParticles = waveDensity;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // 방출 설정
        emission.enabled = true;
        emission.rateOverTime = waveDensity * 0.6f;
        
        // 모양 설정 - 원형 바닥에서 시작
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectArea.x * 0.9f;
        shape.position = Vector3.down * (effectArea.y * 0.4f);
        
        // 위로 흐르는 속도 + 회전 효과
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(waveSpeed, waveSpeed * 1.5f);
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        
        // 크기 변화
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.2f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(0.7f, 1.2f);
        sizeCurve.AddKey(1f, 0.8f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // 오로라 색상 변화
        colorOverLifetime.enabled = true;
        Gradient auroraGradient = CreateAuroraGradient();
        colorOverLifetime.color = auroraGradient;
        
        // 머티리얼 적용
        var renderer = auroraWaves.GetComponent<ParticleSystemRenderer>();
        if (auroraMaterial != null)
        {
            renderer.material = auroraMaterial;
        }
        else
        {
            renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        }
        renderer.sortingOrder = 8;
    }
    
    /// <summary>
    /// 오로라 커튼 설정 - 완전히 가리는 불투명한 커튼
    /// </summary>
    private void SetupAuroraCurtainParticles()
    {
        var main = auroraCurtain.main;
        var emission = auroraCurtain.emission;
        var shape = auroraCurtain.shape;
        var velocityOverLifetime = auroraCurtain.velocityOverLifetime;
        var colorOverLifetime = auroraCurtain.colorOverLifetime;
        
        // 메인 설정 - 큰 파티클들로 완전 커버
        main.startLifetime = coverDuration + 1f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3f);    // 매우 큰 파티클들
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360);
        main.startColor = new Color(auroraColors[0].r, auroraColors[0].g, auroraColors[0].b, curtainOpacity);
        main.maxParticles = waveDensity * 2;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // 높은 방출량
        emission.enabled = true;
        emission.rateOverTime = waveDensity * 1.2f;
        
        // 박스 형태로 완전 둘러쌈
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = effectArea;
        
        // 느린 상승과 회전
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(-30f, 30f); // 회전
        
        // 오로라 커튼 색상 (불투명 유지)
        colorOverLifetime.enabled = true;
        Gradient curtainGradient = new Gradient();
        curtainGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(auroraColors[1], 0.0f),
                new GradientColorKey(auroraColors[2], 0.3f),
                new GradientColorKey(auroraColors[0], 0.6f),
                new GradientColorKey(auroraColors[3], 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(curtainOpacity, 0.1f),     // 빠르게 불투명
                new GradientAlphaKey(curtainOpacity, 0.8f),     // 오래 유지
                new GradientAlphaKey(0.0f, 1.0f)               // 마지막에 사라짐
            }
        );
        colorOverLifetime.color = curtainGradient;
        
        // 머티리얼 적용
        var renderer = auroraCurtain.GetComponent<ParticleSystemRenderer>();
        if (auroraMaterial != null)
        {
            renderer.material = auroraMaterial;
        }
        else
        {
            renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        }
        renderer.sortingOrder = 12;  // 최상위 렌더링
    }
    
    /// <summary>
    /// 오로라 반짝임 설정
    /// </summary>
    private void SetupAuroraSparkleParticles()
    {
        var main = auroraSparkles.main;
        var emission = auroraSparkles.emission;
        var shape = auroraSparkles.shape;
        var velocityOverLifetime = auroraSparkles.velocityOverLifetime;
        
        main.startLifetime = 2f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startColor = new Color(1f, 1f, 1f, 0.9f);
        main.maxParticles = 150;
        
        emission.enabled = false; // 버스트만 사용
        
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = effectArea.magnitude * 0.4f;
        
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        
        var renderer = auroraSparkles.GetComponent<ParticleSystemRenderer>();
        renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        renderer.sortingOrder = 15;
    }
    
    /// <summary>
    /// 오로라 그라디언트 생성
    /// </summary>
    private Gradient CreateAuroraGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(auroraColors[0], 0.0f),
                new GradientColorKey(auroraColors[1], 0.2f),
                new GradientColorKey(auroraColors[2], 0.4f),
                new GradientColorKey(auroraColors[3], 0.6f),
                new GradientColorKey(auroraColors[4], 0.8f),
                new GradientColorKey(auroraColors[0], 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(0.8f, 0.1f),
                new GradientAlphaKey(1.0f, 0.3f),
                new GradientAlphaKey(0.9f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        return gradient;
    }
    
    /// <summary>
    /// 랜덤 오로라 색상 반환
    /// </summary>
    private Color GetRandomAuroraColor()
    {
        return auroraColors[Random.Range(0, auroraColors.Length)];
    }
    
    #region Public API
    
    /// <summary>
    /// 오로라 변신 효과 시작
    /// </summary>
    /// <param name="onTransformCallback">변신 실행 콜백 (오로라가 가려준 상태에서 호출)</param>
    public void StartAuroraTransformation(System.Action onTransformCallback = null)
    {
        if (isTransforming) return;
        
        isTransforming = true;
        transformCoroutine = StartCoroutine(AuroraTransformationSequence(onTransformCallback));
    }
    
    /// <summary>
    /// 변신 효과 중지
    /// </summary>
    public void StopTransformation()
    {
        if (!isTransforming) return;
        
        isTransforming = false;
        
        if (transformCoroutine != null)
        {
            StopCoroutine(transformCoroutine);
        }
        
        auroraWaves.Stop();
        auroraCurtain.Stop();
        auroraSparkles.Stop();
    }
    
    /// <summary>
    /// 효과 영역 크기 설정
    /// </summary>
    public void SetEffectArea(Vector3 newArea)
    {
        effectArea = newArea;
        
        // 파티클 시스템들 업데이트
        var wavesShape = auroraWaves.shape;
        wavesShape.radius = effectArea.x * 0.9f;
        
        var curtainShape = auroraCurtain.shape;
        curtainShape.scale = effectArea;
        
        var sparklesShape = auroraSparkles.shape;
        sparklesShape.radius = effectArea.magnitude * 0.4f;
    }
    
    #endregion
    
    #region Transformation Sequence
    
    /// <summary>
    /// 오로라 변신 시퀀스
    /// </summary>
    private IEnumerator AuroraTransformationSequence(System.Action onTransformCallback)
    {
        Debug.Log("[Aurora] 오로라 변신 시작");
        
        // 1단계: 오로라 상승
        yield return StartCoroutine(AuroraRisePhase());
        
        // 2단계: 오로라 커튼 (완전 가림)
        yield return StartCoroutine(AuroraCurtainPhase(onTransformCallback));
        
        // 3단계: 점진적 소거
        yield return StartCoroutine(AuroraFadePhase());
        
        isTransforming = false;
        Debug.Log("[Aurora] 오로라 변신 완료");
    }
    
    /// <summary>
    /// 1단계: 오로라 상승
    /// </summary>
    private IEnumerator AuroraRisePhase()
    {
        Debug.Log("[Aurora] 오로라 상승 단계");
        
        // 오로라 파도 시작
        auroraWaves.Play();
        
        // 반짝임 효과
        auroraSparkles.Emit(30);
        
        // 오로라 상승 사운드
        if (audioSource != null && auroraRiseSound != null)
        {
            audioSource.PlayOneShot(auroraRiseSound, 0.8f);
        }
        
        // 점진적 강화
        float duration = 1.5f;
        float timer = 0f;
        var emission = auroraWaves.emission;
        float maxRate = emission.rateOverTime.constant;
        
        while (timer < duration)
        {
            float progress = timer / duration;
            emission.rateOverTime = maxRate * progress;
            
            // 중간에 추가 반짝임
            if (progress > 0.5f && Random.Range(0f, 1f) < 0.1f)
            {
                auroraSparkles.Emit(10);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        emission.rateOverTime = maxRate;
        yield return new WaitForSeconds(0.7f);
    }
    
    /// <summary>
    /// 2단계: 오로라 커튼 (실제 변신 실행)
    /// </summary>
    private IEnumerator AuroraCurtainPhase(System.Action onTransformCallback)
    {
        Debug.Log("[Aurora] 오로라 커튼 단계");
        
        // 오로라 커튼 시작 (완전히 가림)
        auroraCurtain.Play();
        
        // 강력한 반짝임 폭발
        auroraSparkles.Emit(80);
        
        // 완전히 가려질 때까지 대기
        yield return new WaitForSeconds(transformDelay);
        
        // ★ 실제 변신 실행 (오로라가 완전히 가려준 상태)
        onTransformCallback?.Invoke();
        
        // 변신 완료 사운드
        if (audioSource != null && transformCompleteSound != null)
        {
            audioSource.PlayOneShot(transformCompleteSound, 0.9f);
        }
        
        // 커튼 유지 시간
        yield return new WaitForSeconds(coverDuration - transformDelay);
    }
    
    /// <summary>
    /// 3단계: 점진적 소거
    /// </summary>
    private IEnumerator AuroraFadePhase()
    {
        Debug.Log("[Aurora] 오로라 소거 단계");
        
        float fadeDuration = 2f;
        float timer = 0f;
        
        var wavesEmission = auroraWaves.emission;
        var curtainEmission = auroraCurtain.emission;
        
        float startWavesRate = wavesEmission.rateOverTime.constant;
        float startCurtainRate = curtainEmission.rateOverTime.constant;
        
        while (timer < fadeDuration)
        {
            float progress = timer / fadeDuration;
            float fadeAmount = Mathf.Lerp(1f, 0f, progress);
            
            wavesEmission.rateOverTime = startWavesRate * fadeAmount;
            curtainEmission.rateOverTime = startCurtainRate * fadeAmount;
            
            // 마지막 반짝임들
            if (progress > 0.7f && Random.Range(0f, 1f) < 0.05f)
            {
                auroraSparkles.Emit(5);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 최종 반짝임 폭발
        auroraSparkles.Emit(20);
        
        // 모든 파티클 정지
        StopTransformation();
        
        yield return new WaitForSeconds(1f);
    }
    
    #endregion
    
    void OnDrawGizmosSelected()
    {
        // 효과 영역 시각화
        Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.3f);
        Gizmos.DrawCube(transform.position, effectArea);
        
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 1f);
        Gizmos.DrawWireCube(transform.position, effectArea);
        
    }
} 