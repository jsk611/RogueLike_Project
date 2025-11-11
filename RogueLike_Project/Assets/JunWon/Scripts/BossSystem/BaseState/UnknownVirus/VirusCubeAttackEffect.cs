using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바이러스 레이저 큐브 공격 이펙트 - 중심점 기반 큐브형성 공격
/// - 사이버 스타일 색상 변화 (보라색 → 빨간색)
/// - 공유 Material로 전체 복셀 동시 제어
/// - 전기 아크 효과 포함
/// </summary>
public class VirusCubeAttackEffect : MonoBehaviour
{
    #region Serialized Fields

    [Header("Enhanced Spread")]
    [SerializeField] private bool useEnhancedSpread = true;
    [SerializeField] private VirusCubeSpread spreadCalculator;

    [Header("Formation Settings")]
    [SerializeField] private float formationTime = 2f;
    [SerializeField] private float compactTime = 1f;
    [SerializeField] private float expandTime = 0.3f;
    [SerializeField] private float returnTime = 1.5f;
    [SerializeField] private bool shouldReturnToOriginal = true;

    [Header("Cube Formation")]
    [SerializeField] private Vector3Int cubeSize = new Vector3Int(8, 8, 8);
    [SerializeField] private float voxelSpacing = 0.15f;
    [SerializeField] private float compactScale = 0.6f;
    [SerializeField] private float expandScale = 1.8f;
    [SerializeField] private Vector3 cubeCenter = Vector3.zero;

    [Header("Visual Effects - Cyber Style")]
    [SerializeField] private Color idleOutlineColor = new Color(0.5f, 0f, 1f);
    [SerializeField] private float idleEmissionStrength = 1f;
    [SerializeField] private Color formationColor = new Color(0.3f, 0f, 0.8f);
    [SerializeField] private Color chargingColor = new Color(0.8f, 0f, 1f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.3f, 0.5f);
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private ParticleSystem expandEffect;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip formationSound;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip expandSound;

    [Header("Electric Arc Effect")]
    [SerializeField] private bool useElectricArc = true;
    [SerializeField] private int arcCount = 8;
    [SerializeField] private float arcDistance = 3f;
    [SerializeField] private float arcUpdateInterval = 0.1f;
    [SerializeField] private float arcMinWidth = 0.05f;
    [SerializeField] private float arcMaxWidth = 0.1f;

    #endregion

    #region Private Fields
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");

    private readonly List<Transform> voxelChildren = new();
    private readonly List<Vector3> originalPositions = new();
    private readonly List<Vector3> cubePositions = new();
    private readonly List<Vector3> compactPositions = new();
    private readonly List<Vector3> expandedPositions = new();

    private Material sharedVoxelMaterial;
    private Material originalSharedMaterial;
    private Color originalEmissionColor = Color.black;
    private Color originalBaseColor = Color.black;
    private bool originalEmissionEnabled;
    private int cachedBaseColorPropertyId = -1;
    private bool hasEmissionStrengthProperty;
    private bool hasEmissionColorProperty;

    private readonly List<LineRenderer> electricArcs = new();
    private Material sharedArcMaterial;

    private Transform target;
    private bool isExecuting;

