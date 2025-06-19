using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 의태식물 스타일 변신 연출 - 뿌연 연기로 가리고 자연스럽게 변신
/// </summary>
public class MimicTransformationEffect : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem dustCloudParticle;      // 메인 먼지 구름
    [SerializeField] private ParticleSystem denseCloudParticle;     // 밀도 있는 가림막
    [SerializeField] private ParticleSystem groundDustParticle;     // 지면 먼지
    [SerializeField] private ParticleSystem sparkParticle;         // 작은 반짝임
    
    [Header("Smoke Materials")]
    [SerializeField] private Material dustMaterial;                // 먼지 머티리얼
    [SerializeField] private Material denseSmokeReference;         // 기존 연기 머티리얼 참조
    
    [Header("Transformation Settings")]
    [SerializeField] private Vector3 effectArea = new Vector3(4f, 3f, 4f);
    [SerializeField] private float effectDuration = 4f;
    [SerializeField] private float coverDuration = 2f;            // 완전히 가려지는 시간
    [SerializeField] private float transformDelay = 1.5f;        // 가려진 후 변신까지의 시간
    
    [Header("Visual Settings")]
    [SerializeField] private Color dustColor = new Color(0.8f, 0.7f, 0.6f, 0.8f);  // 자연스러운 먼지색
    [SerializeField] private Color denseColor = new Color(0.6f, 0.5f, 0.4f, 1f);   // 밀도 있는 구름색
    [SerializeField] private int dustDensity = 400;              // 먼지 밀도
    [SerializeField] private float dustSpeed = 1.5f;             // 먼지 상승 속도
    [SerializeField] private bool addWindEffect = true;          // 바람 효과
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dustRiseSound;           // 먼지 올라가는 소리
    [SerializeField] private AudioClip transformCompleteSound;  // 변신 완료 소리
    
    private bool isTransforming = false;
    private Coroutine transformCoroutine;
    
    void Start()
    {
        InitializeParticleSystems();
    }
    
    private void InitializeParticleSystems()
    {
        // 먼지 구름 파티클 시스템 생성
        if (dustCloudParticle == null)
        {
            GameObject dustObj = new GameObject("DustCloud");
            dustObj.transform.SetParent(transform);
            dustCloudParticle = dustObj.AddComponent<ParticleSystem>();
        }
        
        // 밀도 있는 구름 파티클 시스템 생성
        if (denseCloudParticle == null)
        {
            GameObject denseObj = new GameObject("DenseCloud");
            denseObj.transform.SetParent(transform);
            denseCloudParticle = denseObj.AddComponent<ParticleSystem>();
        }
        
        // 지면 먼지 파티클 시스템 생성
        if (groundDustParticle == null)
        {
            GameObject groundObj = new GameObject("GroundDust");
            groundObj.transform.SetParent(transform);
            groundDustParticle = groundObj.AddComponent<ParticleSystem>();
        }
        
        // 반짝임 파티클 시스템 생성
        if (sparkParticle == null)
        {
            GameObject sparkObj = new GameObject("SparkEffect");
            sparkObj.transform.SetParent(transform);
            sparkParticle = sparkObj.AddComponent<ParticleSystem>();
        }
        
        SetupDustCloudParticles();
        SetupDenseCloudParticles();
        SetupGroundDustParticles();
        SetupSparkParticles();
    }
    
    /// <summary>
    /// 메인 먼지 구름 설정 - 자연스럽게 위로 올라가는 효과
    /// </summary>
    private void SetupDustCloudParticles()
    {
        var main = dustCloudParticle.main;
        var emission = dustCloudParticle.emission;
        var shape = dustCloudParticle.shape;
        var velocityOverLifetime = dustCloudParticle.velocityOverLifetime;
        var sizeOverLifetime = dustCloudParticle.sizeOverLifetime;
        var colorOverLifetime = dustCloudParticle.colorOverLifetime;
        var noise = dustCloudParticle.noise;
        
        // 메인 설정
        main.startLifetime = effectDuration;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, dustSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360);
        main.startColor = dustColor;
        main.maxParticles = dustDensity;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // 방출 설정
        emission.enabled = true;
        emission.rateOverTime = dustDensity * 0.7f;
        
        // 모양 설정 - 원형으로 바닥에서 시작
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectArea.x * 0.8f;
        shape.position = Vector3.down * (effectArea.y * 0.4f);
        
        // 위로 올라가는 속도
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(dustSpeed, dustSpeed * 2f);
        if (addWindEffect)
        {
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        }
        
        // 크기 증가 (위로 올라갈수록 커짐)
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 1.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // 색상 변화 (점점 투명해짐)
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(dustColor, 0.0f),
                new GradientColorKey(dustColor, 0.8f),
                new GradientColorKey(dustColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(dustColor.a, 0.1f),
                new GradientAlphaKey(dustColor.a * 0.8f, 0.6f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // 노이즈 효과 (자연스러운 움직임)
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.5f;
        
        // 머티리얼 적용
        var renderer = dustCloudParticle.GetComponent<ParticleSystemRenderer>();
        if (dustMaterial != null)
        {
            renderer.material = dustMaterial;
        }
        else
        {
            // 기본 연기 머티리얼 사용
            renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        }
        renderer.sortingOrder = 5;
    }
    
    /// <summary>
    /// 밀도 있는 구름 설정 - 완전히 가리는 효과
    /// </summary>
    private void SetupDenseCloudParticles()
    {
        var main = denseCloudParticle.main;
        var emission = denseCloudParticle.emission;
        var shape = denseCloudParticle.shape;
        var velocityOverLifetime = denseCloudParticle.velocityOverLifetime;
        var colorOverLifetime = denseCloudParticle.colorOverLifetime;
        
        // 메인 설정 - 큰 파티클들로 완전 커버
        main.startLifetime = coverDuration + 1f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 2f);    // 큰 파티클들
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360);
        main.startColor = denseColor;
        main.maxParticles = dustDensity * 2;                         // 높은 밀도
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // 높은 방출량
        emission.enabled = true;
        emission.rateOverTime = dustDensity * 1.5f;
        
        // 박스 형태로 완전 커버
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = effectArea;
        
        // 느린 움직임
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.1f);
        
        // 불투명 유지
        colorOverLifetime.enabled = true;
        Gradient denseGradient = new Gradient();
        denseGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(denseColor, 0.0f),
                new GradientColorKey(denseColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.15f),      // 빠르게 불투명
                new GradientAlphaKey(1.0f, 0.7f),       // 오래 유지
                new GradientAlphaKey(0.0f, 1.0f)        // 마지막에 사라짐
            }
        );
        colorOverLifetime.color = denseGradient;
        
        // 머티리얼 적용
        var renderer = denseCloudParticle.GetComponent<ParticleSystemRenderer>();
        if (dustMaterial != null)
        {
            renderer.material = dustMaterial;
        }
        else
        {
            renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        }
        renderer.sortingOrder = 10;  // 최상위 렌더링
    }
    
    /// <summary>
    /// 지면 먼지 설정
    /// </summary>
    private void SetupGroundDustParticles()
    {
        var main = groundDustParticle.main;
        var emission = groundDustParticle.emission;
        var shape = groundDustParticle.shape;
        var velocityOverLifetime = groundDustParticle.velocityOverLifetime;
        
        main.startLifetime = 1.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startColor = new Color(0.7f, 0.6f, 0.5f, 0.6f);
        main.maxParticles = 100;
        
        emission.enabled = false; // 버스트만 사용
        
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectArea.x * 0.6f;
        shape.position = Vector3.down * (effectArea.y * 0.5f);
        
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f, 3f);
        
        var renderer = groundDustParticle.GetComponent<ParticleSystemRenderer>();
        if (dustMaterial != null)
        {
            renderer.material = dustMaterial;
        }
        else
        {
            renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        }
        renderer.sortingOrder = 3;
    }
    
    /// <summary>
    /// 반짝임 설정
    /// </summary>
    private void SetupSparkParticles()
    {
        var main = sparkParticle.main;
        var emission = sparkParticle.emission;
        var shape = sparkParticle.shape;
        
        main.startLifetime = 1f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.startColor = new Color(1f, 0.9f, 0.7f, 0.8f);
        main.maxParticles = 50;
        
        emission.enabled = false; // 버스트만 사용
        
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = effectArea.magnitude * 0.3f;
        
        var renderer = sparkParticle.GetComponent<ParticleSystemRenderer>();
        renderer.material = Resources.GetBuiltinResource<Material>("Default-ParticleSystem.mat");
        renderer.sortingOrder = 8;
    }
    
    #region Public API
    
    /// <summary>
    /// 의태식물 스타일 변신 시작
    /// </summary>
    /// <param name="onTransformCallback">변신 실행 콜백 (파티클이 가려준 상태에서 호출)</param>
    public void StartMimicTransformation(System.Action onTransformCallback = null)
    {
        if (isTransforming) return;
        
        isTransforming = true;
        transformCoroutine = StartCoroutine(MimicTransformationSequence(onTransformCallback));
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
        
        dustCloudParticle.Stop();
        denseCloudParticle.Stop();
        groundDustParticle.Stop();
        sparkParticle.Stop();
    }
    
    /// <summary>
    /// 효과 영역 크기 설정
    /// </summary>
    public void SetEffectArea(Vector3 newArea)
    {
        effectArea = newArea;
        
        // 파티클 시스템들 업데이트
        var dustShape = dustCloudParticle.shape;
        dustShape.radius = effectArea.x * 0.8f;
        
        var denseShape = denseCloudParticle.shape;
        denseShape.scale = effectArea;
        
        var groundShape = groundDustParticle.shape;
        groundShape.radius = effectArea.x * 0.6f;
        
        var sparkShape = sparkParticle.shape;
        sparkShape.radius = effectArea.magnitude * 0.3f;
    }
    
    #endregion
    
    #region Transformation Sequence
    
    /// <summary>
    /// 의태식물 변신 시퀀스
    /// </summary>
    private IEnumerator MimicTransformationSequence(System.Action onTransformCallback)
    {
        Debug.Log("[MimicTransform] 변신 시작");
        
        // 1단계: 지면 먼지 효과
        yield return StartCoroutine(GroundDustPhase());
        
        // 2단계: 먼지 구름 상승
        yield return StartCoroutine(DustRisePhase());
        
        // 3단계: 밀도 있는 커버 (완전 가림)
        yield return StartCoroutine(DenseCoverPhase(onTransformCallback));
        
        // 4단계: 점진적 소거
        yield return StartCoroutine(ClearingPhase());
        
        isTransforming = false;
        Debug.Log("[MimicTransform] 변신 완료");
    }
    
    /// <summary>
    /// 1단계: 지면 먼지 효과
    /// </summary>
    private IEnumerator GroundDustPhase()
    {
        Debug.Log("[MimicTransform] 지면 먼지 단계");
        
        // 지면 먼지 버스트
        groundDustParticle.Emit(30);
        
        // 약간의 반짝임
        sparkParticle.Emit(10);
        
        // 사운드 재생
        if (audioSource != null && dustRiseSound != null)
        {
            audioSource.PlayOneShot(dustRiseSound, 0.7f);
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    /// <summary>
    /// 2단계: 먼지 구름 상승
    /// </summary>
    private IEnumerator DustRisePhase()
    {
        Debug.Log("[MimicTransform] 먼지 상승 단계");
        
        // 먼지 구름 시작
        dustCloudParticle.Play();
        
        // 점진적 강화
        float duration = 1f;
        float timer = 0f;
        var emission = dustCloudParticle.emission;
        float maxRate = emission.rateOverTime.constant;
        
        while (timer < duration)
        {
            float progress = timer / duration;
            emission.rateOverTime = maxRate * progress;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        emission.rateOverTime = maxRate;
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 3단계: 밀도 있는 커버 (실제 변신 실행)
    /// </summary>
    private IEnumerator DenseCoverPhase(System.Action onTransformCallback)
    {
        Debug.Log("[MimicTransform] 밀도 커버 단계");
        
        // 밀도 있는 구름 시작
        denseCloudParticle.Play();
        
        // 추가 지면 먼지
        groundDustParticle.Emit(50);
        
        // 반짝임 효과
        sparkParticle.Emit(20);
        
        // 완전히 가려질 때까지 대기
        yield return new WaitForSeconds(transformDelay);
        
        // ★ 실제 변신 실행 (파티클이 완전히 가려준 상태)
        onTransformCallback?.Invoke();
        
        // 변신 완료 사운드
        if (audioSource != null && transformCompleteSound != null)
        {
            audioSource.PlayOneShot(transformCompleteSound, 0.8f);
        }
        
        // 커버 유지 시간
        yield return new WaitForSeconds(coverDuration - transformDelay);
    }
    
    /// <summary>
    /// 4단계: 점진적 소거
    /// </summary>
    private IEnumerator ClearingPhase()
    {
        Debug.Log("[MimicTransform] 소거 단계");
        
        float clearDuration = 1.5f;
        float timer = 0f;
        
        var dustEmission = dustCloudParticle.emission;
        var denseEmission = denseCloudParticle.emission;
        
        float startDustRate = dustEmission.rateOverTime.constant;
        float startDenseRate = denseEmission.rateOverTime.constant;
        
        while (timer < clearDuration)
        {
            float progress = timer / clearDuration;
            float fadeAmount = Mathf.Lerp(1f, 0f, progress);
            
            dustEmission.rateOverTime = startDustRate * fadeAmount;
            denseEmission.rateOverTime = startDenseRate * fadeAmount;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 마지막 반짝임
        sparkParticle.Emit(15);
        
        // 모든 파티클 정지
        StopTransformation();
        
        yield return new WaitForSeconds(1f);
    }
    
    #endregion
    
    void OnDrawGizmosSelected()
    {
        // 효과 영역 시각화
        Gizmos.color = new Color(0.8f, 0.7f, 0.6f, 0.3f);
        Gizmos.DrawCube(transform.position, effectArea);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, effectArea);
        
        // 지면 먼지 영역
        Vector3 groundPos = transform.position + Vector3.down * (effectArea.y * 0.4f);
    }
} 