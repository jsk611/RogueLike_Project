using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 몬스터별 캡슐 변신 정보
/// </summary>
[System.Serializable]
public class MonsterCapsuleData
{
    public float radius = 3f;           // 캡슐 반지름
    public float height = 6f;           // 캡슐 총 높이
    public Vector3 scale = Vector3.one; // 추가 스케일링
    public float transformTime = 1.5f;  // 변신 시간
    
    [Header("Capsule Orientation")]
    public Vector3 direction = Vector3.up;      // 캡슐 형성 방향 (기본: 위쪽)
    public Vector3 forwardAxis = Vector3.forward; // 캡슐 앞면 방향 (회전 기준)
    
    [Header("Fog Effects")]
    public bool enableFogEffect = true;         // 안개 효과 사용 여부
    public Color fogColor = Color.cyan;         // 안개 색상
    public float fogDensity = 0.5f;             // 안개 밀도
    public float fogFadeTime = 1.2f;            // 안개 페이드아웃 시간
}

/// <summary>
/// 🔄 사이버 변신 공간 시스템 (Cyber Transformation Space)
/// 
/// ▶ 시스템 개요:
/// 복셀(Voxel) 오브젝트들을 활용한 3단계 변신 시퀀스로 몬스터를 극적으로 등장시키는 시스템
/// 
/// ▶ 변신 단계:
/// 【1단계】 구형 집결 (Sphere Formation)
///   - Fibonacci-sphere 알고리즘으로 복셀들을 구 표면에 균등 분포 배치
///   - 원래 위치에서 구 표면으로 부드러운 이동 애니메이션
///   - 설정 가능한 구 반지름과 형성 시간
/// 
/// 【2단계】 캡슐 변형 (Capsule Transformation)
///   - 구 형태에서 캡슐(원통 + 상하 반구) 형태로 3D 맵핑 변환
///   - 복셀들이 캡슐 표면을 따라 재배치되며 법선 방향으로 회전
///   - 동적으로 조절 가능한 캡슐 높이와 반지름
/// 
/// 【3단계】 순차 해체 & 몬스터 등장 (Dissolve & Monster Reveal)
///   - Y축 상단부터 하단으로 순차적 해체 (연속 0.015초 간격)
///   - 해체 과정: 위치 이동 + 크기 축소 + 알파 페이드아웃
///   - 상단 30% 해체 시점에 몬스터 등장 (스케일링 + 위치 보간)
///   - 완료 후 자동 원상 복귀 옵션
/// 
/// ▶ 주요 특징:
/// - 실시간 파라미터 조정 가능 (인스펙터 노출)
/// - 수학적 정확도: Fibonacci 나선, 캡슐 기하학 활용
/// - 부드러운 애니메이션: AnimationCurve 기반 알파 보간
/// - 디버그 모드: U키 실행, 자동 시작 옵션
/// </summary>
public class CyberTransformationSpace : MonoBehaviour
{
    // ------------------------------------------------------------------
    // ✨ 인스펙터 노출 변수
    // ------------------------------------------------------------------

    [Header("Voxel Source")]
    [SerializeField] private Transform voxelRoot;                  // 큐브 모음 루트 (없으면 자기 자신)

    [Header("Sphere Formation")]
    [SerializeField] private float sphereRadius = 4f;              // 구 반지름
    [SerializeField] private float sphereFormationTime = 2f;       // 구형태로 뭉치는 시간

    [Header("Capsule Settings")]
    [SerializeField] private MonsterCapsuleData defaultCapsule;    // 기본 캡슐 설정

    [Header("Dissolve")]
    [SerializeField] private float dissolveTimePerVoxel = 0.6f;    // 개별 큐브 해체 시간
    [SerializeField]
    private AnimationCurve dissolveAlpha =        // 알파 변곡
        AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Monster Reveal")]
    [SerializeField] private GameObject monster;                   // 등장할 몬스터 오브젝트
    [SerializeField] private float monsterRevealTime = 1.4f;       // 몬스터 등장 시간

    [Header("Fog Effects")]
    [SerializeField] private GameObject fogParticlePrefab;         // 안개 파티클 프리팹
    [SerializeField] private Material fogMaterial;                // 안개 전용 머티리얼 (CyberFogMaterial 권장)
    [SerializeField] private bool enableCustomFogShape = true;    // 캡슐 모양 안개 사용
    [SerializeField] private bool useVolumetricShader = true;     // 볼류메트릭 셰이더 사용 여부
    [SerializeField] private bool enableCapsuleMask = true;       // 캡슐 메시 마스크 사용 여부

    [Header("Debug")]
    [SerializeField] private bool autoStart = true;                // 자동 실행 여부

    // ------------------------------------------------------------------
    // 🔒 내부 상태
    // ------------------------------------------------------------------

    private readonly List<Transform> voxels = new();               // 큐브 Transform 목록
    private readonly Dictionary<Transform, Vector3> originPos =    // 원래 위치 백업
        new();

    private Vector3[] spherePos;                                   // 구 표면 위치
    private Vector3[] capsulePos;                                  // 캡슐 표면 위치
    private MonsterCapsuleData currentCapsule;                     // 현재 변신할 캡슐 데이터
    private bool isBusy;

    // 안개 효과 관련
    private ParticleSystem fogParticleSystem;                      // 안개 파티클 시스템
    [SerializeField] private GameObject fogContainer;              // 안개 컨테이너 오브젝트
    private bool fogActive = false;                                // 안개 활성 상태
    private Coroutine fogTransformCoroutine;                       // 안개 변형 코루틴

    //-------------------------------------------------------------------
    // 🏁 초기화
    //-------------------------------------------------------------------

    void Awake()
    {
        if (voxelRoot == null) voxelRoot = transform;
        CacheVoxels();
        
        // 기본 캡슐 데이터 초기화
        if (defaultCapsule == null)
        {
            defaultCapsule = new MonsterCapsuleData();
        }
        currentCapsule = defaultCapsule;
        PrecomputeTargets(currentCapsule);
        
        if (monster) monster.SetActive(false);
    }

    void Start()
    {
        if (autoStart) StartTransformation();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
            StartTransformation();

        if (Input.GetKeyDown(KeyCode.R))
            ResetToOriginal();
    }

