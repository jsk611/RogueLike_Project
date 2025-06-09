using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 292조각 바이러스 큐브 변신 시스템 - Base 하위 객체 사용
/// </summary>
public class CubeTransformationDirector : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform baseContainer; // Base 객체 참조
    [SerializeField] private bool autoFindBaseContainer = true;

    [Header("Transformation Patterns")]
    [SerializeField] private TransformPattern currentPattern = TransformPattern.Implosion;
    [SerializeField] private float transformDuration = 3f;
    [SerializeField] private Ease transformEase = Ease.OutCubic;

    [Header("Cube Formation Settings")]
    [SerializeField] private Vector3 cubeCenter = Vector3.zero;
    [SerializeField] private float cubeSize = 6f;
    [SerializeField] private Vector3Int cubeGridSize = new Vector3Int(8, 8, 8); // 8x8x8 = 512, 하지만 292개만 사용
    [SerializeField] private float voxelSpacing = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem assemblyEffect;
    [SerializeField] private ParticleSystem energyAura;
    [SerializeField] private ParticleSystem completionBurst;
    [SerializeField] private Light coreLight;
    [SerializeField] private AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 5);
    [SerializeField] private Color[] transformColors = { Color.cyan, Color.yellow, Color.red };

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] transformSounds;
    [SerializeField] private AudioClip completionSound;

    [Header("Performance")]
    [SerializeField] private int batchSize = 10; // 동시에 이동할 복셀 수
    [SerializeField] private float batchDelay = 0.05f; // 배치 간 딜레이

    // 핵심 컴포넌트들
    private VoxelFloatEffect floatEffect;
    private VirusCubeStateManager stateManager;

    // 292개 복셀 관리
    [SerializeField] private List<Transform> voxels = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> formationPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, int> voxelIndices = new Dictionary<Transform, int>();

    // 변신 상태 관리
    private bool isTransforming = false;
    private bool isInCubeForm = false;

    // 큐브 형성 좌표 저장
    private List<Vector3> availableCubePositions = new List<Vector3>();

    public enum TransformPattern
    {
        Spiral,      // 나선형으로 조립
        Wave,        // 파도처럼 순차적 조립  
        Implosion,   // 중심으로 급속 수축
        Organic,     // 유기체처럼 자연스러운 변형
        Glitch,      // 글리치 효과와 함께 변형
        Magnetic,    // 자기장처럼 끌려가는 효과
        Sequential,  // 순차적 조립
        Explosion    // 폭발 후 재조립
    }

    #region Initialization

    void Start()
    {
        InitializeComponents();
        FindOrAssignBaseContainer();
        CollectVoxelsFromBase();
        CalculateAllCubePositions();
        AssignFormationPositions();

        Debug.Log($"[CubeTransformation] 초기화 완료 - {voxels.Count}개 복셀 발견");
    }

    private void InitializeComponents()
    {
        floatEffect = GetComponent<VoxelFloatEffect>();
        stateManager = GetComponent<VirusCubeStateManager>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // DOTween 초기화
        DOTween.Init();
    }

    private void FindOrAssignBaseContainer()
    {
        if (baseContainer == null && autoFindBaseContainer)
        {
            // Base라는 이름의 하위 객체 찾기
            baseContainer = transform.Find("Base");

            if (baseContainer == null)
            {
                Debug.LogError("[CubeTransformation] Base 컨테이너를 찾을 수 없습니다!");
                return;
            }
        }

        Debug.Log($"[CubeTransformation] Base 컨테이너 설정: {baseContainer.name}");
    }

    private void CollectVoxelsFromBase()
    {
        if (baseContainer == null)
        {
            Debug.LogError("[CubeTransformation] Base 컨테이너가 설정되지 않았습니다!");
            return;
        }

        voxels.Clear();
        originalPositions.Clear();
        voxelIndices.Clear();

        // Base 하위의 모든 자식 객체를 복셀로 수집
        int index = 0;
        foreach (Transform child in baseContainer)
        {
            voxels.Add(child);
            originalPositions[child] = child.localPosition;
            voxelIndices[child] = index;
            index++;
        }

        Debug.Log($"[CubeTransformation] {voxels.Count}개 복셀을 Base에서 수집했습니다.");
    }

    private void CalculateAllCubePositions()
    {
        availableCubePositions.Clear();

        // 8x8x8 정육면체의 외곽 면만 사용하여 위치 계산
        for (int x = 0; x < cubeGridSize.x; x++)
        {
            for (int y = 0; y < cubeGridSize.y; y++)
            {
                for (int z = 0; z < cubeGridSize.z; z++)
                {
                    // 정육면체의 외곽인지 확인 (6면 중 하나 이상에 속함)
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

        Debug.Log($"[CubeTransformation] {availableCubePositions.Count}개의 큐브 위치 계산 완료");
    }

    private void AssignFormationPositions()
    {
        formationPositions.Clear();

        // 사용 가능한 위치가 복셀 수보다 적으면 경고
        if (availableCubePositions.Count < voxels.Count)
        {
            Debug.LogWarning($"[CubeTransformation] 복셀 수({voxels.Count})가 사용 가능한 위치 수({availableCubePositions.Count})보다 많습니다!");
        }

        // 각 복셀에 대해 가장 가까운 큐브 위치 할당
        var assignedPositions = new HashSet<Vector3>();

        for (int i = 0; i < voxels.Count && i < availableCubePositions.Count; i++)
        {
            Transform voxel = voxels[i];
            Vector3 bestPosition = Vector3.zero;
            float minDistance = float.MaxValue;

            // 아직 할당되지 않은 위치 중에서 가장 가까운 곳 찾기
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

        Debug.Log($"[CubeTransformation] {formationPositions.Count}개 복셀에 대해 형성 위치 할당 완료");
    }

    #endregion

    #region Public API

    /// <summary>
    /// 큐브 형태로 변신 시작
    /// </summary>
    public void StartCubeTransformation()
    {
        if (isTransforming)
        {
            Debug.LogWarning("[CubeTransformation] 이미 변신 중입니다!");
            return;
        }

        Debug.Log($"[CubeTransformation] 큐브 변신 시작 - 패턴: {currentPattern}");
        StartCoroutine(ExecuteTransformation());
    }

    /// <summary>
    /// 원래 형태로 되돌리기
    /// </summary>
    public void RevertToOriginal()
    {
        if (isTransforming)
        {
            Debug.LogWarning("[CubeTransformation] 변신 중에는 되돌릴 수 없습니다!");
            return;
        }

        Debug.Log("[CubeTransformation] 원형 복귀 시작");
        StartCoroutine(ExecuteReversion());
    }

    /// <summary>
    /// 변신 패턴 설정
    /// </summary>
    public void SetTransformPattern(TransformPattern pattern)
    {
        currentPattern = pattern;
        Debug.Log($"[CubeTransformation] 변신 패턴 변경: {pattern}");
    }

    /// <summary>
    /// 변신 지속 시간 설정
    /// </summary>
    public void SetTransformDuration(float duration)
    {
        transformDuration = Mathf.Max(0.5f, duration);
    }

    /// <summary>
    /// 현재 변신 상태 확인
    /// </summary>
    public bool IsInCubeForm => isInCubeForm;
    public bool IsTransforming => isTransforming;

    #endregion

    #region Transformation Execution

    private IEnumerator ExecuteTransformation()
    {
        isTransforming = true;

        // 1. 준비 단계
        PrepareTransformation();
        yield return new WaitForSeconds(0.5f);

        // 2. 패턴별 변환 실행
        yield return StartCoroutine(ExecutePatternTransformation());

        // 3. 완성 단계
        yield return StartCoroutine(FinalizeFormation());

        // 4. 완성 효과
        PlayCompletionEffects();

        isInCubeForm = true;
        isTransforming = false;

        Debug.Log("[CubeTransformation] 큐브 변신 완료!");
    }

    private void PrepareTransformation()
    {
        // 부유 효과를 충전 모드로 전환
        if (floatEffect != null)
        {
            floatEffect.SetChargingMode(true);
        }

        // 코어 라이트 활성화
        if (coreLight != null)
        {
            coreLight.enabled = true;
            coreLight.color = transformColors[0]; // 시작 색상
            coreLight.transform.position = transform.position + cubeCenter;

            DOTween.To(() => coreLight.intensity, x => coreLight.intensity = x, 3f, 0.5f);
        }

        // 조립 파티클 시작
        if (assemblyEffect != null)
        {
            assemblyEffect.transform.position = transform.position + cubeCenter;
            assemblyEffect.Play();
        }

        PlayRandomTransformSound();
    }

    private IEnumerator ExecutePatternTransformation()
    {
        switch (currentPattern)
        {
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
        }
    }

    #endregion

    #region Transformation Patterns

    private IEnumerator SpiralTransformation()
    {
        var sortedVoxels = SortVoxelsByDistanceFromCenter();
        int batchCount = 0;

        for (int i = 0; i < sortedVoxels.Count; i++)
        {
            var voxel = sortedVoxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;

            float delay = (i / (float)batchSize) * batchDelay;
            Vector3 spiralWaypoint = CalculateSpiralPath(i, sortedVoxels.Count);
            Vector3 targetPos = formationPositions[voxel];

            StartCoroutine(MoveVoxelAlongPath(voxel, spiralWaypoint, targetPos, delay));

            batchCount++;
            if (batchCount >= batchSize)
            {
                yield return new WaitForSeconds(batchDelay);
                batchCount = 0;
            }
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

                // 파도 효과로 이동
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
        List<Tween> allTweens = new List<Tween>();

        foreach (var voxel in voxels)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            // 급속한 수축 효과
            var moveTween = voxel.DOLocalMove(targetPos, transformDuration * 0.7f)
                                 .SetEase(Ease.InExpo);

            var scaleTween = voxel.DOScale(Vector3.one * 0.2f, transformDuration * 0.2f)
                                  .SetEase(Ease.InQuad)
                                  .OnComplete(() => {
                                      voxel.DOScale(Vector3.one, transformDuration * 0.5f)
                                           .SetEase(Ease.OutBounce);
                                  });

            allTweens.Add(moveTween);
            allTweens.Add(scaleTween);
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

        // 복셀을 무작위 순서로 글리치 텔레포트
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
        // 인덱스 순서대로 차례차례 이동
        for (int i = 0; i < voxels.Count; i++)
        {
            var voxel = voxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            voxel.DOLocalMove(targetPos, 0.5f).SetEase(Ease.OutBack);
            voxel.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutQuad)
                 .OnComplete(() => voxel.DOScale(Vector3.one, 0.2f));

            // 일정 간격으로 배치 처리
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

    #endregion

    #region Helper Methods

    private Vector3 CalculateSpiralPath(int index, int totalCount)
    {
        float progress = index / (float)totalCount;
        float angle = progress * Mathf.PI * 6f; // 3바퀴 나선
        float radius = 4f * (1f - progress);
        float height = Mathf.Sin(progress * Mathf.PI) * 3f;

        return cubeCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            height,
            Mathf.Sin(angle) * radius
        );
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

    private List<List<Transform>> GroupVoxelsByLayer()
    {
        var layers = new List<List<Transform>>();
        var sorted = new List<Transform>(voxels);

        // Y 좌표 기준으로 정렬
        sorted.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        // 층별로 그룹화 (더 세밀한 레이어링)
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

        // 웨이포인트를 거쳐 목표점으로 이동
        var sequence = DOTween.Sequence();
        sequence.Append(voxel.DOLocalMove(waypoint, transformDuration * 0.4f).SetEase(Ease.OutQuad));
        sequence.Append(voxel.DOLocalMove(target, transformDuration * 0.6f).SetEase(Ease.InOutCubic));

        // 회전도 부드럽게
        sequence.Join(voxel.DOLocalRotate(Vector3.zero, transformDuration));
    }

    private IEnumerator GlitchTeleport(Transform voxel)
    {
        if (!formationPositions.ContainsKey(voxel)) yield break;

        Vector3 targetPos = formationPositions[voxel];
        Vector3 glitchOffset = Random.insideUnitSphere * 1f;

        // 글리치 효과: 잠깐 사라졌다가 목표 위치 근처에 나타남
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

        // 자기장 효과: 곡선 경로로 끌려감
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

            // 근처의 복셀들을 클러스터에 추가 (더 작은 클러스터 크기)
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

            // 유기적 성장 효과
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
        // 모든 조각을 정확한 위치로 미세 조정
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
        // 완성 파티클 효과
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

        // 라이트 효과
        if (coreLight != null)
        {
            coreLight.color = transformColors[2]; // 완성 색상 (빨간색)
            coreLight.DOIntensity(8f, 0.2f).OnComplete(() => {
                coreLight.DOIntensity(3f, 0.8f);
            });
        }

        // 완성 사운드
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }

        // 부유 효과를 공격 모드로 전환
        if (floatEffect != null)
        {
            floatEffect.SetAttackMode(true);
        }

        Debug.Log("[CubeTransformation] 큐브 형성 완료!");
    }

    #endregion

    #region Reversion

    private IEnumerator ExecuteReversion()
    {
        isTransforming = true;

        Debug.Log("[CubeTransformation] 원형 복귀 시작");

        // 복귀 준비
        PrepareReversion();
        yield return new WaitForSeconds(0.3f);

        // 원형 복귀 실행
        yield return StartCoroutine(RevertToOriginalPositions());

        // 복귀 완료 처리
        CompleteReversion();

        isInCubeForm = false;
        isTransforming = false;

        Debug.Log("[CubeTransformation] 원형 복귀 완료!");
    }

    private void PrepareReversion()
    {
        // 라이트 효과 변경
        if (coreLight != null)
        {
            coreLight.color = transformColors[0]; // 원래 색상으로
            coreLight.DOIntensity(1f, 0.5f);
        }

        // 복귀 사운드
        PlayRandomTransformSound();
    }

    private IEnumerator RevertToOriginalPositions()
    {
        if (stateManager != null)
        {
            // StateManager를 사용한 부드러운 복귀
            yield return StartCoroutine(stateManager.RestoreOriginalStateSmooth(transformDuration));
        }
        else
        {
            // 직접 복귀 처리
            foreach (var voxel in voxels)
            {
                if (originalPositions.ContainsKey(voxel))
                {
                    voxel.DOLocalMove(originalPositions[voxel], transformDuration)
                         .SetEase(Ease.OutCubic);

                    voxel.DOLocalRotate(Vector3.zero, transformDuration * 0.8f);

                    // 약간의 지연을 주어 자연스러운 해체 효과
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
                }
            }

            yield return new WaitForSeconds(transformDuration);
        }
    }

    private void CompleteReversion()
    {
        // 라이트 끄기
        if (coreLight != null)
        {
            coreLight.DOIntensity(0f, 0.5f).OnComplete(() => {
                coreLight.enabled = false;
            });
        }

        // 파티클 정지
        if (assemblyEffect != null && assemblyEffect.isPlaying)
        {
            assemblyEffect.Stop();
        }

        if (energyAura != null && energyAura.isPlaying)
        {
            energyAura.Stop();
        }

        // 부유 효과 복원
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

        // 동적으로 라이트 강도 조절 (큐브 형태일 때)
        if (isInCubeForm && coreLight != null && lightIntensityCurve != null)
        {
            float normalizedTime = (Time.time % 4f) / 4f; // 4초 주기
            float intensity = lightIntensityCurve.Evaluate(normalizedTime) * 3f + 2f;
            coreLight.intensity = intensity;
        }
    }

    private void HandleTestInput()
    {
        // T키로 변신 테스트
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!isTransforming)
            {
                if (isInCubeForm)
                {
                    Debug.Log("=== T키 눌림: 원형 복귀 시작 ===");
                    RevertToOriginal();
                }
                else
                {
                    Debug.Log("=== T키 눌림: 큐브 변신 시작 ===");
                    StartCubeTransformation();
                }
            }
        }

        // R키로 강제 복귀
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isTransforming && isInCubeForm)
            {
                Debug.Log("=== R키 눌림: 강제 원형 복귀 ===");
                RevertToOriginal();
            }
        }

        // 숫자 키로 패턴 변경
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetTransformPattern(TransformPattern.Spiral); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetTransformPattern(TransformPattern.Wave); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetTransformPattern(TransformPattern.Implosion); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SetTransformPattern(TransformPattern.Organic); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SetTransformPattern(TransformPattern.Glitch); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SetTransformPattern(TransformPattern.Magnetic); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { SetTransformPattern(TransformPattern.Sequential); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { SetTransformPattern(TransformPattern.Explosion); }

        // +/- 키로 변신 속도 조절
        if (Input.GetKeyDown(KeyCode.Equals)) // + 키
        {
            SetTransformDuration(transformDuration - 0.5f);
            Debug.Log($"변신 속도 증가: {transformDuration}초");
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetTransformDuration(transformDuration + 0.5f);
            Debug.Log($"변신 속도 감소: {transformDuration}초");
        }

        // C키로 복셀 수 확인
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"=== 복셀 정보 ===");
            Debug.Log($"전체 복셀 수: {voxels.Count}");
            Debug.Log($"형성 위치 할당 수: {formationPositions.Count}");
            Debug.Log($"사용 가능한 큐브 위치 수: {availableCubePositions.Count}");
            Debug.Log($"현재 상태: {(isInCubeForm ? "큐브" : "원형")}");
            Debug.Log($"변신 중: {isTransforming}");
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // 큐브 형태 미리보기
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + cubeCenter, Vector3.one * cubeSize);

        // 큐브 중심점
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + cubeCenter, 0.2f);

        // Base 컨테이너 표시
        if (baseContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(baseContainer.position, Vector3.one * 0.5f);
        }

        // 사용 가능한 큐브 위치들 표시
        if (availableCubePositions.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in availableCubePositions)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(pos), 0.05f);
            }
        }

        // 현재 할당된 형성 위치들 표시
        if (formationPositions.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var pos in formationPositions.Values)
            {
                Gizmos.DrawSphere(transform.TransformPoint(pos), 0.08f);
            }
        }

        // 복셀들의 현재 위치 표시
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

    private void OnDrawGizmos()
    {
        // 항상 표시되는 기본 정보
        if (baseContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, baseContainer.position);
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// 복셀 수집을 다시 수행 (런타임에 Base 내용이 변경된 경우)
    /// </summary>
    public void RefreshVoxelCollection()
    {
        CollectVoxelsFromBase();
        CalculateAllCubePositions();
        AssignFormationPositions();

        Debug.Log($"[CubeTransformation] 복셀 컬렉션 새로고침 완료 - {voxels.Count}개");
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

    /// <summary>
    /// 변신 진행률 확인 (0~1)
    /// </summary>
    public float GetTransformationProgress()
    {
        if (!isTransforming) return isInCubeForm ? 1f : 0f;

        // 각 복셀의 목표 위치 대비 현재 위치로 진행률 계산
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

    /// <summary>
    /// 긴급 정지 (모든 트윈 중단)
    /// </summary>
    public void EmergencyStop()
    {
        DOTween.KillAll();
        isTransforming = false;

        Debug.LogWarning("[CubeTransformation] 긴급 정지 실행!");
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
}