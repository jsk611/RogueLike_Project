using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 292���� ���̷��� ť�� ���� �ý��� - Base ���� ��ü ���
/// </summary>
public class CubeTransformationDirector : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform baseContainer;
    [SerializeField] private bool autoFindBaseContainer = true;

    [Header("Transformation Patterns")]
    [SerializeField] private TransformPattern currentPattern = TransformPattern.DissolveReassemble;
    [SerializeField] private float transformDuration = 3f;
    [SerializeField] private Ease transformEase = Ease.OutCubic;

    [Header("Dissolve Effect Settings")]
    [SerializeField] private float dissolveRadius = 8f;
    [SerializeField] private float dissolveDuration = 1.5f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Material virusInfectedMaterial;
    [SerializeField] private ParticleSystem virusParticlePrefab;
    [SerializeField] private bool useWaveDissolve = true;

    [Header("Cube Formation Settings")]
    [SerializeField] private Vector3 cubeCenter = Vector3.zero;
    [SerializeField] private float cubeSize = 6f;
    [SerializeField] private Vector3Int cubeGridSize = new Vector3Int(8, 8, 8);
    [SerializeField] private float voxelSpacing = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem assemblyEffect;
    [SerializeField] private ParticleSystem energyAura;
    [SerializeField] private ParticleSystem completionBurst;
    [SerializeField] private ParticleSystem dataStreamEffect;
    [SerializeField] private Light coreLight;
    [SerializeField] private AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 5);
    [SerializeField] private Color[] transformColors = { Color.cyan, Color.yellow, Color.red };

    [Header("Cyberpunk Effects")]
    [SerializeField] private Material hologramMaterial;
    [SerializeField] private Material glitchMaterial;
    [SerializeField] private bool enableCyberEffects = true;
    [SerializeField] private float glitchIntensity = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] transformSounds;
    [SerializeField] private AudioClip dissolveSound;
    [SerializeField] private AudioClip assembleSound;
    [SerializeField] private AudioClip completionSound;

    [Header("Performance")]
    [SerializeField] private int batchSize = 10;
    [SerializeField] private float batchDelay = 0.05f;

    [Header("Enhanced Transformation Effects")]
    [SerializeField] private bool enableCameraEffects = true;
    [SerializeField] private float cameraShakeDuration = 1f;
    [SerializeField] private float cameraShakeStrength = 0.5f;
    [SerializeField] private AnimationCurve transformProgressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private ParticleSystem anticipationEffect;
    [SerializeField] private ParticleSystem morphingEffect;
    [SerializeField] private ParticleSystem stabilizationEffect;
    
    [Header("Natural Transition Effects")]
    [SerializeField] private TransformationEffectManager effectManager;
    [SerializeField] private Material mimicPlantDissolveMaterial;
    [SerializeField] private bool useNaturalTransitions = true;
    
    [Header("Cyber Dust Effects")]
    [SerializeField] private CyberDustParticle cyberDustPrefab;
    [SerializeField] private bool enableCyberDust = true;
    [SerializeField] private float dustEmissionIntensity = 1f;
    
    [Header("Mimic Plant Effects")]
    [SerializeField] private MimicTransformationEffect mimicTransformEffect;
    [SerializeField] private bool useMimicPlantStyle = true;      // 의태식물 스타일 사용 여부
    
    [Header("Aurora Effects")]
    [SerializeField] private AuroraTransformationEffect auroraTransformEffect;
    [SerializeField] private bool useAuroraStyle = false;         // 오로라 스타일 사용 여부
    
    // 사이버 더스트 인스턴스들
    private List<CyberDustParticle> activeDustParticles = new List<CyberDustParticle>();

    // 변신 단계별 상태 관리
    private enum TransformationPhase
    {
        None,
        Anticipation,    // 예고/준비
        Dissolution,     // 해체
        Morphing,        // 변형
        Reassembly,      // 재조립
        Stabilization    // 안정화
    }

    private TransformationPhase currentTransformPhase = TransformationPhase.None;
    private float phaseStartTime = 0f;

    // ٽ Ʈ��
    private VoxelFloatEffect floatEffect;
    private VirusCubeStateManager stateManager;

    // ���� ����
    [SerializeField] private List<Transform> voxels = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> formationPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, int> voxelIndices = new Dictionary<Transform, int>();
    private Dictionary<Transform, Material> originalMaterials = new Dictionary<Transform, Material>();

    // ���� ���� ����
    private bool isTransforming = false;
    private bool isInCubeForm = false;
    private bool isDissolving = false;

    // ť�� ���� ��ǥ ����
    private List<Vector3> availableCubePositions = new List<Vector3>();

    public enum TransformPattern
    {
        DissolveReassemble,  // ���� �� ������ (�⺻)
        Spiral,              // ���������� ����
        Wave,                // �ĵ�ó�� ������ ����  
        Implosion,           // �߽����� �޼� ����
        Organic,             // ����üó�� �ڿ������� ����
        Glitch,              // �۸�ġ ȿ���� �Բ� ����
        Magnetic,            // �ڱ���ó�� �������� ȿ��
        Sequential,          // ������ ����
        Explosion,           // ���� �� ������
        VirusSpread,         // ̷  Ȯ 
        MimicPlantEmergence  // 의태식물 등장 연출
    }

    #region Initialization

    void Start()
    {
        InitializeComponents();
        FindOrAssignBaseContainer();
        CollectVoxelsFromBase();
        CalculateAllCubePositions();
        AssignFormationPositions();
        StoreOriginalMaterials();

        Debug.Log($"[EnhancedCubeTransformation] �ʱ�ȭ �Ϸ� - {voxels.Count}�� ���� �߰�");
    }

    private void InitializeComponents()
    {
        floatEffect = GetComponent<VoxelFloatEffect>();
        stateManager = GetComponent<VirusCubeStateManager>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        DOTween.Init();
    }

    private void FindOrAssignBaseContainer()
    {
        if (baseContainer == null && autoFindBaseContainer)
        {
            baseContainer = transform.Find("Base");
            if (baseContainer == null)
            {
                Debug.LogError("[EnhancedCubeTransformation] Base �����̳ʸ� ã�� �� �����ϴ�!");
                return;
            }
        }
        Debug.Log($"[EnhancedCubeTransformation] Base �����̳� ����: {baseContainer.name}");
    }

    private void CollectVoxelsFromBase()
    {
        if (baseContainer == null) return;

        voxels.Clear();
        originalPositions.Clear();
        voxelIndices.Clear();

        int index = 0;
        foreach (Transform child in baseContainer)
        {
            voxels.Add(child);
            originalPositions[child] = child.localPosition;
            voxelIndices[child] = index;
            index++;
        }

        Debug.Log($"[EnhancedCubeTransformation] {voxels.Count}�� ������ Base���� �����߽��ϴ�.");
    }

    private void StoreOriginalMaterials()
    {
        originalMaterials.Clear();
        foreach (var voxel in voxels)
        {
            var renderer = voxel.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials[voxel] = renderer.material;
            }
        }
    }

    private void CalculateAllCubePositions()
    {
        availableCubePositions.Clear();

        for (int x = 0; x < cubeGridSize.x; x++)
        {
            for (int y = 0; y < cubeGridSize.y; y++)
            {
                for (int z = 0; z < cubeGridSize.z; z++)
                {
                    bool isOnFace = (x == 0 || x == cubeGridSize.x - 1) ||
                                    (y == 0 || y == cubeGridSize.y - 1) ||
                                    (z == 0 || z == cubeGridSize.z - 1);

                    if (isOnFace)
                    {
                        Vector3 position = new Vector3(
                            (x - cubeGridSize.x / 2f + 0.5f) * voxelSpacing,
                            (y - cubeGridSize.y / 2f + 0.5f) * voxelSpacing,
                            (z - cubeGridSize.z / 2f + 0.5f) * voxelSpacing
                        ) + cubeCenter;

                        availableCubePositions.Add(position);
                    }
                }
            }
        }

        Debug.Log($"[EnhancedCubeTransformation] {availableCubePositions.Count}���� ť�� ��ġ ��� �Ϸ�");
    }

    private void AssignFormationPositions()
    {
        formationPositions.Clear();
        var assignedPositions = new HashSet<Vector3>();

        for (int i = 0; i < voxels.Count && i < availableCubePositions.Count; i++)
        {
            Transform voxel = voxels[i];
            Vector3 bestPosition = Vector3.zero;
            float minDistance = float.MaxValue;

            foreach (var pos in availableCubePositions)
            {
                if (assignedPositions.Contains(pos)) continue;

                float distance = Vector3.Distance(voxel.localPosition, pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestPosition = pos;
                }
            }

            if (bestPosition != Vector3.zero)
            {
                formationPositions[voxel] = bestPosition;
                assignedPositions.Add(bestPosition);
            }
        }

        Debug.Log($"[EnhancedCubeTransformation] {formationPositions.Count}�� ������ ���� ���� ��ġ �Ҵ� �Ϸ�");
    }

    #endregion

    #region Public API

        public void StartCubeTransformation()
    {
        if (isTransforming) return;
        
        Debug.Log($"[EnhancedCubeTransformation] 큐브 변신 시작 - 패턴: {currentPattern}");
        StartCoroutine(ExecutePatternTransformation());
    }

    public void StartSmoothTransformation(TransformPattern pattern = TransformPattern.VirusSpread)
    {
        if (isTransforming) return;
        
        SetTransformPattern(pattern);
        StartCoroutine(ExecuteSmoothTransformation());
    }

    public void RevertToOriginal()
    {
        if (isTransforming)
        {
            Debug.LogWarning("[EnhancedCubeTransformation] ���� �߿��� �ǵ��� �� �����ϴ�!");
            return;
        }

        Debug.Log("[EnhancedCubeTransformation] ���� ���� ����");
        StartCoroutine(ExecuteReversion());
    }

    public void SetTransformPattern(TransformPattern pattern)
    {
        currentPattern = pattern;
        Debug.Log($"[EnhancedCubeTransformation] ���� ���� ����: {pattern}");
    }

    public bool IsInCubeForm => isInCubeForm;
    public bool IsTransforming => isTransforming;

    #endregion

    #region Dissolve Effects

    /// <summary>
    /// ���̷��� ���� ��Ÿ�� ���� ȿ��
    /// </summary>
    private IEnumerator DissolveCubes(Transform epicenter = null)
    {
        isDissolving = true;

        if (epicenter == null)
            epicenter = transform;

        PlayDissolveSound();

        var sequence = DOTween.Sequence();
        var sortedVoxels = SortVoxelsByDistanceFromEpicenter(epicenter);

        foreach (var voxel in sortedVoxels)
        {
            float distance = Vector3.Distance(voxel.position, epicenter.position);
            float delay = useWaveDissolve ? (distance / dissolveRadius) * 0.3f : Random.Range(0f, 0.5f);
            float scaleDuration = dissolveDuration * dissolveCurve.Evaluate(distance / dissolveRadius);

            // ���̷��� ���� ȿ�� ����
            sequence.Insert(delay, CreateDissolveSequence(voxel, scaleDuration, delay));
        }

        yield return sequence.WaitForCompletion();
        isDissolving = false;
    }

    private Sequence CreateDissolveSequence(Transform voxel, float duration, float delay)
    {
        var sequence = DOTween.Sequence();

        // 1. ���̷��� ���� ��Ƽ���� ����
        sequence.AppendCallback(() => ApplyVirusEffect(voxel));

        // 2. �۸�ġ ȿ�� (���̹� �׸�)
        if (enableCyberEffects)
        {
            sequence.Append(CreateGlitchEffect(voxel, duration * 0.3f));
        }

        // 3. ���� ȿ�� (������ ���)
        sequence.Append(voxel.DOScale(Vector3.zero, duration * 0.7f)
            .SetEase(Ease.InBack)
            .OnComplete(() => PrepareForReconstruction(voxel)));

        return sequence;
    }

    private void ApplyVirusEffect(Transform voxel)
    {
        // 사이버 더스트 파티클 생성
        if (enableCyberDust && cyberDustPrefab != null)
        {
            CreateCyberDustAt(voxel.position);
        }
        
        // 기존 바이러스 감염 머티리얼 적용
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer != null && virusInfectedMaterial != null)
        {
            renderer.material = virusInfectedMaterial;

            if (virusInfectedMaterial.HasProperty("_GlitchStrength"))
            {
                virusInfectedMaterial.SetFloat("_GlitchStrength", glitchIntensity);
            }
        }

        // 바이러스 파티클 효과 생성
        if (virusParticlePrefab != null)
        {
            var particles = Instantiate(virusParticlePrefab, voxel.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, 3f);
        }

        // 데이터 스트림 효과
        if (dataStreamEffect != null)
        {
            StartCoroutine(CreateDataStreamToCenter(voxel));
        }
    }

    private Tween CreateGlitchEffect(Transform voxel, float duration)
    {
        var sequence = DOTween.Sequence();

        // ��ġ �۸�ġ
        for (int i = 0; i < 5; i++)
        {
            Vector3 glitchOffset = Random.insideUnitSphere * 0.2f;
            sequence.Append(voxel.DOLocalMove(voxel.localPosition + glitchOffset, duration / 10f))
                   .Append(voxel.DOLocalMove(voxel.localPosition, duration / 10f));
        }

        // ȸ�� �۸�ġ
        sequence.Join(voxel.DOLocalRotate(Random.insideUnitSphere * 360f, duration)
                          .SetEase(Ease.InOutQuad));

        return sequence;
    }

    private void PrepareForReconstruction(Transform voxel)
    {
        // �������� ���� ���� ó��
        voxel.gameObject.SetActive(false);
    }

    private IEnumerator CreateDataStreamToCenter(Transform voxel)
    {
        if (dataStreamEffect == null) yield break;

        Vector3 startPos = voxel.position;
        Vector3 endPos = transform.position + cubeCenter;

        var streamInstance = Instantiate(dataStreamEffect, startPos, Quaternion.identity);
        var trails = streamInstance.trails;
        trails.enabled = true;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            streamInstance.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        streamInstance.Stop();
        Destroy(streamInstance.gameObject, 2f);
    }

    #endregion

    #region Reconstruction Effects

    /// <summary>
    /// ť�� ���·� ������
    /// </summary>
    private IEnumerator ReconstructToCube()
    {
        PlayAssembleSound();

        // �߽ɿ��� ������ ���� ȿ��
        if (completionBurst != null)
        {
            completionBurst.transform.position = transform.position + cubeCenter;
            completionBurst.Play();
        }

        // �������� ť�� ��ġ�� ���ġ�ϰ� Ȱ��ȭ
        var reconstructionSequence = DOTween.Sequence();

        foreach (var voxel in voxels)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            // ���� Ȱ��ȭ �� ��ġ ����
            reconstructionSequence.AppendCallback(() => {
                voxel.gameObject.SetActive(true);
                voxel.localPosition = cubeCenter; // �߽ɿ��� ����
                voxel.localScale = Vector3.zero;

                // ���� ��Ƽ���� ���� (Ȧ�α׷� ȿ�� ����)
                RestoreOriginalMaterial(voxel, true);
            });

            // ��ǥ ��ġ�� �̵��ϸ� ������ ����
            reconstructionSequence.Append(voxel.DOLocalMove(targetPos, 0.8f).SetEase(Ease.OutElastic));
            reconstructionSequence.Join(voxel.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack));

            // �ణ�� ������ �־� ������ ������ ȿ��
            reconstructionSequence.AppendInterval(Random.Range(0.02f, 0.08f));
        }

        yield return reconstructionSequence.WaitForCompletion();
    }

    private void RestoreOriginalMaterial(Transform voxel, bool withHologramEffect = false)
    {
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer != null && originalMaterials.ContainsKey(voxel))
        {
            if (withHologramEffect && hologramMaterial != null)
            {
                renderer.material = hologramMaterial;

                // Ȧ�α׷� ȿ�� �Ķ���� ����
                if (hologramMaterial.HasProperty("_FresnelColor"))
                {
                    hologramMaterial.SetColor("_FresnelColor", transformColors[1]);
                }
            }
            else
            {
                renderer.material = originalMaterials[voxel];
            }
        }
    }

    #endregion

    #region Transformation Execution

    private IEnumerator ExecuteSmoothTransformation()
    {
        isTransforming = true;
        isInCubeForm = false;
        currentTransformPhase = TransformationPhase.Anticipation;
        phaseStartTime = Time.time;

        // 1단계: 예고/준비 효과
        yield return StartCoroutine(AnticipationPhase());

        // 2단계: 해체 단계
        currentTransformPhase = TransformationPhase.Dissolution;
        phaseStartTime = Time.time;
        yield return StartCoroutine(DissolutionPhase());

        // 3단계: 변형 단계
        currentTransformPhase = TransformationPhase.Morphing;
        phaseStartTime = Time.time;
        yield return StartCoroutine(MorphingPhase());

        // 4단계: 재조립 단계
        currentTransformPhase = TransformationPhase.Reassembly;
        phaseStartTime = Time.time;
        yield return StartCoroutine(ReassemblyPhase());

        // 5단계: 안정화 단계
        currentTransformPhase = TransformationPhase.Stabilization;
        phaseStartTime = Time.time;
        yield return StartCoroutine(StabilizationPhase());

        // 완료
        currentTransformPhase = TransformationPhase.None;
        isInCubeForm = true;
        isTransforming = false;
    }

    private IEnumerator AnticipationPhase()
    {
        Debug.Log("[CubeTransformation] 변신 예고 단계 시작");

        // 예고 효과
        if (anticipationEffect != null)
        {
            anticipationEffect.transform.position = transform.position;
            anticipationEffect.Play();
        }

        // 카메라 미묘한 진동
        if (enableCameraEffects)
        {
            //Camera.main?.transform?.DOShakePosition(0.5f, 0.1f, 20, 90, false, true);
        }

        // 라이트 효과 강화
        if (coreLight != null)
        {
            coreLight.DOIntensity(lightIntensityCurve.Evaluate(0.2f), 0.5f);
        }

        // 복셀들 미묘한 흔들림
        foreach (var voxel in voxels)
        {
            voxel.DOShakePosition(0.5f, 0.05f, 10, 90, false, true);
        }

        yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator DissolutionPhase()
    {
        Debug.Log("[CubeTransformation] 해체 단계 시작");

        // 해체 사운드
        PlayDissolveSound();

        // 해체 효과에 따라 다른 패턴 적용
        switch (currentPattern)
        {
            case TransformPattern.VirusSpread:
                yield return StartCoroutine(ViralDissolution());
                break;
            case TransformPattern.MimicPlantEmergence:
                yield return StartCoroutine(MimicPlantEmergenceTransformation());
                break;
            default:
                yield return StartCoroutine(StandardDissolution());
                break;
        }
    }

    private IEnumerator ViralDissolution()
    {
        // 바이러스식 해체: 감염 확산
        var sortedVoxels = SortVoxelsByDistanceFromCenter();

        if (sortedVoxels.Count > 0)
        {
            // 첫 감염
            yield return StartCoroutine(InfectAndDissolve(sortedVoxels[0], 0f));

            // 감염 확산
            int infectionRadius = 1;
            List<Transform> infected = new List<Transform> { sortedVoxels[0] };

            while (infected.Count < voxels.Count)
            {
                List<Transform> newInfections = new List<Transform>();

                foreach (var infectedVoxel in infected)
                {
                    foreach (var voxel in voxels)
                    {
                        if (!infected.Contains(voxel) && !newInfections.Contains(voxel))
                        {
                            float distance = Vector3.Distance(infectedVoxel.position, voxel.position);
                            if (distance <= infectionRadius * 2f)
                            {
                                newInfections.Add(voxel);
                            }
                        }
                    }
                }

                // 새로운 감염들을 동시에 처리
                foreach (var voxel in newInfections)
                {
                    StartCoroutine(InfectAndDissolve(voxel, Random.Range(0f, 0.3f)));
                }

                infected.AddRange(newInfections);
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    private IEnumerator InfectAndDissolve(Transform voxel, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 감염 효과
        yield return StartCoroutine(InfectVoxel(voxel, 0f));

        // 해체 효과
        voxel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        voxel.DOLocalRotate(Random.insideUnitSphere * 180f, 0.3f);
    }

    private IEnumerator StandardDissolution()
    {
        // 기존 해체 방식
        yield return StartCoroutine(DissolveCubes());
    }

    private IEnumerator MorphingPhase()
    {
        Debug.Log("[CubeTransformation] 변형 단계 시작");

        // 변형 효과
        if (morphingEffect != null)
        {
            morphingEffect.transform.position = transform.position;
            morphingEffect.Play();
        }

        // 에너지 집중 효과
        if (coreLight != null)
        {
            coreLight.DOIntensity(lightIntensityCurve.Evaluate(0.8f), 0.5f);
            coreLight.DOColor(Color.red, 0.5f);
        }

        // 강한 카메라 진동
        if (enableCameraEffects)
        {
            Camera.main?.transform?.DOShakePosition(cameraShakeDuration, cameraShakeStrength, 20, 90, false, true);
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ReassemblyPhase()
    {
        Debug.Log("[CubeTransformation] 재조립 단계 시작");

        // 재조립 사운드
        PlayAssembleSound();

        // 패턴에 따른 재조립
        yield return StartCoroutine(ExecutePatternTransformation());
    }

    private IEnumerator StabilizationPhase()
    {
        Debug.Log("[CubeTransformation] 안정화 단계 시작");

        // 안정화 효과
        if (stabilizationEffect != null)
        {
            stabilizationEffect.transform.position = transform.position;
            stabilizationEffect.Play();
        }

        // 라이트 안정화
        if (coreLight != null)
        {
            coreLight.DOIntensity(lightIntensityCurve.Evaluate(1f), 0.8f);
            coreLight.DOColor(Color.white, 0.8f);
        }

        // 최종 정렬
        yield return StartCoroutine(FinalizeFormation());

        // 완료 효과
        PlayCompletionEffects();

        yield return new WaitForSeconds(0.5f);
    }

    #endregion

    #region Pattern Implementations

    private IEnumerator ExecutePatternTransformation()
    {
        switch (currentPattern)
        {
            case TransformPattern.DissolveReassemble:
                yield return StartCoroutine(DissolveCubes());
                yield return StartCoroutine(ReconstructToCube());
                break;
            case TransformPattern.Spiral:
                yield return StartCoroutine(SpiralTransformation());
                break;
            case TransformPattern.Wave:
                yield return StartCoroutine(WaveTransformation());
                break;
            case TransformPattern.Implosion:
                yield return StartCoroutine(ImplosionTransformation());
                break;
            case TransformPattern.Organic:
                yield return StartCoroutine(OrganicTransformation());
                break;
            case TransformPattern.Glitch:
                yield return StartCoroutine(GlitchTransformation());
                break;
            case TransformPattern.Magnetic:
                yield return StartCoroutine(MagneticTransformation());
                break;
            case TransformPattern.Sequential:
                yield return StartCoroutine(SequentialTransformation());
                break;
            case TransformPattern.Explosion:
                yield return StartCoroutine(ExplosionTransformation());
                break;
            case TransformPattern.VirusSpread:
                yield return StartCoroutine(VirusSpreadTransformation());
                break;
            case TransformPattern.MimicPlantEmergence:
                yield return StartCoroutine(MimicPlantEmergenceTransformation());
                break;
        }
    }

    private IEnumerator SpiralTransformation()
    {
        var sortedVoxels = SortVoxelsByDistanceFromCenter();

        for (int i = 0; i < sortedVoxels.Count; i++)
        {
            var voxel = sortedVoxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;

            float delay = (i / (float)batchSize) * batchDelay;
            Vector3 spiralWaypoint = CalculateSpiralPath(i, sortedVoxels.Count);
            Vector3 targetPos = formationPositions[voxel];

            StartCoroutine(MoveVoxelAlongPath(voxel, spiralWaypoint, targetPos, delay));

            if (i % batchSize == 0)
                yield return new WaitForSeconds(batchDelay);
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator WaveTransformation()
    {
        var layeredVoxels = GroupVoxelsByLayer();

        for (int layerIndex = 0; layerIndex < layeredVoxels.Count; layerIndex++)
        {
            var layer = layeredVoxels[layerIndex];

            foreach (var voxel in layer)
            {
                if (!formationPositions.ContainsKey(voxel)) continue;

                Vector3 targetPos = formationPositions[voxel];
                Vector3 waveHeight = targetPos + Vector3.up * (3f + layerIndex * 0.5f);

                var sequence = DOTween.Sequence();
                sequence.Append(voxel.DOLocalMove(waveHeight, transformDuration * 0.4f).SetEase(Ease.OutQuad));
                sequence.Append(voxel.DOLocalMove(targetPos, transformDuration * 0.6f).SetEase(Ease.InOutBounce));
                voxel.DOLocalRotate(Vector3.zero, transformDuration);
            }

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator ImplosionTransformation()
    {
        foreach (var voxel in voxels)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            var moveTween = voxel.DOLocalMove(targetPos, transformDuration * 0.7f)
                                 .SetEase(Ease.InExpo);

            var scaleTween = voxel.DOScale(Vector3.one * 0.2f, transformDuration * 0.2f)
                                  .SetEase(Ease.InQuad)
                                  .OnComplete(() => {
                                      voxel.DOScale(Vector3.one, transformDuration * 0.5f)
                                           .SetEase(Ease.OutBounce);
                                  });
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator OrganicTransformation()
    {
        var clusters = CreateVoxelClusters();

        foreach (var cluster in clusters)
        {
            yield return StartCoroutine(GrowCluster(cluster));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }

    private IEnumerator GlitchTransformation()
    {
        if (floatEffect != null)
        {
            floatEffect.SetGlitchMode(true, 3f);
        }

        var shuffledVoxels = new List<Transform>(voxels);
        for (int i = 0; i < shuffledVoxels.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledVoxels.Count);
            var temp = shuffledVoxels[i];
            shuffledVoxels[i] = shuffledVoxels[randomIndex];
            shuffledVoxels[randomIndex] = temp;
        }

        foreach (var voxel in shuffledVoxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                StartCoroutine(GlitchTeleport(voxel));
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
            }
        }

        yield return new WaitForSeconds(transformDuration);

        if (floatEffect != null)
        {
            floatEffect.SetGlitchMode(false);
        }
    }

    private IEnumerator MagneticTransformation()
    {
        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                StartCoroutine(MagneticPull(voxel));
                yield return new WaitForSeconds(0.02f);
            }
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator SequentialTransformation()
    {
        for (int i = 0; i < voxels.Count; i++)
        {
            var voxel = voxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            voxel.DOLocalMove(targetPos, 0.5f).SetEase(Ease.OutBack);
            voxel.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutQuad)
                 .OnComplete(() => voxel.DOScale(Vector3.one, 0.2f));

            if (i % batchSize == 0)
                yield return new WaitForSeconds(batchDelay);
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ExplosionTransformation()
    {
        // 1단계: 폭발하여 흩어짐
        foreach (var voxel in voxels)
        {
            Vector3 explosionDirection = Random.insideUnitSphere.normalized;
            Vector3 explosionPos = voxel.localPosition + explosionDirection * Random.Range(5f, 10f);

            voxel.DOLocalMove(explosionPos, transformDuration * 0.3f).SetEase(Ease.OutExpo);
            voxel.DORotate(Random.insideUnitSphere * 360f, transformDuration * 0.3f);
        }

        yield return new WaitForSeconds(transformDuration * 0.4f);

        // 2단계: 큐브 형태로 재조립
        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                Vector3 targetPos = formationPositions[voxel];
                voxel.DOLocalMove(targetPos, transformDuration * 0.6f).SetEase(Ease.InOutCubic);
                voxel.DOLocalRotate(Vector3.zero, transformDuration * 0.6f);
            }
        }

        yield return new WaitForSeconds(transformDuration * 0.6f);
    }

    private IEnumerator VirusSpreadTransformation()
    {
        isTransforming = true;
        isInCubeForm = false;

        PlayRandomTransformSound();

        // 바이러스 확산 패턴: 중심부터 감염이 퍼져나가는 방식
        Vector3 centerWorld = transform.TransformPoint(cubeCenter);
        var sortedVoxels = SortVoxelsByDistanceFromCenter();

        // 감염 시작점 (중심에서 가장 가까운 복셀)
        if (sortedVoxels.Count > 0)
        {
            Transform patientZero = sortedVoxels[0];
            
            // 1단계: 첫 감염
            yield return StartCoroutine(InfectVoxel(patientZero, 0f));
            
            // 2단계: 감염 확산 (웨이브 형태로)
            int waveSize = 3;
            int currentWave = 1;
            
            while (currentWave * waveSize < sortedVoxels.Count)
            {
                int startIdx = currentWave * waveSize;
                int endIdx = Mathf.Min(startIdx + waveSize, sortedVoxels.Count);
                
                // 동시에 여러 복셀 감염
                for (int i = startIdx; i < endIdx; i++)
                {
                    float waveDelay = (i - startIdx) * 0.1f;
                    StartCoroutine(InfectVoxel(sortedVoxels[i], waveDelay));
                }
                
                yield return new WaitForSeconds(0.5f);
                currentWave++;
            }
            
            // 3단계: 모든 감염된 복셀들이 큐브 형태로 재배열
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(VirusReassembly());
        }

        yield return StartCoroutine(FinalizeFormation());
        PlayCompletionEffects();

        isInCubeForm = true;
        isTransforming = false;
    }
    
    private IEnumerator InfectVoxel(Transform voxel, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 감염 효과: 빨간색으로 변하며 진동
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer != null && virusInfectedMaterial != null)
        {
            renderer.material = virusInfectedMaterial;
        }
        
        // 감염 진동 효과
        voxel.DOShakePosition(0.3f, 0.2f, 10, 90, false, true);
        
        // 감염 파티클 효과
        if (virusParticlePrefab != null)
        {
            var particle = Instantiate(virusParticlePrefab, voxel.position, Quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, 2f);
        }
        
        // 스케일 펄스 효과
        var originalScale = voxel.localScale;
        voxel.DOScale(originalScale * 1.3f, 0.2f)
             .SetEase(Ease.OutQuad)
             .OnComplete(() => {
                 voxel.DOScale(originalScale, 0.3f).SetEase(Ease.InQuad);
             });
    }
    
    private IEnumerator VirusReassembly()
    {
        // 감염된 복셀들이 큐브 형태로 재조립
        foreach (var voxel in voxels)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;
            
            Vector3 targetPos = formationPositions[voxel];
            float randomDelay = Random.Range(0f, 0.8f);
            
            // 복셀들이 서로 다른 경로로 목표 지점에 도달
            Vector3 currentPos = voxel.localPosition;
            Vector3 midPoint = Vector3.Lerp(currentPos, targetPos, 0.5f) + 
                              Random.insideUnitSphere * 2f;
            
            // 곡선 경로로 이동
            var sequence = DOTween.Sequence();
            sequence.SetDelay(randomDelay);
            sequence.Append(voxel.DOLocalMove(midPoint, transformDuration * 0.4f).SetEase(Ease.OutQuad));
            sequence.Append(voxel.DOLocalMove(targetPos, transformDuration * 0.6f).SetEase(Ease.InOutCubic));
            
            // 회전 효과
            sequence.Join(voxel.DOLocalRotate(Vector3.zero, transformDuration).SetEase(Ease.OutQuart));
            
            // 재료질 복원 (감염 해제)
            sequence.OnComplete(() => RestoreOriginalMaterial(voxel, true));
        }
        
        yield return new WaitForSeconds(transformDuration);
    }

    /// <summary>
    /// 의태식물 등장 연출 - 자연스러운 숨김과 폭발적 등장
    /// </summary>
    private IEnumerator MimicPlantEmergenceTransformation()
    {
        Debug.Log("[MimicPlant] 의태식물 등장 연출 시작");
        
        // 오로라 효과 우선 사용
        if (useAuroraStyle && auroraTransformEffect != null)
        {
            yield return StartCoroutine(NewAuroraTransformation());
        }
        // 새로운 의태식물 효과 사용
        else if (useMimicPlantStyle && mimicTransformEffect != null)
        {
            yield return StartCoroutine(NewMimicPlantTransformation());
        }
        else
        {
            // 기존 방식 사용
            // 1단계: 지하로 숨기기 (완전히 자연스러운 디졸브)
            yield return StartCoroutine(NaturalDissolveToGround());
            
            // 2단계: 지면 효과 및 감지 연출
            yield return StartCoroutine(GroundDetectionPhase());
            
            // 3단계: 폭발적 등장
            yield return StartCoroutine(ExplosiveEmergenceFromGround());
        }
        
        Debug.Log("[MimicPlant] 의태식물 등장 연출 완료");
    }
    
    /// <summary>
    /// 새로운 의태식물 변신 효과 - 뿌연 연기로 자연스럽게 가리고 변신
    /// </summary>
    private IEnumerator NewMimicPlantTransformation()
    {
        Debug.Log("[MimicPlant] 새로운 의태식물 변신 시작");
        
        // 효과 영역 자동 설정
        Vector3 transformArea = CalculateVoxelBounds();
        mimicTransformEffect.SetEffectArea(transformArea);
        
        bool transformCompleted = false;
        
        // 뿌연 연기 효과 시작 (콜백으로 실제 변신 실행)
        mimicTransformEffect.StartMimicTransformation(() => {
            StartCoroutine(ExecuteMimicTransformation(() => {
                transformCompleted = true;
            }));
        });
        
        // 변신이 완료될 때까지 대기
        yield return new WaitUntil(() => transformCompleted);
        
        Debug.Log("[MimicPlant] 새로운 의태식물 변신 완료");
    }
    
    /// <summary>
    /// 연기 속에서 실제 변신 실행
    /// </summary>
    private IEnumerator ExecuteMimicTransformation(System.Action onComplete)
    {
        Debug.Log("[MimicPlant] 연기 속에서 변신 실행");
        
        // 1단계: 원본 voxel들 즉시 비활성화 (연기가 가려준 상태)
        foreach (var voxel in voxels)
        {
            if (voxel != null) voxel.gameObject.SetActive(false);
        }
        
        yield return new WaitForSeconds(0.3f); // 잠시 대기
        
        // 2단계: 새로운 형태로 즉시 배치 (여전히 연기 속)
        var sortedVoxels = SortVoxelsByDistanceFromCenter();
        
        for (int i = 0; i < sortedVoxels.Count; i++)
        {
            var voxel = sortedVoxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;
            
            Vector3 targetPos = formationPositions[voxel];
            
            // 즉시 큐브 위치로 이동 (연기가 가려져 있어서 안 보임)
            voxel.localPosition = targetPos;
            voxel.gameObject.SetActive(true);
            
            // 작은 딜레이로 자연스러운 등장
            yield return new WaitForSeconds(0.02f);
        }
        
        // 완료 사운드
        PlayAssembleSound();
        
        // 상태 업데이트
        isInCubeForm = true;
        isTransforming = false;
        
        onComplete?.Invoke();
        
        Debug.Log("[MimicPlant] 연기 속 변신 실행 완료");
    }
    
    /// <summary>
    /// Voxel들의 전체 바운딩 박스 계산
    /// </summary>
    private Vector3 CalculateVoxelBounds()
    {
        if (voxels == null || voxels.Count == 0)
            return new Vector3(6f, 4f, 6f); // 기본 크기
        
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool boundsInitialized = false;
        
        foreach (var voxel in voxels)
        {
            if (voxel == null) continue;
            
            Vector3 worldPos = voxel.position;
            
            if (!boundsInitialized)
            {
                bounds = new Bounds(worldPos, Vector3.one);
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(worldPos);
            }
        }
        
        // 큐브 형성 위치들도 포함
        foreach (var formationPos in formationPositions.Values)
        {
            Vector3 worldPos = transform.TransformPoint(formationPos);
            bounds.Encapsulate(worldPos);
        }
        
        // 최소 크기 보장 및 여유 공간 추가
        Vector3 size = bounds.size;
        size.x = Mathf.Max(size.x, 3f) + 3f;  // 여유 공간
        size.y = Mathf.Max(size.y, 3f) + 2f;
        size.z = Mathf.Max(size.z, 3f) + 3f;
        
        Debug.Log($"[MimicPlant] 계산된 변신 영역: {size}");
        return size;
    }
    
    /// <summary>
    /// 새로운 오로라 변신 효과 - 신비로운 오로라로 가리고 변신
    /// </summary>
    private IEnumerator NewAuroraTransformation()
    {
        Debug.Log("[Aurora] 새로운 오로라 변신 시작");
        
        // 효과 영역 자동 설정
        Vector3 transformArea = CalculateVoxelBounds();
        auroraTransformEffect.SetEffectArea(transformArea);
        
        bool transformCompleted = false;
        
        // 오로라 효과 시작 (콜백으로 실제 변신 실행)
        auroraTransformEffect.StartAuroraTransformation(() => {
            StartCoroutine(ExecuteAuroraTransformation(() => {
                transformCompleted = true;
            }));
        });
        
        // 변신이 완료될 때까지 대기
        yield return new WaitUntil(() => transformCompleted);
        
        Debug.Log("[Aurora] 새로운 오로라 변신 완료");
    }
    
    /// <summary>
    /// 오로라 속에서 실제 변신 실행
    /// </summary>
    private IEnumerator ExecuteAuroraTransformation(System.Action onComplete)
    {
        Debug.Log("[Aurora] 오로라 속에서 변신 실행");
        
        // 1단계: 원본 voxel들 즉시 비활성화 (오로라가 가려준 상태)
        foreach (var voxel in voxels)
        {
            if (voxel != null) voxel.gameObject.SetActive(false);
        }
        
        yield return new WaitForSeconds(0.4f); // 잠시 대기 (오로라 커튼 효과)
        
        // 2단계: 새로운 형태로 즉시 배치 (여전히 오로라 속)
        var sortedVoxels = SortVoxelsByDistanceFromCenter();
        
        for (int i = 0; i < sortedVoxels.Count; i++)
        {
            var voxel = sortedVoxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;
            
            Vector3 targetPos = formationPositions[voxel];
            
            // 즉시 큐브 위치로 이동 (오로라가 가려져 있어서 안 보임)
            voxel.localPosition = targetPos;
            voxel.gameObject.SetActive(true);
            
            // 작은 딜레이로 자연스러운 등장
            yield return new WaitForSeconds(0.02f);
        }
        
        // 완료 사운드
        PlayAssembleSound();
        
        // 상태 업데이트
        isInCubeForm = true;
        isTransforming = false;
        
        onComplete?.Invoke();
        
        Debug.Log("[Aurora] 오로라 속 변신 실행 완료");
    }
    
    /// <summary>
    /// 1단계: 자연스럽게 지하로 디졸브하며 숨기기
    /// </summary>
    private IEnumerator NaturalDissolveToGround()
    {
        Debug.Log("[MimicPlant] 지하로 숨기기 시작");
        
        // 각 복셀에 디졸브 머티리얼 적용
        foreach (var voxel in voxels)
        {
            if (useNaturalTransitions && mimicPlantDissolveMaterial != null)
            {
                var renderer = voxel.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = mimicPlantDissolveMaterial;
                    
                    // 디졸브 파라미터 초기화
                    if (mimicPlantDissolveMaterial.HasProperty("_DissolveAmount"))
                    {
                        mimicPlantDissolveMaterial.SetFloat("_DissolveAmount", 0f);
                    }
                    if (mimicPlantDissolveMaterial.HasProperty("_GroundLevel"))
                    {
                        mimicPlantDissolveMaterial.SetFloat("_GroundLevel", transform.position.y);
                    }
                }
            }
        }
        
        // 이펙트 매니저로 자연스러운 디졸브 실행
        if (effectManager != null)
        {
            foreach (var voxel in voxels)
            {
                effectManager.PlayDigitalDissolve(voxel, dissolveDuration);
                yield return new WaitForSeconds(0.03f); // 약간의 딜레이로 자연스러운 흐름
            }
        }
        else
        {
            // 기본 디졸브 (이펙트 매니저가 없는 경우)
            yield return StartCoroutine(BasicGroundDissolve());
        }
        
        yield return new WaitForSeconds(dissolveDuration);
        
        // 모든 복셀을 지하 위치로 이동 후 비활성화
        foreach (var voxel in voxels)
        {
            voxel.localPosition = voxel.localPosition + Vector3.down * 3f; // 지하로 이동
            voxel.gameObject.SetActive(false);
        }
        
        Debug.Log("[MimicPlant] 지하 숨김 완료");
    }
    
    /// <summary>
    /// 2단계: 지면 감지 및 경고 효과
    /// </summary>
    private IEnumerator GroundDetectionPhase()
    {
        Debug.Log("[MimicPlant] 지면 감지 단계 시작");
        
        Vector3 emergenceCenter = transform.position + cubeCenter;
        
        // 이펙트 매니저로 의태식물 등장 효과 실행
        if (effectManager != null)
        {
            effectManager.PlayMimicPlantEmergence(emergenceCenter, 2f);
        }
        else
        {
            // 기본 경고 효과
            yield return StartCoroutine(BasicWarningEffect(emergenceCenter));
        }
        
        yield return new WaitForSeconds(2f);
        Debug.Log("[MimicPlant] 지면 감지 완료");
    }
    
    /// <summary>
    /// 3단계: 폭발적 등장 및 큐브 형성
    /// </summary>
    private IEnumerator ExplosiveEmergenceFromGround()
    {
        Debug.Log("[MimicPlant] 폭발적 등장 시작");
        
        Vector3 emergenceCenter = transform.position + cubeCenter;
        
        // 강한 카메라 진동
        if (enableCameraEffects && Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(0.8f, 1.2f, 30, 90, false, true);
        }
        
        // 등장 사운드
        PlayAssembleSound();
        
        // 중심에서 바깥쪽으로 확산되며 등장
        var sortedVoxels = SortVoxelsByDistanceFromCenter();
        
        for (int i = 0; i < sortedVoxels.Count; i++)
        {
            var voxel = sortedVoxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;
            
            Vector3 targetPos = formationPositions[voxel];
            float emergenceDelay = (i / (float)sortedVoxels.Count) * 0.8f;
            
            StartCoroutine(EmergeSingleVoxel(voxel, targetPos, emergenceDelay));
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // 최종 정렬 및 안정화
        yield return StartCoroutine(FinalizeFormation());
        PlayCompletionEffects();
        
        Debug.Log("[MimicPlant] 폭발적 등장 완료");
    }
    
    /// <summary>
    /// 개별 복셀의 등장 연출
    /// </summary>
    private IEnumerator EmergeSingleVoxel(Transform voxel, Vector3 targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 활성화 및 지하에서 시작
        voxel.gameObject.SetActive(true);
        voxel.localPosition = targetPos + Vector3.down * 4f;
        voxel.localScale = Vector3.zero;
        
        // 홀로그램 재조립 효과
        if (effectManager != null && useNaturalTransitions)
        {
            effectManager.PlayHologramReassembly(voxel, targetPos, 0.8f);
        }
        
        // 폭발적 등장 애니메이션
        var sequence = DOTween.Sequence();
        
        // 1. 땅에서 튀어나오듯 빠르게 상승
        sequence.Append(voxel.DOLocalMove(targetPos + Vector3.up * 1f, 0.3f)
                            .SetEase(Ease.OutBack));
        
        // 2. 크기 확장 (약간의 오버슈트)
        sequence.Join(voxel.DOScale(Vector3.one * 1.3f, 0.3f)
                          .SetEase(Ease.OutElastic));
        
        // 3. 목표 위치로 안착
        sequence.Append(voxel.DOLocalMove(targetPos, 0.2f)
                            .SetEase(Ease.OutQuad));
        
        // 4. 정상 크기로 복원
        sequence.Join(voxel.DOScale(Vector3.one, 0.2f)
                          .SetEase(Ease.OutBounce));
        
        // 머티리얼 복원
        yield return new WaitForSeconds(0.6f);
        RestoreOriginalMaterial(voxel, false);
    }
    
    /// <summary>
    /// 기본 지면 디졸브 (이펙트 매니저가 없는 경우)
    /// </summary>
    private IEnumerator BasicGroundDissolve()
    {
        foreach (var voxel in voxels)
        {
            // 기본 스케일 축소 애니메이션
            voxel.DOScale(Vector3.zero, dissolveDuration * 0.8f)
                 .SetEase(Ease.InBack);
            
            // 지하로 이동
            voxel.DOLocalMove(voxel.localPosition + Vector3.down * 2f, dissolveDuration)
                 .SetEase(Ease.InQuad);
                 
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    /// <summary>
    /// 기본 경고 효과 (이펙트 매니저가 없는 경우)
    /// </summary>
    private IEnumerator BasicWarningEffect(Vector3 center)
    {
        // 미묘한 진동 효과
        if (enableCameraEffects && Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(1.5f, 0.1f, 20, 90, false, true);
        }
        
        // 라이트 효과
        if (coreLight != null)
        {
            coreLight.transform.position = center;
            coreLight.color = Color.yellow;
            coreLight.enabled = true;
            
            // 깜빡이는 경고 효과
            for (int i = 0; i < 6; i++)
            {
                coreLight.DOIntensity(3f, 0.15f).OnComplete(() => {
                    coreLight.DOIntensity(0.5f, 0.15f);
                });
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }

    #endregion

    #region Helper Methods

    private List<Transform> SortVoxelsByDistanceFromEpicenter(Transform epicenter)
    {
        var sorted = new List<Transform>(voxels);
        sorted.Sort((a, b) => {
            float distA = Vector3.Distance(a.position, epicenter.position);
            float distB = Vector3.Distance(b.position, epicenter.position);
            return distA.CompareTo(distB);
        });
        return sorted;
    }

    private List<Transform> SortVoxelsByDistanceFromCenter()
    {
        var sorted = new List<Transform>(voxels);
        Vector3 centerWorld = transform.TransformPoint(cubeCenter);

        sorted.Sort((a, b) => {
            float distA = Vector3.Distance(a.position, centerWorld);
            float distB = Vector3.Distance(b.position, centerWorld);
            return distA.CompareTo(distB);
        });

        return sorted;
    }

    private Vector3 CalculateSpiralPath(int index, int totalCount)
    {
        float progress = index / (float)totalCount;
        float angle = progress * Mathf.PI * 6f; // 3번의 회전
        float radius = 4f * (1f - progress);
        float height = Mathf.Sin(progress * Mathf.PI) * 3f;

        return cubeCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            height,
            Mathf.Sin(angle) * radius
        );
    }

    private List<List<Transform>> GroupVoxelsByLayer()
    {
        var layers = new List<List<Transform>>();
        var sorted = new List<Transform>(voxels);

        sorted.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        float layerThickness = 1f;
        var currentLayer = new List<Transform>();
        float currentY = sorted[0].localPosition.y;

        foreach (var voxel in sorted)
        {
            if (Mathf.Abs(voxel.localPosition.y - currentY) > layerThickness)
            {
                if (currentLayer.Count > 0)
                {
                    layers.Add(currentLayer);
                    currentLayer = new List<Transform>();
                }
                currentY = voxel.localPosition.y;
            }
            currentLayer.Add(voxel);
        }

        if (currentLayer.Count > 0)
            layers.Add(currentLayer);

        return layers;
    }

    private IEnumerator MoveVoxelAlongPath(Transform voxel, Vector3 waypoint, Vector3 target, float delay)
    {
        yield return new WaitForSeconds(delay);

        var sequence = DOTween.Sequence();
        sequence.Append(voxel.DOLocalMove(waypoint, transformDuration * 0.4f).SetEase(Ease.OutQuad));
        sequence.Append(voxel.DOLocalMove(target, transformDuration * 0.6f).SetEase(Ease.InOutCubic));
        sequence.Join(voxel.DOLocalRotate(Vector3.zero, transformDuration));
    }

    private IEnumerator GlitchTeleport(Transform voxel)
    {
        if (!formationPositions.ContainsKey(voxel)) yield break;

        Vector3 targetPos = formationPositions[voxel];
        Vector3 glitchOffset = Random.insideUnitSphere * 1f;

        voxel.DOScale(Vector3.zero, 0.1f).OnComplete(() => {
            voxel.localPosition = targetPos + glitchOffset;
            voxel.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            voxel.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutElastic);
        });

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator MagneticPull(Transform voxel)
    {
        if (!formationPositions.ContainsKey(voxel)) yield break;

        Vector3 targetPos = formationPositions[voxel];
        Vector3 currentPos = voxel.localPosition;

        Vector3 midPoint1 = Vector3.Lerp(currentPos, targetPos, 0.33f) + Random.insideUnitSphere * 2f;
        Vector3 midPoint2 = Vector3.Lerp(currentPos, targetPos, 0.66f) + Random.insideUnitSphere * 1f;

        Vector3[] path = new Vector3[] { currentPos, midPoint1, midPoint2, targetPos };

        voxel.DOLocalPath(path, transformDuration * 0.8f, PathType.CatmullRom)
             .SetEase(Ease.InOutQuad);

        yield return null;
    }

    private List<List<Transform>> CreateVoxelClusters()
    {
        var clusters = new List<List<Transform>>();
        var remaining = new List<Transform>(voxels);

        while (remaining.Count > 0)
        {
            var cluster = new List<Transform>();
            var seed = remaining[Random.Range(0, remaining.Count)];
            cluster.Add(seed);
            remaining.Remove(seed);

            int clusterSize = Random.Range(5, 15);
            for (int i = 0; i < clusterSize && remaining.Count > 0; i++)
            {
                Transform closest = null;
                float minDist = float.MaxValue;

                foreach (var voxel in remaining)
                {
                    float dist = Vector3.Distance(seed.localPosition, voxel.localPosition);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = voxel;
                    }
                }

                if (closest != null)
                {
                    cluster.Add(closest);
                    remaining.Remove(closest);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private IEnumerator GrowCluster(List<Transform> cluster)
    {
        foreach (var voxel in cluster)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            voxel.DOScale(Vector3.zero, 0.1f).OnComplete(() => {
                voxel.localPosition = targetPos;
                voxel.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutElastic);
            });

            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
        }
    }

    #endregion

    #region Finalization & Effects

    private IEnumerator FinalizeFormation()
    {
        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                voxel.DOLocalMove(formationPositions[voxel], 0.3f).SetEase(Ease.OutQuad);
                voxel.DOLocalRotate(Vector3.zero, 0.3f);
                voxel.DOScale(Vector3.one, 0.2f);
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void PlayCompletionEffects()
    {
        if (completionBurst != null)
        {
            completionBurst.transform.position = transform.position + cubeCenter;
            completionBurst.Play();
        }

        if (energyAura != null)
        {
            energyAura.transform.position = transform.position + cubeCenter;
            energyAura.Play();
        }

        if (coreLight != null)
        {
            coreLight.color = transformColors[2];
            coreLight.DOIntensity(8f, 0.2f).OnComplete(() => {
                coreLight.DOIntensity(3f, 0.8f);
            });
        }

        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }

        if (floatEffect != null)
        {
            floatEffect.SetAttackMode(true);
        }

        Debug.Log("[EnhancedCubeTransformation] ť�� ���� �Ϸ�!");
    }

    #endregion

    #region Reversion

    private IEnumerator ExecuteReversion()
    {
        isTransforming = true;

        Debug.Log("[EnhancedCubeTransformation] ���� ���� ����");

        PrepareReversion();
        yield return new WaitForSeconds(0.3f);

        // ������ ����: ť�꿡�� ��������
        yield return StartCoroutine(ReverseDissolution());

        CompleteReversion();

        isInCubeForm = false;
        isTransforming = false;

        Debug.Log("[EnhancedCubeTransformation] ���� ���� �Ϸ�!");
    }

    private void PrepareReversion()
    {
        if (coreLight != null)
        {
            coreLight.color = transformColors[0];
            coreLight.DOIntensity(1f, 0.5f);
        }

        PlayRandomTransformSound();
    }

    private IEnumerator ReverseDissolution()
    {
        // 1�ܰ�: ť�� ���¿��� �߽����� ����
        var contractionSequence = DOTween.Sequence();

        foreach (var voxel in voxels)
        {
            if (originalPositions.ContainsKey(voxel))
            {
                // �߽����� �̵��ϸ� ������ ���
                contractionSequence.Join(voxel.DOLocalMove(cubeCenter, transformDuration * 0.4f)
                    .SetEase(Ease.InQuad));
                contractionSequence.Join(voxel.DOScale(Vector3.zero, transformDuration * 0.3f)
                    .SetEase(Ease.InBack));
            }
        }

        yield return contractionSequence.WaitForCompletion();

        // 2�ܰ�: ���� ��ġ�� Ȯ���ϸ� ����
        var expansionSequence = DOTween.Sequence();

        foreach (var voxel in voxels)
        {
            if (originalPositions.ContainsKey(voxel))
            {
                Vector3 originalPos = originalPositions[voxel];

                // ���� ��Ƽ���� ����
                expansionSequence.AppendCallback(() => RestoreOriginalMaterial(voxel, false));

                // ���� ��ġ�� Ȯ��
                expansionSequence.Join(voxel.DOLocalMove(originalPos, transformDuration * 0.6f)
                    .SetEase(Ease.OutElastic));
                expansionSequence.Join(voxel.DOScale(Vector3.one, transformDuration * 0.5f)
                    .SetEase(Ease.OutBack));

                // ������ ���� ȿ��
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
            }
        }

        yield return new WaitForSeconds(transformDuration * 0.6f);
    }

    private void CompleteReversion()
    {
        if (coreLight != null)
        {
            coreLight.DOIntensity(0f, 0.5f).OnComplete(() => {
                coreLight.enabled = false;
            });
        }

        if (assemblyEffect != null && assemblyEffect.isPlaying)
        {
            assemblyEffect.Stop();
        }

        if (energyAura != null && energyAura.isPlaying)
        {
            energyAura.Stop();
        }

        if (floatEffect != null)
        {
            floatEffect.SetFloatIntensity(1f);
            floatEffect.SetAttackMode(false);
        }
    }

    #endregion

    #region Audio

    private void PlayRandomTransformSound()
    {
        if (audioSource != null && transformSounds.Length > 0)
        {
            var sound = transformSounds[Random.Range(0, transformSounds.Length)];
            audioSource.PlayOneShot(sound);
        }
    }

    private void PlayDissolveSound()
    {
        if (audioSource != null && dissolveSound != null)
        {
            audioSource.PlayOneShot(dissolveSound);
        }
    }

    private void PlayAssembleSound()
    {
        if (audioSource != null && assembleSound != null)
        {
            audioSource.PlayOneShot(assembleSound);
        }
    }

    #endregion

    #region Debug & Testing

    [Header("Debug & Testing")]
    [SerializeField] private bool enableTestKeys = true;
    [SerializeField] private bool showDebugGizmos = true;

    void Update()
    {
        if (enableTestKeys && Application.isPlaying)
        {
            HandleTestInput();
        }

        if (isInCubeForm && coreLight != null && lightIntensityCurve != null)
        {
            float normalizedTime = (Time.time % 4f) / 4f;
            float intensity = lightIntensityCurve.Evaluate(normalizedTime) * 3f + 2f;
            coreLight.intensity = intensity;
        }
    }

    private void HandleTestInput()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    if (!isTransforming)
        //    {
        //        if (isInCubeForm)
        //        {
        //            Debug.Log("=== KŰ ����: ���� ���� ���� ===");
        //            RevertToOriginal();
        //        }
        //        else
        //        {
        //            Debug.Log("=== KŰ ����: ť�� ���� ���� ===");
        //            StartCubeTransformation();
        //        }
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    if (!isTransforming && isInCubeForm)
        //    {
        //        Debug.Log("=== RŰ ����: ���� ���� ���� ===");
        //        RevertToOriginal();
        //    }
        //}

        //// ���� ����
        //if (Input.GetKeyDown(KeyCode.Alpha1)) { SetTransformPattern(TransformPattern.DissolveReassemble); }
        //if (Input.GetKeyDown(KeyCode.Alpha2)) { SetTransformPattern(TransformPattern.Spiral); }
        //if (Input.GetKeyDown(KeyCode.Alpha3)) { SetTransformPattern(TransformPattern.Wave); }
        //if (Input.GetKeyDown(KeyCode.Alpha4)) { SetTransformPattern(TransformPattern.Implosion); }
        //if (Input.GetKeyDown(KeyCode.Alpha5)) { SetTransformPattern(TransformPattern.Organic); }
        //if (Input.GetKeyDown(KeyCode.Alpha6)) { SetTransformPattern(TransformPattern.Glitch); }
        //if (Input.GetKeyDown(KeyCode.Alpha7)) { SetTransformPattern(TransformPattern.Magnetic); }
        //if (Input.GetKeyDown(KeyCode.Alpha8)) { SetTransformPattern(TransformPattern.Sequential); }
        //if (Input.GetKeyDown(KeyCode.Alpha9)) { SetTransformPattern(TransformPattern.Explosion); }
        //if (Input.GetKeyDown(KeyCode.Alpha0)) { SetTransformPattern(TransformPattern.VirusSpread); }
        //if (Input.GetKeyDown(KeyCode.Minus)) { SetTransformPattern(TransformPattern.MimicPlantEmergence); }

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    Debug.Log($"=== ���� ���� ===");
        //    Debug.Log($"��ü ���� ��: {voxels.Count}");
        //    Debug.Log($"���� ��ġ �Ҵ� ��: {formationPositions.Count}");
        //    Debug.Log($"��� ������ ť�� ��ġ ��: {availableCubePositions.Count}");
        //    Debug.Log($"���� ����: {(isInCubeForm ? "ť��" : "����")}");
        //    Debug.Log($"���� ��: {isTransforming}");
        //    Debug.Log($"���� ��: {isDissolving}");
        //    Debug.Log($"���� ����: {currentPattern}");
        //}
        
        //// 사이버 더스트 테스트 (D키)
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    TestCyberDustEffect();
        //}
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // ť�� ���� �̸�����
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + cubeCenter, Vector3.one * cubeSize);

        // ť�� �߽���
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + cubeCenter, 0.2f);

        // Base �����̳�
        if (baseContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(baseContainer.position, Vector3.one * 0.5f);
        }

        // ���� �ݰ� ǥ��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + cubeCenter, dissolveRadius);

        // ��� ������ ť�� ��ġ��
        if (availableCubePositions.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in availableCubePositions)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(pos), 0.05f);
            }
        }

        // ���� �Ҵ�� ���� ��ġ��
        if (formationPositions.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var pos in formationPositions.Values)
            {
                Gizmos.DrawSphere(transform.TransformPoint(pos), 0.08f);
            }
        }

        // �������� ���� ��ġ
        if (voxels.Count > 0)
        {
            Gizmos.color = isInCubeForm ? Color.blue : Color.white;
            foreach (var voxel in voxels)
            {
                if (voxel != null)
                {
                    Gizmos.DrawWireSphere(voxel.position, 0.03f);
                }
            }
        }
    }

    #endregion

    #region Utility Methods

    public void RefreshVoxelCollection()
    {
        CollectVoxelsFromBase();
        CalculateAllCubePositions();
        AssignFormationPositions();
        StoreOriginalMaterials();

        Debug.Log($"[EnhancedCubeTransformation] ���� �÷��� ���ΰ�ħ �Ϸ� - {voxels.Count}��");
    }

    public float GetTransformationProgress()
    {
        if (!isTransforming) return isInCubeForm ? 1f : 0f;

        float totalProgress = 0f;
        int validVoxels = 0;

        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                Vector3 start = originalPositions[voxel];
                Vector3 target = formationPositions[voxel];
                Vector3 current = voxel.localPosition;

                float distance = Vector3.Distance(start, target);
                if (distance > 0.01f)
                {
                    float currentDistance = Vector3.Distance(current, target);
                    float progress = 1f - (currentDistance / distance);
                    totalProgress += Mathf.Clamp01(progress);
                    validVoxels++;
                }
            }
        }

        return validVoxels > 0 ? totalProgress / validVoxels : 0f;
    }

    public void EmergencyStop()
    {
        DOTween.KillAll();
        isTransforming = false;
        isDissolving = false;

        Debug.LogWarning("[EnhancedCubeTransformation] ��� ���� ����!");
    }

    /// <summary>
    /// 진행률 추적을 위한 메서드
    /// </summary>
    public float GetCurrentPhaseProgress()
    {
        if (currentTransformPhase == TransformationPhase.None) return 0f;

        float phaseElapsed = Time.time - phaseStartTime;
        float phaseDuration = transformDuration / 5f; // 5단계로 나누어진 시간

        return Mathf.Clamp01(phaseElapsed / phaseDuration);
    }

    public string GetCurrentPhaseDescription()
    {
        switch (currentTransformPhase)
        {
            case TransformationPhase.Anticipation: return "변신 준비 중...";
            case TransformationPhase.Dissolution: return "구조 해체 중...";
            case TransformationPhase.Morphing: return "형태 변환 중...";
            case TransformationPhase.Reassembly: return "재조립 중...";
            case TransformationPhase.Stabilization: return "안정화 중...";
            default: return "대기 중";
        }
    }

    /// <summary>
    /// 변신 과정을 더 부드럽게 만들기 위해 페이즈 기반 변신과 카메라 효과, 화면 진동 등을 추가합니다
    /// </summary>
    public void QuickTransform(TransformPattern pattern)
    {
        if (isTransforming) return;

        SetTransformPattern(pattern);
        StartSmoothTransformation();
    }

    /// <summary>
    /// 바이러스 효과를 테스트하기 위한 메서드
    /// </summary>
    public void TestVirusEffect()
    {
        if (isTransforming) return;
        StartCoroutine(DissolveCubes());
    }

    /// <summary>
    /// 변신 속도 설정
    /// </summary>
    public void SetTransformDuration(float duration)
    {
        transformDuration = Mathf.Max(0.5f, duration);
    }

    /// <summary>
    /// 특정 복셀의 정보 조회
    /// </summary>
    public VoxelInfo GetVoxelInfo(Transform voxel)
    {
        var info = new VoxelInfo();
        info.voxel = voxel;
        info.index = voxelIndices.ContainsKey(voxel) ? voxelIndices[voxel] : -1;
        info.originalPosition = originalPositions.ContainsKey(voxel) ? originalPositions[voxel] : Vector3.zero;
        info.formationPosition = formationPositions.ContainsKey(voxel) ? formationPositions[voxel] : Vector3.zero;
        info.isAssigned = formationPositions.ContainsKey(voxel);

        return info;
    }

    #endregion

    #region Data Structures

    [System.Serializable]
    public struct VoxelInfo
    {
        public Transform voxel;
        public int index;
        public Vector3 originalPosition;
        public Vector3 formationPosition;
        public bool isAssigned;
    }

    #endregion

    #region Cyber Dust Effects
    
    /// <summary>
    /// 특정 위치에 사이버 더스트 파티클 생성
    /// </summary>
    private void CreateCyberDustAt(Vector3 position)
    {
        if (cyberDustPrefab == null) return;
        
        var dustInstance = Instantiate(cyberDustPrefab, position, Quaternion.identity);
        dustInstance.transform.SetParent(transform);
        
        // 강도 설정
        dustInstance.AdjustIntensity(dustEmissionIntensity);
        
        // 사이버 색상 테마 설정
        Color[] virusColors = { 
            Color.red, 
            new Color(1, 0.5f, 0, 1), // 오렌지
            Color.magenta,
            new Color(1, 0, 0.5f, 1)  // 핑크
        };
        dustInstance.SetColorTheme(virusColors);
        
        // 재생 시작
        dustInstance.PlayCyberDust();
        
        // 활성 리스트에 추가
        activeDustParticles.Add(dustInstance);
        
        // 일정 시간 후 정리
        StartCoroutine(CleanupDustParticle(dustInstance, 5f));
    }
    
    /// <summary>
    /// 폭발적 더스트 효과 (의태식물 등장 시)
    /// </summary>
    private void CreateExplosiveDustAt(Vector3 position, int particleCount = 100)
    {
        if (cyberDustPrefab == null) return;
        
        var dustInstance = Instantiate(cyberDustPrefab, position, Quaternion.identity);
        dustInstance.transform.SetParent(transform);
        
        // 폭발적 등장용 색상 테마
        Color[] emergenceColors = { 
            Color.cyan, 
            Color.white, 
            new Color(0.5f, 1, 1, 1), // 라이트 시안
            new Color(0.8f, 0.9f, 1, 1) // 블루 화이트
        };
        dustInstance.SetColorTheme(emergenceColors);
        
        // 강한 강도로 설정
        dustInstance.AdjustIntensity(2f);
        
        // 폭발적 방출
        dustInstance.ExplodeAt(position, particleCount);
        
        activeDustParticles.Add(dustInstance);
        StartCoroutine(CleanupDustParticle(dustInstance, 3f));
    }
    
    /// <summary>
    /// 모든 사이버 더스트 정리
    /// </summary>
    private void CleanupAllDustParticles()
    {
        foreach (var dust in activeDustParticles)
        {
            if (dust != null)
            {
                dust.StopCyberDust();
                Destroy(dust.gameObject, 1f);
            }
        }
        activeDustParticles.Clear();
    }
    
    /// <summary>
    /// 개별 더스트 파티클 정리
    /// </summary>
    private IEnumerator CleanupDustParticle(CyberDustParticle dustParticle, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (dustParticle != null)
        {
            dustParticle.StopCyberDust();
            activeDustParticles.Remove(dustParticle);
            
            // 페이드 아웃 후 제거
            yield return new WaitForSeconds(1f);
            Destroy(dustParticle.gameObject);
        }
    }
    
    /// <summary>
    /// 변신 중 연속적인 더스트 효과
    /// </summary>
    private IEnumerator ContinuousVirusDustEffect()
    {
        while (isTransforming)
        {
            // 랜덤한 복셀 위치에서 더스트 생성
            if (voxels.Count > 0)
            {
                var randomVoxel = voxels[Random.Range(0, voxels.Count)];
                CreateCyberDustAt(randomVoxel.position);
            }
            
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
        }
    }
    
    /// <summary>
    /// 사이버 더스트 효과 테스트
    /// </summary>
    public void TestCyberDustEffect()
    {
        if (cyberDustPrefab == null)
        {
            Debug.LogWarning("[CyberDust] CyberDustPrefab이 할당되지 않았습니다!");
            return;
        }
        
        Vector3 testPosition = transform.position + cubeCenter;
        
        Debug.Log("[CyberDust] 사이버 더스트 효과 테스트 시작");
        CreateExplosiveDustAt(testPosition, 50);
        
        // 연속 효과도 테스트
        StartCoroutine(TestContinuousDust());
    }
    
    private IEnumerator TestContinuousDust()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * 3f;
            CreateCyberDustAt(randomPos);
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    #endregion
}