    //-------------------------------------------------------------------
    // 📦 큐브 수집 및 목표 위치 선계산
    //-------------------------------------------------------------------

    private void CacheVoxels()
    {
        voxels.Clear();
        foreach (Transform t in voxelRoot.GetComponentsInChildren<Transform>())
        {
            if (t != voxelRoot)
            {
                voxels.Add(t);
                originPos[t] = t.localPosition;
            }
        }
    }

    private void PrecomputeTargets(MonsterCapsuleData capsuleData)
    {
        int n = voxels.Count;
        spherePos = new Vector3[n];
        capsulePos = new Vector3[n];

        // Fibonacci‑sphere로 균등 분포 좌표 계산
        for (int i = 0; i < n; i++)
        {
            float k = (i + 0.5f) / n;
            float theta = Mathf.Acos(1 - 2 * k);
            float phi = Mathf.PI * (1 + Mathf.Sqrt(5)) * i;

            Vector3 dir = new(
                Mathf.Sin(theta) * Mathf.Cos(phi),
                Mathf.Cos(theta),
                Mathf.Sin(theta) * Mathf.Sin(phi));

            // ① 구 표면 위치
            spherePos[i] = dir * sphereRadius;

            // ② 캡슐 표면 위치 계산 (원통 + 반구) - 전달받은 데이터 사용
            Vector3 p = dir * capsuleData.radius;
            float halfCyl = capsuleData.height * 0.5f - capsuleData.radius;
            p.y = dir.y * halfCyl;

            if (Mathf.Abs(p.y) > halfCyl)               // 반구 영역 보정
            {
                float sign = Mathf.Sign(p.y);
                Vector3 capCenter = new(0, sign * halfCyl, 0);
                Vector3 radial = new Vector3(p.x, 0, p.z).normalized * capsuleData.radius;
                p = capCenter + radial;
            }
            
            // 캡슐 방향 회전 적용
            p = RotateCapsulePosition(p, capsuleData.direction, capsuleData.forwardAxis);
            
            // 추가 스케일 적용
            p = Vector3.Scale(p, capsuleData.scale);
            capsulePos[i] = p;
        }
    }

    /// <summary>
    /// 캡슐 위치를 지정된 방향으로 회전
    /// </summary>
    private Vector3 RotateCapsulePosition(Vector3 position, Vector3 targetDirection, Vector3 forwardAxis)
    {
        // 기본 방향 (Y축 위쪽)에서 목표 방향으로의 회전 계산
        Vector3 defaultDirection = Vector3.up;
        targetDirection = targetDirection.normalized;
        
        // 방향이 같으면 회전하지 않음
        if (Vector3.Dot(defaultDirection, targetDirection) > 0.99f)
            return position;
            
        // 기본 방향에서 목표 방향으로의 회전 쿼터니언 생성
        Quaternion rotation = Quaternion.FromToRotation(defaultDirection, targetDirection);
        
        // 추가 회전축 고려 (forwardAxis가 지정된 경우)
        if (forwardAxis != Vector3.forward && forwardAxis.sqrMagnitude > 0.1f)
        {
            Vector3 currentForward = rotation * Vector3.forward;
            Vector3 desiredForward = forwardAxis.normalized;
            
            // 목표 방향 축 주위로 추가 회전
            Vector3 axis = targetDirection;
            float angle = Vector3.SignedAngle(currentForward, desiredForward, axis);
            Quaternion additionalRotation = Quaternion.AngleAxis(angle, axis);
            
            rotation = additionalRotation * rotation;
        }
        
        // 위치에 회전 적용
        return rotation * position;
    }

    //-------------------------------------------------------------------
    // 🌫️ 안개 효과 시스템
    //-------------------------------------------------------------------

    /// <summary>
    /// 캡슐 내부 안개 효과 생성 및 컨테이너 설정
    /// </summary>
    private void CreateCapsuleFog()
    {
        if (!currentCapsule.enableFogEffect) return;

        // fogContainer가 설정되어 있는지 확인
        if (fogContainer == null)
        {
            Debug.LogWarning("[CyberTransformationSpace] FogContainer가 설정되지 않았습니다. 자동 생성합니다.");
            fogContainer = new GameObject("CapsuleFog");
            fogContainer.transform.SetParent(transform);
        }

        // fogContainer가 비활성화되어 있으면 활성화
        if (!fogContainer.activeInHierarchy)
        {
            fogContainer.SetActive(true);
        }

        // fogContainer 위치와 크기 설정
        ConfigureFogContainer();

        // 파티클 시스템 생성
        if (fogParticleSystem == null)
        {
            if (fogParticlePrefab != null)
            {
                GameObject fogObj = Instantiate(fogParticlePrefab, fogContainer.transform);
                fogParticleSystem = fogObj.GetComponent<ParticleSystem>();
            }
            else
            {
                // 기본 파티클 시스템 생성
                GameObject fogObj = new GameObject("FogParticles");
                fogObj.transform.SetParent(fogContainer.transform);
                fogObj.transform.localPosition = Vector3.zero;
                fogParticleSystem = fogObj.AddComponent<ParticleSystem>();
            }
        }

        ConfigureFogParticleSystem();
    }

    /// <summary>
    /// FogContainer(캡슐 마스크)의 위치와 크기를 캡슐 데이터에 맞춰 설정
    /// </summary>
    private void ConfigureFogContainer()
    {
        if (fogContainer == null) return;

        // 위치 설정 - 변신 중심점에 위치
        fogContainer.transform.position = transform.position;
        fogContainer.transform.localPosition = Vector3.zero;

        // 회전 초기화 (구형 단계에서는 회전 없음)
        fogContainer.transform.localRotation = Quaternion.identity;

        // CapsuleData 기반 스케일 설정
        UpdateFogContainerScale();

        Debug.Log($"[CyberTransformationSpace] FogContainer 설정 완료 - 위치: {fogContainer.transform.position}");
    }