    private const float COLOR_PHASE_1 = 0.3f;
    private const float COLOR_PHASE_2 = 0.7f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeArcMaterial();
    }

    private void Start()
    {
        InitializeComponents();
        CollectAndPrepareVoxels();
    }

    private void OnDestroy()
    {
        CleanupResources();
    }

    #endregion

    #region Initialization

    private void InitializeArcMaterial()
    {
        if (!useElectricArc) return;

        sharedArcMaterial = new Material(Shader.Find("Sprites/Default"));
        sharedArcMaterial.EnableKeyword("_EMISSION");
    }

    private void InitializeComponents()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        target = GameObject.FindWithTag("Player")?.transform;
        cubeCenter = transform.position;

        if (useEnhancedSpread)
        {
            spreadCalculator = GetComponent<VirusCubeSpread>();
        }
    }

    private void CollectAndPrepareVoxels()
    {
        voxelChildren.Clear();
        originalPositions.Clear();

        if (transform.childCount > 0)
        {
            Renderer firstRenderer = transform.GetChild(0).GetComponent<Renderer>();
            if (firstRenderer != null)
            {
                InitializeSharedVoxelMaterial(firstRenderer);
            }
        }

        foreach (Transform child in transform)
        {
            voxelChildren.Add(child);
            originalPositions.Add(child.localPosition);

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null && sharedVoxelMaterial != null)
            {
                renderer.sharedMaterial = sharedVoxelMaterial;
            }
            else if (renderer == null)
            {
                Debug.LogWarning($"[VirusCubeAttack] 복셀 {child.name}에 Renderer가 없습니다.", child);
            }
        }

        CalculateFormationPositions();
        ResetSharedMaterialToOriginalState();
        Debug.Log($"[VirusCubeAttack] 복셀 수집 완료: {voxelChildren.Count}개 (공유 Material)");
    }

    private void InitializeSharedVoxelMaterial(Renderer sampleRenderer)
    {
        originalSharedMaterial = sampleRenderer.sharedMaterial;

        if (originalSharedMaterial == null)
        {
            Debug.LogError("[VirusCubeAttack] 원본 Material을 찾을 수 없습니다.");
            return;
        }

        CacheOriginalMaterialState(originalSharedMaterial);

        sharedVoxelMaterial = new Material(originalSharedMaterial)
        {
            name = $"{originalSharedMaterial.name}_VirusCubeShared"
        };

        sharedVoxelMaterial.EnableKeyword("_EMISSION");
        hasEmissionStrengthProperty = sharedVoxelMaterial.HasProperty(EmissionStrengthId);
        hasEmissionColorProperty = sharedVoxelMaterial.HasProperty(EmissionColorId);
    }

    private void CacheOriginalMaterialState(Material sourceMaterial)
    {
        if (sourceMaterial.HasProperty(BaseColorId))
        {
            cachedBaseColorPropertyId = BaseColorId;
            originalBaseColor = sourceMaterial.GetColor(BaseColorId);
        }
        else if (sourceMaterial.HasProperty(ColorId))
        {
            cachedBaseColorPropertyId = ColorId;
            originalBaseColor = sourceMaterial.GetColor(ColorId);
        }
        else
        {
            cachedBaseColorPropertyId = -1;
            originalBaseColor = Color.black;
        }

        if (sourceMaterial.HasProperty(EmissionColorId))
        {
            originalEmissionColor = sourceMaterial.GetColor(EmissionColorId);
            hasEmissionColorProperty = true;
        }
        else
        {
            originalEmissionColor = Color.black;
            hasEmissionColorProperty = false;
        }

        originalEmissionEnabled = sourceMaterial.IsKeywordEnabled("_EMISSION");
        hasEmissionStrengthProperty = sourceMaterial.HasProperty(EmissionStrengthId);
    }

    

    #endregion

    #region Formation Calculation

    private void CalculateFormationPositions()
    {
        cubePositions.Clear();
        compactPositions.Clear();
        expandedPositions.Clear();

        int positionIndex = 0;
        int totalPositions = CountEdgePositions();

        for (int x = 0; x < cubeSize.x; x++)
        {
            for (int y = 0; y < cubeSize.y; y++)
            {
                for (int z = 0; z < cubeSize.z; z++)
                {
                    if (!IsEdgePosition(x, y, z)) continue;

                    Vector3 cubePos = CalculateCubePosition(x, y, z);
                    cubePositions.Add(cubePos);
                    compactPositions.Add(cubePos * compactScale);

                    Vector3 expandPos = CalculateExpandPosition(cubePos, positionIndex, totalPositions);
                    expandedPositions.Add(expandPos);

                    positionIndex++;
                }
            }
        }

        Debug.Log($"[VirusCubeAttack] 형성 위치 계산 완료: {cubePositions.Count}개");
    }

    private int CountEdgePositions()
    {
        int count = 0;
        for (int x = 0; x < cubeSize.x; x++)
            for (int y = 0; y < cubeSize.y; y++)
                for (int z = 0; z < cubeSize.z; z++)
                    if (IsEdgePosition(x, y, z)) count++;
        return count;
    }

    private bool IsEdgePosition(int x, int y, int z)
    {
        return (x == 0 || x == cubeSize.x - 1) ||
               (y == 0 || y == cubeSize.y - 1) ||
               (z == 0 || z == cubeSize.z - 1);
    }

    private Vector3 CalculateCubePosition(int x, int y, int z)
    {
        return new Vector3(
            (x - cubeSize.x / 2f + 0.5f) * voxelSpacing,
            (y - cubeSize.y / 2f + 0.5f) * voxelSpacing,
            (z - cubeSize.z / 2f + 0.5f) * voxelSpacing
        );
    }

    private Vector3 CalculateExpandPosition(Vector3 cubePos, int index, int total)
    {
        if (useEnhancedSpread && spreadCalculator != null)
        {
            return spreadCalculator.CalculateSpreadPosition(cubePos, index, total);
        }

        Vector3 direction = cubePos.normalized;
        return direction * expandScale;
    }

    #endregion

    #region Material Control

    private void SetSharedBaseColor(Color baseColor)
    {
        if (sharedVoxelMaterial == null || cachedBaseColorPropertyId == -1) return;
        sharedVoxelMaterial.SetColor(cachedBaseColorPropertyId, baseColor);
    }

    private void SetSharedEmission(Color emissionColor, float intensity, bool forceEnable = true)
    {
        if (sharedVoxelMaterial == null || !hasEmissionColorProperty) return;

        float sanitizedIntensity = Mathf.Max(0f, intensity);
        sharedVoxelMaterial.SetColor(EmissionColorId, emissionColor * sanitizedIntensity);

        if (hasEmissionStrengthProperty)
        {
            sharedVoxelMaterial.SetFloat(EmissionStrengthId, sanitizedIntensity);
        }

        if (sanitizedIntensity <= 0.001f && !forceEnable)
        {
            sharedVoxelMaterial.DisableKeyword("_EMISSION");
        }
        else
        {
            sharedVoxelMaterial.EnableKeyword("_EMISSION");
        }
    }

    private void ResetSharedMaterialToOriginalState()
    {
        SetSharedBaseColor(originalBaseColor);
        SetSharedEmission(Color.black, 0f, false);
    }

    #endregion

    #region Public API

    public void StartLaserAttack()
    {
        if (isExecuting)
        {
            Debug.LogWarning("[VirusCubeAttack] 이미 공격이 실행 중입니다.");
            return;
        }

        StartCoroutine(ExecuteLaserAttack());
    }

    public void StopEffect()
    {
        StopAllCoroutines();
        ClearElectricArcs();
        isExecuting = false;
        ResetSharedMaterialToOriginalState();
        Debug.Log("[VirusCubeAttack] 효과 중지됨");
    }

    public void SetReturnMode(bool shouldReturn)
    {
        shouldReturnToOriginal = shouldReturn;
    }

    #endregion

    #region Attack Execution

    private IEnumerator ExecuteLaserAttack()
    {
        isExecuting = true;
        Debug.Log("[VirusCubeAttack] 레이저 공격 시작");

        yield return StartCoroutine(FormCubePhase());
        yield return StartCoroutine(CompactAndChargePhase());
        yield return StartCoroutine(ExpandAndLaserPhase());

        if (shouldReturnToOriginal)
        {
            yield return StartCoroutine(ReturnToFloatingPhase());
        }
        else
        {
            ResetSharedMaterialToOriginalState();
        }

        isExecuting = false;
        Debug.Log("[VirusCubeAttack] 레이저 공격 완료");
    }

    #endregion

    #region Phase 1: Cube Formation

    private IEnumerator FormCubePhase()
    {
        PlaySound(formationSound);
        Debug.Log("[Phase 1] 큐브 형성 시작");

        float elapsed = 0f;
        while (elapsed < formationTime)
        {
            float progress = elapsed / formationTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                if (voxelChildren[i] == null) continue;

                Vector3 startPos = GetSafeOriginalPosition(i);
                Vector3 targetPos = GetSafeCubePosition(i);

                voxelChildren[i].localPosition = Vector3.Lerp(startPos, targetPos, easedProgress);
                voxelChildren[i].Rotate(Vector3.up, Time.deltaTime * 180f * (1f - easedProgress));
            }

            Color emissionColor = Color.Lerp(idleOutlineColor, formationColor, progress);
            float intensity = Mathf.Lerp(1f, 2f, progress);
            SetSharedEmission(emissionColor, intensity);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[Phase 1] 큐브 형성 완료");
    }

    #endregion

    #region Phase 2: Compact and Charge

    private IEnumerator CompactAndChargePhase()
    {
        PlaySound(chargeSound);
        Debug.Log("[Phase 2] 수축 및 차징 시작");

        ActivateParticle(chargeEffect);

        if (useElectricArc)
        {
            CreateElectricArcs();
        }

        float elapsed = 0f;
        float nextArcUpdate = 0f;

        while (elapsed < compactTime)
        {
            float progress = elapsed / compactTime;
            Color currentEmission = CalculateChargingColor(progress);
            float pulseSpeed = 3f + progress * 10f;
            float intensity = Mathf.PingPong(elapsed * pulseSpeed, 1f);

            UpdateVoxelsCompacting(progress);
            float emissionMultiplier = Mathf.Lerp(2f, 8f, progress);
            Color pulseEmission = Color.Lerp(currentEmission, Color.white, intensity * 0.4f);
            SetSharedEmission(pulseEmission, emissionMultiplier);

            if (useElectricArc && elapsed >= nextArcUpdate)
            {
                UpdateElectricArcs(progress, currentEmission);
                nextArcUpdate = elapsed + arcUpdateInterval;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.15f);

        if (useElectricArc)
        {
            ClearElectricArcs();
        }

        Debug.Log("[Phase 2] 수축 및 차징 완료");
    }

    private Color CalculateChargingColor(float progress)
    {
        if (progress < COLOR_PHASE_1)
        {
            float t = progress / COLOR_PHASE_1;
            return Color.Lerp(formationColor, chargingColor, t);
        }

        if (progress < COLOR_PHASE_2)
        {
            float t = (progress - COLOR_PHASE_1) / (COLOR_PHASE_2 - COLOR_PHASE_1);
            return Color.Lerp(chargingColor, dangerColor, t);
        }

        float finalT = (progress - COLOR_PHASE_2) / (1f - COLOR_PHASE_2);
        return Color.Lerp(dangerColor, attackColor, finalT);
    }

    private void UpdateVoxelsCompacting(float progress)
    {
        for (int i = 0; i < voxelChildren.Count; i++)
        {
            if (voxelChildren[i] == null) continue;

            Vector3 cubePos = GetSafeCubePosition(i);
            Vector3 compactPos = GetSafeCompactPosition(i);
            voxelChildren[i].localPosition = Vector3.Lerp(cubePos, compactPos, progress);

            if (progress > COLOR_PHASE_2)
            {
                float shake = (progress - COLOR_PHASE_2) / (1f - COLOR_PHASE_2);
                voxelChildren[i].localPosition += Random.insideUnitSphere * 0.02f * shake;
            }
        }
    }

    #endregion

    #region Phase 3: Expand and Laser

    private IEnumerator ExpandAndLaserPhase()
    {
        PlaySound(expandSound);
        Debug.Log("[Phase 3] 확산 및 레이저 발사 시작");

        ApplyWhiteFlash();
        yield return new WaitForSeconds(0.06f);

        DeactivateParticle(chargeEffect);
        ActivateParticle(expandEffect);

        float elapsed = 0f;
        while (elapsed < expandTime)
        {
            float progress = elapsed / expandTime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                if (voxelChildren[i] == null) continue;

                Vector3 compactPos = GetSafeCompactPosition(i);
                Vector3 expandPos = GetSafeExpandPosition(i);
                voxelChildren[i].localPosition = Vector3.Lerp(compactPos, expandPos, easedProgress);
            }

            ApplyExpandFade(progress);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[Phase 3] 확산 및 레이저 발사 완료");
    }

    private void ApplyWhiteFlash()
    {
        SetSharedEmission(Color.white, 10f);
    }

    private void ApplyExpandFade(float progress)
    {
        Color fadeEmission = Color.Lerp(attackColor, Color.black, progress);
        float fadeIntensity = Mathf.Lerp(8f, 0f, progress);
        SetSharedEmission(fadeEmission, fadeIntensity, progress < 0.999f);
    }

    #endregion

    #region Phase 4: Return to Floating

    private IEnumerator ReturnToFloatingPhase()
    {
        Debug.Log("[Phase 4] 원래 플로팅 상태로 복귀 시작");

        float elapsed = 0f;
        while (elapsed < returnTime)
        {
            float progress = elapsed / returnTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                if (voxelChildren[i] == null) continue;

                Vector3 expandPos = GetSafeExpandPosition(i);
                Vector3 originalPos = GetSafeOriginalPosition(i);

                voxelChildren[i].localPosition = Vector3.Lerp(expandPos, originalPos, easedProgress);
                voxelChildren[i].localScale = Vector3.Lerp(voxelChildren[i].localScale, Vector3.one, easedProgress);
                voxelChildren[i].Rotate(Vector3.up, Time.deltaTime * 90f * (1f - progress));
            }

            Color emissionColor = Color.Lerp(attackColor, originalEmissionColor, easedProgress);
            float currentIntensity = Mathf.Lerp(6f, 0f, easedProgress);
            bool keepEmission = easedProgress < 0.999f && currentIntensity > 0.01f;
            SetSharedEmission(emissionColor, currentIntensity, keepEmission);

            elapsed += Time.deltaTime;
            yield return null;
        }

        RestoreOriginalFloatingState();
        Debug.Log("[Phase 4] 원래 플로팅 상태 복귀 완료");
    }

    private void RestoreOriginalFloatingState()
    {
        for (int i = 0; i < voxelChildren.Count; i++)
        {
            if (voxelChildren[i] == null) continue;

            if (i < originalPositions.Count)
            {
                voxelChildren[i].localPosition = originalPositions[i];
            }
            voxelChildren[i].localScale = Vector3.one;
        }

        ResetSharedMaterialToOriginalState();

        DeactivateParticle(chargeEffect);
        DeactivateParticle(expandEffect);

        Debug.Log("[VirusCubeAttack] 상태 완전 복원 (Material 색상/Emission 초기화)");
    }

    #endregion

    #region Electric Arc Management

    private void CreateElectricArcs()
    {
        ClearElectricArcs();

        for (int i = 0; i < arcCount; i++)
        {
            GameObject arcObj = new GameObject($"ElectricArc_{i}");
            arcObj.transform.SetParent(transform);
            arcObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = arcObj.AddComponent<LineRenderer>();
            lr.startWidth = arcMinWidth;
            lr.endWidth = arcMinWidth * 0.5f;
            lr.positionCount = 2;
            lr.material = sharedArcMaterial;
            lr.startColor = formationColor;
            lr.endColor = Color.white;
            lr.numCapVertices = 5;

            electricArcs.Add(lr);
        }

        Debug.Log($"[VirusCubeAttack] 전기 아크 {arcCount}개 생성");
    }

    private void UpdateElectricArcs(float intensity, Color currentColor)
    {
        if (sharedArcMaterial != null && sharedArcMaterial.HasProperty(EmissionColorId))
        {
            float emissionPower = 2f + intensity * 3f;
            sharedArcMaterial.SetColor(EmissionColorId, currentColor * emissionPower);
        }

        foreach (LineRenderer arc in electricArcs)
        {
            if (arc == null) continue;

            arc.SetPosition(0, transform.position);

            Vector3 randomDirection = Random.onUnitSphere;
            float currentDistance = arcDistance * Mathf.Max(0.2f, intensity);
            Vector3 endPoint = transform.position + randomDirection * currentDistance;
            arc.SetPosition(1, endPoint);

            arc.startColor = currentColor;
            arc.endColor = Color.white;

            float width = Mathf.Lerp(arcMinWidth, arcMaxWidth, intensity);
            arc.startWidth = width;
            arc.endWidth = width * 0.5f;
        }
    }

    private void ClearElectricArcs()
    {
        foreach (LineRenderer arc in electricArcs)
        {
            if (arc != null)
            {
                Destroy(arc.gameObject);
            }
        }
        electricArcs.Clear();
    }

    #endregion

    #region Utility Methods

    private Vector3 GetSafeOriginalPosition(int index)
    {
        return index < originalPositions.Count ? originalPositions[index] : Vector3.zero;
    }

    private Vector3 GetSafeCubePosition(int index)
    {
        return index < cubePositions.Count ? cubePositions[index] : Vector3.zero;
    }

    private Vector3 GetSafeCompactPosition(int index)
    {
        return index < compactPositions.Count ? compactPositions[index] : Vector3.zero;
    }

    private Vector3 GetSafeExpandPosition(int index)
    {
        return index < expandedPositions.Count ? expandedPositions[index] : Vector3.zero;
    }

    private void ActivateParticle(ParticleSystem particle)
    {
        if (particle == null) return;

        particle.gameObject.SetActive(true);
        particle.Play();
    }

    private void DeactivateParticle(ParticleSystem particle)
    {
        if (particle == null) return;

        particle.Stop();
        particle.gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void CleanupResources()
    {
        ClearElectricArcs();

        if (sharedArcMaterial != null)
        {
            Destroy(sharedArcMaterial);
            sharedArcMaterial = null;
        }

        if (sharedVoxelMaterial != null)
        {
            foreach (Transform child in voxelChildren)
            {
                if (child == null) continue;

                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null && originalSharedMaterial != null)
                {
                    renderer.sharedMaterial = originalSharedMaterial;
                }
            }

            Destroy(sharedVoxelMaterial);
            sharedVoxelMaterial = null;
        }

        Debug.Log("[VirusCubeAttack] 리소스 정리 완료");
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        Gizmos.color = Color.cyan;
        Vector3 cubeExtents = new Vector3(
            cubeSize.x * voxelSpacing,
            cubeSize.y * voxelSpacing,
            cubeSize.z * voxelSpacing
        );
        Gizmos.DrawWireCube(transform.position, cubeExtents);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, cubeExtents * compactScale);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, cubeExtents * expandScale);
    }

    #endregion
}