    /// <summary>
    /// FogContainer(캡슐 마스크)의 스케일을 캡슐 데이터에 맞춰 업데이트
    /// </summary>
    private void UpdateFogContainerScale()
    {
        if (!enableCapsuleMask || fogContainer == null) return;

        // CapsuleData 기반 스케일 (Unity Capsule의 기본 크기는 반지름 0.5, 높이 2)
        float diameterX = currentCapsule.radius * 2f * currentCapsule.scale.x;
        float diameterZ = currentCapsule.radius * 2f * currentCapsule.scale.z;
        float heightY = currentCapsule.height * currentCapsule.scale.y;

        fogContainer.transform.localScale = new Vector3(diameterX, heightY * 0.5f, diameterZ);
        
        // direction으로 회전 (초기에는 회전 없음, 나중에 캡슐 변형시 적용)
        fogContainer.transform.localRotation = Quaternion.identity;

        Debug.Log($"[CyberTransformationSpace] FogContainer 스케일 업데이트 - 크기: {fogContainer.transform.localScale}");
    }

    /// <summary>
    /// FogContainer의 목표 스케일 계산 (캡슐 데이터 기반)
    /// </summary>
    private Vector3 GetTargetFogContainerScale()
    {
        // CapsuleData 기반 스케일 (Unity Capsule의 기본 크기는 반지름 0.5, 높이 2)
        float diameterX = currentCapsule.radius * 2f * currentCapsule.scale.x;
        float diameterZ = currentCapsule.radius * 2f * currentCapsule.scale.z;
        float heightY = currentCapsule.height * currentCapsule.scale.y;

        return new Vector3(diameterX, heightY * 0.5f, diameterZ);
    }

    /// <summary>
    /// 안개 파티클 시스템 기본 설정 (공통 설정만)
    /// </summary>
    private void ConfigureFogParticleSystem()
    {
        if (fogParticleSystem == null) return;

        var main = fogParticleSystem.main;
        var velocityOverLifetime = fogParticleSystem.velocityOverLifetime;
        var colorOverLifetime = fogParticleSystem.colorOverLifetime;

        // 메인 설정 (공통)
        main.startLifetime = 3f;
        main.startSpeed = 0.2f;
        main.startSize = 0.8f;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // 속도 설정 (안개가 천천히 움직임)
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        
        AnimationCurve velocityCurve = new AnimationCurve();
        velocityCurve.AddKey(0f, 0f);
        velocityCurve.AddKey(1f, 0.1f);
        
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f, velocityCurve);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, velocityCurve);

        // 색상 및 투명도 변화 (기본 그라데이션)
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(currentCapsule.fogColor, 0.0f), 
                new GradientColorKey(currentCapsule.fogColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(currentCapsule.fogDensity, 0.3f),
                new GradientAlphaKey(currentCapsule.fogDensity * 0.8f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;

        // 머티리얼 적용 및 셰이더 프로퍼티 설정
        ConfigureFogMaterial();
        
        Debug.Log("[CyberTransformationSpace] 안개 파티클 시스템 기본 설정 완료");
    }

    /// <summary>
    /// 안개 머티리얼의 셰이더 프로퍼티 설정
    /// </summary>
    private void ConfigureFogMaterial()
    {
        if (fogMaterial == null) return;

        var renderer = fogParticleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer == null) return;

        // 머티리얼 인스턴스 생성 (원본 보호)
        Material fogMatInstance = new Material(fogMaterial);
        renderer.material = fogMatInstance;

        // 볼류메트릭 셰이더 프로퍼티 설정
        if (useVolumetricShader && fogMatInstance.HasProperty("_FogColor"))
        {
            // 기본 안개 색상 및 밀도
            fogMatInstance.SetColor("_FogColor", currentCapsule.fogColor);
            fogMatInstance.SetFloat("_Density", currentCapsule.fogDensity);
            
            // 방출 색상 (더 밝은 색상으로)
            Color emissionColor = currentCapsule.fogColor * 1.5f;
            emissionColor.a = 1f;
            if (fogMatInstance.HasProperty("_EmissionColor"))
                fogMatInstance.SetColor("_EmissionColor", emissionColor);

            // 사이버 효과 강도 조절
            if (fogMatInstance.HasProperty("_PulseIntensity"))
                fogMatInstance.SetFloat("_PulseIntensity", 0.2f + currentCapsule.fogDensity * 0.3f);
            
            if (fogMatInstance.HasProperty("_FlickerIntensity"))
                fogMatInstance.SetFloat("_FlickerIntensity", 0.05f + currentCapsule.fogDensity * 0.1f);
                
            // 투명도 설정
            if (fogMatInstance.HasProperty("_Alpha"))
                fogMatInstance.SetFloat("_Alpha", currentCapsule.fogDensity * 0.8f);
        }
        else
        {
            // 기본 파티클 셰이더의 경우
            if (fogMatInstance.HasProperty("_Color"))
                fogMatInstance.SetColor("_Color", currentCapsule.fogColor);
            if (fogMatInstance.HasProperty("_TintColor"))
                fogMatInstance.SetColor("_TintColor", currentCapsule.fogColor);
        }

        Debug.Log($"[CyberTransformationSpace] 안개 머티리얼 설정 완료 - 색상: {currentCapsule.fogColor}, 밀도: {currentCapsule.fogDensity}");
    }

    /// <summary>
    /// 캡슐 내부 크기 계산 (모든 캡슐 데이터 반영)
    /// </summary>
    private Vector3 CalculateCapsuleInnerSize()
    {
        // 기본 내부 크기 계산
        float innerRadius = currentCapsule.radius * 0.7f; // 내부 공간 (30% 여백)
        float innerHeight = currentCapsule.height * 0.8f; // 내부 높이 (20% 여백)
        
        // 기본 박스 크기
        Vector3 baseSize = new Vector3(innerRadius * 2f, innerHeight, innerRadius * 2f);
        
        // 캡슐의 스케일 적용
        Vector3 scaledSize = Vector3.Scale(baseSize, currentCapsule.scale);
        
        Debug.Log($"[CyberTransformationSpace] 캡슐 내부 크기 계산 - 기본: {baseSize}, 스케일 적용 후: {scaledSize}");
        
        return scaledSize;
    }

    /// <summary>
    /// 안개 효과 활성화 (구형부터 시작)
    /// </summary>
    private IEnumerator ActivateFog()
    {
        if (!currentCapsule.enableFogEffect) yield break;

        CreateCapsuleFog();
        
        if (fogParticleSystem != null)
        {
            // 구형 모양으로 시작 (투명도 낮게)
            ConfigureFogForSphere();
            
            fogParticleSystem.gameObject.SetActive(true);
            fogParticleSystem.Play();
            fogActive = true;
            
            Debug.Log("[CyberTransformationSpace] 안개 효과 활성화 (구형 단계)");
        }
        
        yield return null;
    }

    /// <summary>
    /// 구형 단계 안개 설정 (캡슐 크기 기반)
    /// </summary>
    private void ConfigureFogForSphere()
    {
        if (fogParticleSystem == null) return;

        var shape = fogParticleSystem.shape;
        var main = fogParticleSystem.main;
        var emission = fogParticleSystem.emission;

        // 구형 모양 설정 - 캡슐 데이터 기반 크기 계산
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        
        // 캡슐 크기를 고려한 구형 반지름 (캡슐 반지름과 높이의 평균 사용)
        float capsuleBasedRadius = (currentCapsule.radius + currentCapsule.height * 0.3f) * 0.7f;
        float scaleFactor = Mathf.Max(currentCapsule.scale.x, currentCapsule.scale.z); // X, Z 중 큰 값 사용
        shape.radius = capsuleBasedRadius * scaleFactor;
        shape.radiusThickness = 0.8f; // 가장자리 중심

        // 초기 투명도 낮게 (서서히 나타나도록)
        Color startColor = currentCapsule.fogColor;
        startColor.a *= 0.3f; // 30% 투명도로 시작
        main.startColor = startColor;
        
        // 방출량 낮게 시작 (캡슐 크기에 비례)
        float sizeMultiplier = (currentCapsule.radius * currentCapsule.height) / (3f * 6f); // 기본 크기 대비
        emission.rateOverTime = 15f * currentCapsule.fogDensity * sizeMultiplier;

        // fogContainer 구형 크기 및 위치 업데이트
        if (fogContainer != null && enableCapsuleMask)
        {
            fogContainer.transform.localPosition = Vector3.zero;
            fogContainer.transform.localRotation = Quaternion.identity;
            
            // 구형 단계에서는 구 반지름에 맞춘 크기
            float sphereRadius = capsuleBasedRadius * scaleFactor;
            Vector3 sphereScale = Vector3.one * sphereRadius;
            fogContainer.transform.localScale = sphereScale;
            
            Debug.Log($"[CyberTransformationSpace] FogContainer 구형 변형 - 크기: {sphereScale}");
        }
            
        Debug.Log($"[CyberTransformationSpace] 구형 안개 크기: {shape.radius}, 마스크 크기: {fogContainer?.transform.localScale}");
    }

    /// <summary>
    /// 안개를 캡슐 모양으로 변형 (캡슐 변형과 동시에) - 캡슐 크기 연동
    /// </summary>
    private IEnumerator TransformFogWithCapsule()
    {
        if (!fogActive || fogParticleSystem == null) yield break;

        float transformTime = currentCapsule.transformTime;
        float elapsed = 0f;
        
        var shape = fogParticleSystem.shape;
        var main = fogParticleSystem.main;
        var emission = fogParticleSystem.emission;
        
        // 시작 값들 (현재 구형 상태 기반)
        float startRadius = (currentCapsule.radius + currentCapsule.height * 0.3f) * 0.7f;
        float scaleFactor = Mathf.Max(currentCapsule.scale.x, currentCapsule.scale.z);
        startRadius *= scaleFactor;
        
        Color startColor = currentCapsule.fogColor;
        startColor.a *= 0.3f;
        
        float sizeMultiplier = (currentCapsule.radius * currentCapsule.height) / (3f * 6f);
        float startEmissionRate = 15f * currentCapsule.fogDensity * sizeMultiplier;
        
        // 목표 값들 (캡슐 크기 기반)
        Vector3 targetCapsuleSize = CalculateCapsuleInnerSize();
        Color targetColor = currentCapsule.fogColor;
        float targetEmissionRate = 30f * currentCapsule.fogDensity * sizeMultiplier;
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, currentCapsule.direction);

        // fogContainer의 시작값과 목표값
        Vector3 startContainerScale = fogContainer?.transform.localScale ?? Vector3.one;
        Vector3 targetContainerScale = GetTargetFogContainerScale();
        Quaternion startContainerRotation = fogContainer?.transform.localRotation ?? Quaternion.identity;
        Quaternion targetContainerRotation = Quaternion.FromToRotation(Vector3.up, currentCapsule.direction);

        Debug.Log($"[CyberTransformationSpace] 안개 캡슐 변형 시작 - 시작반지름: {startRadius}, 목표크기: {targetCapsuleSize}");

        while (elapsed < transformTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transformTime;
            
            // 구형에서 박스(캡슐 내부)로 모양 변경
            if (progress > 0.3f && shape.shapeType == ParticleSystemShapeType.Sphere)
            {
                shape.shapeType = ParticleSystemShapeType.Box;
                Debug.Log("[CyberTransformationSpace] 안개 모양 변경: 구형 → 박스");
            }
            
            if (shape.shapeType == ParticleSystemShapeType.Box)
            {
                // 박스 크기 점진적 변경 (캡슐 크기 반영)
                Vector3 startBoxSize = Vector3.one * startRadius * 2f;
                Vector3 currentSize = Vector3.Lerp(startBoxSize, targetCapsuleSize, progress);
                shape.scale = currentSize;
                
                // fogContainer 크기와 회전 실시간 업데이트
                if (fogContainer != null && enableCapsuleMask)
                {
                    // 크기 적용 (파티클 스케일에 맞춤)
                    Vector3 containerScale = currentSize / 6f; // 6은 기본 박스 크기
                    fogContainer.transform.localScale = containerScale;
                    
                    // 회전 적용
                    fogContainer.transform.localRotation = Quaternion.Lerp(
                        Quaternion.identity, 
                        targetRotation, 
                        progress
                    );
                }
            }
            else
            {
                // 구형 단계에서 크기만 조절 (캡슐 크기 고려)
                float currentRadius = Mathf.Lerp(startRadius, startRadius * 1.2f, progress);
                shape.radius = currentRadius;
                
                // fogContainer 구형 크기 실시간 업데이트
                if (fogContainer != null && enableCapsuleMask)
                {
                    Vector3 sphereScale = Vector3.one * currentRadius;
                    fogContainer.transform.localScale = sphereScale;
                }
            }
            
            // 색상 및 투명도 점진적 증가
            Color currentColor = Color.Lerp(startColor, targetColor, progress);
            main.startColor = currentColor;
            
            // 방출량 점진적 증가 (캡슐 크기 반영)
            emission.rateOverTime = Mathf.Lerp(startEmissionRate, targetEmissionRate, progress);

            // fogContainer 실시간 업데이트
            if (fogContainer != null && enableCapsuleMask)
            {
                // 크기 변형
                Vector3 currentContainerScale = Vector3.Lerp(startContainerScale, targetContainerScale, progress);
                fogContainer.transform.localScale = currentContainerScale;
                
                // 회전 변형
                Quaternion currentContainerRotation = Quaternion.Lerp(startContainerRotation, targetContainerRotation, progress);
                fogContainer.transform.localRotation = currentContainerRotation;
            }
            
            yield return null;
        }

        // 최종 캡슐 설정 적용
        ConfigureFogForCapsule();
        
        // fogContainer 최종 설정
        if (fogContainer != null && enableCapsuleMask)
        {
            fogContainer.transform.localScale = targetContainerScale;
            fogContainer.transform.localRotation = targetContainerRotation;
        }
        
        Debug.Log("[CyberTransformationSpace] 안개 캡슐 변형 완료 (마스크 포함)");
    }

    /// <summary>
    /// 캡슐 단계 최종 안개 설정 (캡슐 크기 완전 연동)
    /// </summary>
    private void ConfigureFogForCapsule()
    {
        if (fogParticleSystem == null) return;

        var shape = fogParticleSystem.shape;
        var main = fogParticleSystem.main;
        var emission = fogParticleSystem.emission;

        // 캡슐 모양 설정
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        
        // 캡슐 크기에 맞춘 박스 형태
        Vector3 capsuleSize = CalculateCapsuleInnerSize();
        shape.scale = capsuleSize;
        
        // fogContainer 최종 크기와 회전 설정
        if (fogContainer != null && enableCapsuleMask)
        {
            // 최종 크기 설정 (파티클 스케일과 동기화)
            Vector3 containerScale = capsuleSize / 6f; // 6은 기본 박스 크기
            fogContainer.transform.localScale = containerScale;
            
            // 캡슐 방향에 맞춘 회전
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, currentCapsule.direction);
            fogContainer.transform.localRotation = rotation;
            
            // 위치 재확인
            fogContainer.transform.localPosition = Vector3.zero;
        }

        // 최대 투명도 및 방출량 (캡슐 크기 반영)
        main.startColor = currentCapsule.fogColor;
        float sizeMultiplier = (currentCapsule.radius * currentCapsule.height) / (3f * 6f); // 기본 크기 대비
        emission.rateOverTime = 30f * currentCapsule.fogDensity * sizeMultiplier;
        
        Debug.Log($"[CyberTransformationSpace] 최종 캡슐 안개 - 파티클크기: {capsuleSize}, 마스크크기: {fogContainer?.transform.localScale}, 방출량: {emission.rateOverTime.constant}");
    }

    /// <summary>
    /// 안개를 해체와 함께 축소 (캡슐 크기 및 방향 반영)
    /// </summary>
    private IEnumerator ShrinkFogWithDissolve()
    {
        if (!fogActive || fogParticleSystem == null) yield break;

        float dissolveTime = voxels.Count * 0.015f + dissolveTimePerVoxel;
        float elapsed = 0f;
        
        var shape = fogParticleSystem.shape;
        var main = fogParticleSystem.main;
        var emission = fogParticleSystem.emission;
        
        // 시작 값들 (현재 캡슐 상태)
        Vector3 startScale = shape.scale;
        Color startColor = main.startColor.color;
        float startEmissionRate = emission.rateOverTime.constant;
        
        // fogContainer 해체용 시작값
        Vector3 startContainerScale = fogContainer?.transform.localScale ?? Vector3.one;
        
        // 캡슐 크기 기반 최소 크기 계산
        float minScaleFactor = Mathf.Min(currentCapsule.scale.x, currentCapsule.scale.y, currentCapsule.scale.z) * 0.15f;
        Vector3 minScale = startScale * minScaleFactor;
        
        Debug.Log($"[CyberTransformationSpace] 안개 해체 축소 시작 - 시작크기: {startScale}, 최소크기: {minScale}");

        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / dissolveTime;
            
            // 크기 점진적 축소 (캡슐 크기 반영)
            Vector3 currentScale = Vector3.Lerp(startScale, minScale, progress);
            
            // 캡슐 방향을 고려한 축소 (방향별 다른 축소 속도)
            Vector3 capsuleDirection = currentCapsule.direction.normalized;
            
            // 캡슐 방향축은 더 빠르게 축소 (해체 방향과 일치)
            float directionAxisScale = Vector3.Dot(capsuleDirection, Vector3.up);
            float heightReduction = Mathf.Lerp(1f, 0.05f, progress * (1.5f + Mathf.Abs(directionAxisScale)));
            
            // 방향에 따른 축소 적용
            if (Mathf.Abs(capsuleDirection.y) > 0.7f) // 주로 Y축 방향
            {
                currentScale.y *= heightReduction;
            }
            else if (Mathf.Abs(capsuleDirection.x) > 0.7f) // 주로 X축 방향
            {
                currentScale.x *= heightReduction;
            }
            else if (Mathf.Abs(capsuleDirection.z) > 0.7f) // 주로 Z축 방향
            {
                currentScale.z *= heightReduction;
            }
            else // 대각선 방향
            {
                // 방향 벡터에 비례해서 축소
                currentScale.x *= Mathf.Lerp(1f, 0.1f, progress * Mathf.Abs(capsuleDirection.x) * 2f);
                currentScale.y *= heightReduction;
                currentScale.z *= Mathf.Lerp(1f, 0.1f, progress * Mathf.Abs(capsuleDirection.z) * 2f);
            }
            
            shape.scale = Vector3.Max(currentScale, Vector3.one * 0.01f); // 최소 크기 보장
            
            // 파티클과 fogContainer 모두 축소
            
            // fogContainer도 동시에 축소
            if (fogContainer != null && enableCapsuleMask)
            {
                Vector3 currentContainerScale = Vector3.Lerp(startContainerScale, minScale, progress);
                
                
                if (Mathf.Abs(capsuleDirection.y) > 0.7f) // 주로 Y축 방향
                {
                    currentContainerScale.y *= heightReduction;
                }
                else if (Mathf.Abs(capsuleDirection.x) > 0.7f) // 주로 X축 방향
                {
                    currentContainerScale.x *= heightReduction;
                }
                else if (Mathf.Abs(capsuleDirection.z) > 0.7f) // 주로 Z축 방향
                {
                    currentContainerScale.z *= heightReduction;
                }
                else // 대각선 방향
                {
                    currentContainerScale.x *= Mathf.Lerp(1f, 0.1f, progress * Mathf.Abs(capsuleDirection.x) * 2f);
                    currentContainerScale.y *= heightReduction;
                    currentContainerScale.z *= Mathf.Lerp(1f, 0.1f, progress * Mathf.Abs(capsuleDirection.z) * 2f);
                }
                
                fogContainer.transform.localScale = Vector3.Max(currentContainerScale, Vector3.one * 0.01f);
            }
            
            // 투명도 감소 (캡슐 밀도 고려)
            float fadeSpeed = 0.7f * (1f + currentCapsule.fogDensity * 0.5f);
            Color currentColor = Color.Lerp(startColor, Color.clear, progress * fadeSpeed);
            main.startColor = currentColor;
            
            // 방출량 감소 (캡슐 크기 고려한 감소 속도)
            float emissionFadeSpeed = 1f + (currentCapsule.radius * currentCapsule.height) / 18f; // 큰 캡슐일수록 느리게 감소
            emission.rateOverTime = Mathf.Lerp(startEmissionRate, 0f, progress * emissionFadeSpeed);
            
            yield return null;
        }

        // 완전 투명하게
        emission.rateOverTime = 0f;
        main.startColor = Color.clear;
        
        Debug.Log("[CyberTransformationSpace] 안개 해체 축소 완료");
    }

    /// <summary>
    /// 안개 효과 페이드아웃 (몬스터 등장과 함께)
    /// </summary>
    private IEnumerator FadeOutFog()
    {
        if (!fogActive || fogParticleSystem == null) yield break;

        float fadeTime = currentCapsule.fogFadeTime;
        float elapsed = 0f;
        
        var emission = fogParticleSystem.emission;
        float originalRate = emission.rateOverTime.constant;

        Debug.Log("[CyberTransformationSpace] 안개 페이드아웃 시작");

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeTime;

            // 방출량 감소
            emission.rateOverTime = Mathf.Lerp(originalRate, 0f, progress);
            
            yield return null;
        }

        // 완전히 중지
        emission.rateOverTime = 0f;
        
        // 남은 파티클이 자연스럽게 사라질 때까지 대기
        yield return new WaitForSeconds(2f);
        
        if (fogParticleSystem != null)
        {
            fogParticleSystem.Stop();
            fogParticleSystem.gameObject.SetActive(false);
        }
        
        fogActive = false;
        Debug.Log("[CyberTransformationSpace] 안개 효과 완전 종료");
    }

    /// <summary>
    /// 안개 즉시 제거
    /// </summary>
    private void ClearFog()
    {
        // 진행 중인 안개 변형 코루틴 중단
        if (fogTransformCoroutine != null)
        {
            StopCoroutine(fogTransformCoroutine);
            fogTransformCoroutine = null;
        }

        if (fogParticleSystem != null)
        {
            fogParticleSystem.Stop();
            fogParticleSystem.gameObject.SetActive(false);
            
            // 파티클 시스템이 fogContainer의 자식이라면 개별 삭제하지 않음
            if (fogParticleSystem.transform.parent != fogContainer?.transform)
            {
                Destroy(fogParticleSystem.gameObject);
            }
            fogParticleSystem = null;
        }
        


        // fogContainer는 사용자가 설정한 것이므로 삭제하지 말고 비활성화만
        if (fogContainer != null)
        {
            // 자식 파티클들만 정리
            foreach (Transform child in fogContainer.transform)
            {
                if (child.GetComponent<ParticleSystem>())
                {
                    Destroy(child.gameObject);
                }
            }
            
            // 컨테이너는 비활성화만 (재사용을 위해)
            fogContainer.SetActive(false);
            
            // 위치, 회전, 크기 초기화
            fogContainer.transform.localPosition = Vector3.zero;
            fogContainer.transform.localRotation = Quaternion.identity;
            fogContainer.transform.localScale = Vector3.one;
        }
        
        fogActive = false;
        Debug.Log("[CyberTransformationSpace] 안개 완전 정리 완료 (컨테이너는 보존)");
    }

    //-------------------------------------------------------------------
    // 🌟 퍼블릭 인터페이스 (보스 시스템에서 호출)
    //-------------------------------------------------------------------
    
    /// <summary>
    /// 몬스터별 캡슐 데이터로 변신 시작
    /// </summary>
    public void StartTransformation(MonsterCapsuleData capsuleData, GameObject targetMonster = null)
    {
        if (isBusy) return;
        
        // 몬스터 교체
        if (targetMonster != null)
        {
            if (monster != null) monster.SetActive(false);
            monster = targetMonster;
        }
        
        // 새로운 캡슐 데이터로 목표 위치 재계산
        currentCapsule = capsuleData;
        PrecomputeTargets(currentCapsule);
        
        StartCoroutine(TransformSequence());
    }
    
    /// <summary>
    /// 기본 설정으로 변신 시작
    /// </summary>
    public void StartTransformation()
    {
        StartTransformation(defaultCapsule);
    }
    
    /// <summary>
    /// 큐브들을 원래 상태로 복귀 (다시 변신할 때 필요)
    /// </summary>
    public void ResetToOriginal()
    {
        if (isBusy) return;
        
        foreach (var kvp in originPos)
        {
            kvp.Key.localPosition = kvp.Value;
            kvp.Key.localScale = Vector3.one;
            SetAlpha(kvp.Key, 1f);
            kvp.Key.gameObject.SetActive(true);
        }
        if (monster) monster.SetActive(false);
        
        // 안개 효과 정리
        ClearFog();
        
        // VoxelFloatEffect 재개
        var floatEffect = GetComponent<VoxelFloatEffect>();
        if (floatEffect != null)
        {
            floatEffect.SetPaused(false);
            Debug.Log("[CyberTransformationSpace] VoxelFloatEffect 재개");
        }
    }

    //-------------------------------------------------------------------
    // 🎬 메인 시퀸스 (구형 → 캡슐 → 해체 & 몬스터)
    //-------------------------------------------------------------------

    public IEnumerator TransformSequence()
    {
        if (isBusy) yield break;
        isBusy = true;

        // VoxelFloatEffect와의 충돌 방지 - 일시 정지
        var floatEffect = GetComponent<VoxelFloatEffect>();
        if (floatEffect != null)
        {
            floatEffect.SetPaused(true);
            floatEffect.RefreshVoxelList(); // 현재 위치를 기준으로 재설정
            Debug.Log("[CyberTransformationSpace] VoxelFloatEffect 일시 정지 및 위치 갱신");
        }

        // 1단계: 구형 안개 시작과 함께 구형 형성
        yield return ActivateFog(); // 구형 안개부터 시작
        yield return SphereFormation();
        
        // 2단계: 캡슐 변형과 안개 변형 동시 진행
        yield return StartParallelTransforms();
        
        // 3단계: 완성된 캡슐 안개 속에서 잠시 대기 (신비로운 효과)
        yield return new WaitForSeconds(0.8f);
        
        // 4단계: 해체와 안개 축소 동시 진행
        yield return StartParallelDissolve();

        // 변신 완료! 몬스터만 남김 (VoxelFloatEffect는 재개하지 않음)
        isBusy = false;
    }

    /// <summary>
    /// 캡슐 변형과 안개 변형을 동시에 실행
    /// </summary>
    private IEnumerator StartParallelTransforms()
    {
        Debug.Log("[CyberTransformationSpace] 병렬 변형 시작: 복셀 캡슐화 + 안개 변형");
        
        // 코루틴 동시 시작
        Coroutine voxelTransform = StartCoroutine(MapToCapsule());
        fogTransformCoroutine = StartCoroutine(TransformFogWithCapsule());
        
        // 두 변형이 모두 완료될 때까지 대기
        yield return voxelTransform;
        if (fogTransformCoroutine != null)
            yield return fogTransformCoroutine;
            
        Debug.Log("[CyberTransformationSpace] 병렬 변형 완료");
    }

    /// <summary>
    /// 해체와 안개 축소를 동시에 실행
    /// </summary>
    private IEnumerator StartParallelDissolve()
    {
        Debug.Log("[CyberTransformationSpace] 병렬 해체 시작: 복셀 해체 + 안개 축소");
        
        // 코루틴 동시 시작
        Coroutine voxelDissolve = StartCoroutine(DissolveAndReveal());
        Coroutine fogShrink = StartCoroutine(ShrinkFogWithDissolve());
        
        // 두 해체가 모두 완료될 때까지 대기
        yield return voxelDissolve;
        yield return fogShrink;
            
        Debug.Log("[CyberTransformationSpace] 병렬 해체 완료");
    }

    //-------------------------------------------------------------------
    // Phase 1 : 구형태 만들기
    //-------------------------------------------------------------------

    private IEnumerator SphereFormation()
    {
        float t = 0f;
        int n = voxels.Count;
        while (t < sphereFormationTime)
        {
            float p = t / sphereFormationTime;
            for (int i = 0; i < n; i++)
            {
                voxels[i].localPosition = Vector3.Lerp(originPos[voxels[i]], spherePos[i], p);
            }
            t += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < n; i++) voxels[i].localPosition = spherePos[i];
    }

    //-------------------------------------------------------------------
    // Phase 2 : 구 → 캡슐 맵핑
    //-------------------------------------------------------------------

    private IEnumerator MapToCapsule()
    {
        float t = 0f;
        int n = voxels.Count;
        float transformTime = currentCapsule.transformTime;
        
        while (t < transformTime)
        {
            float p = t / transformTime;
            for (int i = 0; i < n; i++)
            {
                voxels[i].localPosition = Vector3.Lerp(spherePos[i], capsulePos[i], p);
                Vector3 normal = capsulePos[i].normalized;
                if (normal.sqrMagnitude > 0.001f)
                    voxels[i].rotation = Quaternion.LookRotation(normal);
            }
            t += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < n; i++) voxels[i].localPosition = capsulePos[i];
    }

    //-------------------------------------------------------------------
    // Phase 3 : Y축 상단부터 해체 + 몬스터 등장
    //-------------------------------------------------------------------

    private IEnumerator DissolveAndReveal()
    {
        // 캡슐 방향을 고려한 정렬 (캡슐 상단 → 하단)
        Vector3 capsuleDirection = currentCapsule.direction.normalized;
        voxels.Sort((a, b) => {
            float dotA = Vector3.Dot(a.localPosition, capsuleDirection);
            float dotB = Vector3.Dot(b.localPosition, capsuleDirection);
            return dotB.CompareTo(dotA); // 캡슐 방향 기준 상단부터
        });
        int n = voxels.Count;

        // 몬스터 출현 시점 (상단 30% 해체 후)
        int revealIndex = Mathf.FloorToInt(n * 0.3f);

        for (int i = 0; i < n; i++)
        {
            StartCoroutine(DissolveVoxel(voxels[i]));
            if (i == revealIndex && monster != null)
                StartCoroutine(RevealMonster());
            yield return new WaitForSeconds(0.015f); // 연속 해체 간격
        }

        // 모든 해체 끝날 때까지 대기
        yield return new WaitForSeconds(dissolveTimePerVoxel + 0.2f);
    }

    private IEnumerator DissolveVoxel(Transform v)
    {
        float t = 0f;
        Vector3 start = v.localPosition;
        
        // 캡슐 방향의 반대로 해체 (더 자연스러운 효과)
        Vector3 capsuleDirection = currentCapsule.direction.normalized;
        Vector3 dissolveDirection = -capsuleDirection * 2f; // 캡슐 방향 반대로
        Vector3 end = start + dissolveDirection + Random.insideUnitSphere * 0.3f;
        
        while (t < dissolveTimePerVoxel)
        {
            float p = t / dissolveTimePerVoxel;
            v.localPosition = Vector3.Lerp(start, end, p);
            v.localScale = Vector3.one * (1 - p);
            SetAlpha(v, dissolveAlpha.Evaluate(1 - p));
            t += Time.deltaTime;
            yield return null;
        }
        v.gameObject.SetActive(false);
    }

    private IEnumerator RevealMonster()
    {
        // 안개는 ShrinkFogWithDissolve()에서 이미 처리되므로 별도 페이드아웃 불필요

        monster.SetActive(true);
        float t = 0f;
        Vector3 oriScale = monster.transform.localScale;
        monster.transform.localScale = Vector3.zero;
        Vector3 startPos = monster.transform.position + Vector3.up * 1f;
        Vector3 oriPos = monster.transform.position;

        while (t < monsterRevealTime)
        {
            float p = t / monsterRevealTime;
            monster.transform.localScale = Vector3.Lerp(Vector3.zero, oriScale, p);
            monster.transform.position = Vector3.Lerp(startPos, oriPos, p);
            t += Time.deltaTime;
            yield return null;
        }
        monster.transform.localScale = oriScale;
        monster.transform.position = oriPos;
        
        Debug.Log("[CyberTransformationSpace] 몬스터 등장 완료");
    }

    //-------------------------------------------------------------------
    // 🎯 몬스터별 헬퍼 메서드 (보스 시스템에서 쉽게 사용)
    //-------------------------------------------------------------------

    /// <summary>
    /// Worm 몬스터용 변신 시작 - 생물학적 위험 테마
    /// </summary>
    public void StartWormTransformation(GameObject wormMonster = null)
    {
        var wormData = new MonsterCapsuleData
        {
            radius = 3.2f,
            height = 5.5f,
            scale = new Vector3(1.1f, 0.9f, 1.1f),
            transformTime = 1.8f,
            
            // 대각선 방향 (뱀처럼 누워있는 형태)
            direction = new Vector3(0.3f, 0.7f, 0f).normalized,
            forwardAxis = Vector3.forward,
            
            // 독성 녹색 안개
            enableFogEffect = true,
            fogColor = new Color(0.2f, 0.8f, 0.3f, 0.5f),
            fogDensity = 0.4f,
            fogFadeTime = 0.8f
        };
        
        StartTransformation(wormData, wormMonster);
        Debug.Log("[CyberTransformationSpace] Worm 변신 시작 - 독성 녹색 안개");
    }

    /// <summary>
    /// Trojan 몬스터용 변신 시작 - 시스템 침입 테마
    /// </summary>
    public void StartTrojanTransformation(GameObject trojanMonster = null)
    {
        var trojanData = new MonsterCapsuleData
        {
            radius = 3.0f,
            height = 6.5f,
            scale = Vector3.one,
            transformTime = 1.5f,
            
            // 수직 방향 (전통적인 직립 형태)
            direction = Vector3.up,
            forwardAxis = Vector3.forward,
            
            // 경고 노란색 안개
            enableFogEffect = true,
            fogColor = new Color(1.0f, 0.8f, 0.2f, 0.5f),
            fogDensity = 0.3f,
            fogFadeTime = 1.2f
        };
        
        StartTransformation(trojanData, trojanMonster);
        Debug.Log("[CyberTransformationSpace] Trojan 변신 시작 - 경고 노란색 안개");
    }

    /// <summary>
    /// Ransomware 몬스터용 변신 시작 - 데이터 암호화 테마
    /// </summary>
    public void StartRansomwareTransformation(GameObject ransomwareMonster = null)
    {
        var ransomwareData = new MonsterCapsuleData
        {
            radius = 3.5f,
            height = 6.0f,
            scale = new Vector3(1.2f, 1.0f, 1.2f),
            transformTime = 2.0f,
            
            // 기울어진 방향 (불안정하고 위협적인 형태)
            direction = new Vector3(-0.4f, 0.8f, 0.2f).normalized,
            forwardAxis = new Vector3(0.1f, 0f, 1f).normalized,
            
            // 위험 빨간색 안개 (높은 밀도)
            enableFogEffect = true,
            fogColor = new Color(0.9f, 0.2f, 0.3f, 0.6f),
            fogDensity = 0.7f,
            fogFadeTime = 1.8f
        };
        
        StartTransformation(ransomwareData, ransomwareMonster);
        Debug.Log("[CyberTransformationSpace] Ransomware 변신 시작 - 위험 빨간색 안개");
    }

    /// <summary>
    /// 커스텀 사이버 안개 설정으로 변신 시작
    /// </summary>
    public void StartCustomCyberTransformation(
        GameObject targetMonster, 
        Color cyberColor, 
        Vector3 capsuleDirection, 
        float fogDensity = 0.5f,
        float capsuleRadius = 3f, 
        float capsuleHeight = 6f)
    {
        var customData = new MonsterCapsuleData
        {
            radius = capsuleRadius,
            height = capsuleHeight,
            scale = Vector3.one,
            transformTime = 1.5f,
            
            direction = capsuleDirection.normalized,
            forwardAxis = Vector3.forward,
            
            enableFogEffect = true,
            fogColor = cyberColor,
            fogDensity = fogDensity,
            fogFadeTime = 1.2f
        };
        
        StartTransformation(customData, targetMonster);
        Debug.Log($"[CyberTransformationSpace] 커스텀 변신 시작 - 색상: {cyberColor}, 방향: {capsuleDirection}");
    }

    //-------------------------------------------------------------------
    // 🔧 알파 유틸리티
    //-------------------------------------------------------------------

    private static void SetAlpha(Transform tf, float a)
    {
        var r = tf.GetComponent<Renderer>();
        if (!r) return;
        foreach (var m in r.materials)
        {
            if (a < 1f)
            {
                m.SetFloat("_Mode", 3);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.EnableKeyword("_ALPHABLEND_ON");
                m.renderQueue = 3000;
            }
            else m.renderQueue = -1;
        }
    }
}
