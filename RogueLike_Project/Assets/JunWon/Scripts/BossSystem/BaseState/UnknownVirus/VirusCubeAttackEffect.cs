using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 수정된 바이러스 큐브 레이저 이펙트 - 중심점 기준 정육면체 형성
/// </summary>
public class VirusCubeAttackEffect : MonoBehaviour
{

    [Header("Enhanced Spread")]
    [SerializeField] private bool useEnhancedSpread = true;

    [SerializeField] private VirusCubeSpread spreadCalculator;

    [Header("Formation Settings")]
    [SerializeField] private float formationTime = 2f;        // 큐브 형성 시간
    [SerializeField] private float compactTime = 1f;          // 압축 시간
    [SerializeField] private float expandTime = 0.3f;         // 펼침 시간
    [SerializeField] private float returnTime = 1.5f;         // 원래 위치로 복귀 시간
    [SerializeField] private bool shouldReturnToOriginal = true; // 원래 위치로 복귀할지 여부

    [Header("Cube Formation")]
    [SerializeField] private Vector3Int cubeSize = new Vector3Int(8, 8, 8);
    [SerializeField] private float voxelSpacing = 0.15f;      // 복셀 간격
    [SerializeField] private float compactScale = 0.6f;       // 압축시 스케일
    [SerializeField] private float expandScale = 1.8f;        // 펼침시 스케일
    [SerializeField] private Vector3 cubeCenter = Vector3.zero; // 큐브 중심점

    [Header("Visual Effects")]
    [SerializeField] private Color formationColor = Color.cyan;
    [SerializeField] private Color chargingColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private ParticleSystem expandEffect;
    [SerializeField] private Light coreLight;                 // 중심 라이트

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip formationSound;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip expandSound;

    // 복셀 관리
    private List<Transform> voxelChildren = new List<Transform>();
    private List<Vector3> originalPositions = new List<Vector3>();    // 원래 위치 (랜덤 플로팅 위치)
    private List<Vector3> cubePositions = new List<Vector3>();        // 큐브 형성 위치
    private List<Vector3> compactPositions = new List<Vector3>();     // 압축 위치
    private List<Vector3> expandedPositions = new List<Vector3>();    // 펼침 위치
    private VoxelFloatEffect floatEffect;

    // 레이저 관리
    private Transform target;
    private bool isExecuting = false;

    private void Start()
    {
        useEnhancedSpread = true;
        // 기존 자식 객체들을 복셀로 사용
        CollectExistingVoxels();

        // 컴포넌트 설정
        floatEffect = GetComponent<VoxelFloatEffect>();
        if (floatEffect == null)
            floatEffect = gameObject.AddComponent<VoxelFloatEffect>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 플레이어 타겟 찾기
        target = GameObject.FindWithTag("Player")?.transform;

        // 큐브 중심점을 현재 위치로 설정
        cubeCenter = transform.position;

        if (useEnhancedSpread)
        {
            spreadCalculator = GetComponent<VirusCubeSpread>();
        }
    }

    /// <summary>
    /// 레이저 공격 시작
    /// </summary>
    public void StartLaserAttack()
    {
        if (isExecuting) return;
        StartCoroutine(ExecuteLaserAttack());
    }

    private IEnumerator ExecuteLaserAttack()
    {
        isExecuting = true;

        // 1. 큐브 형성 (현재 위치에서 중심점 기준 정육면체로)
        yield return StartCoroutine(FormCubePhase());

        // 2. 압축 및 차징
        yield return StartCoroutine(CompactAndChargePhase());

        // 3. 펼침 및 레이저 발사
        yield return StartCoroutine(ExpandAndLaserPhase());

        // 4. 원래 위치로 복귀 (랜덤 플로팅 상태)
        if (shouldReturnToOriginal)
        {
            yield return StartCoroutine(ReturnToFloatingPhase());
        }
        isExecuting = false;
    }

    /// <summary>
    /// 기존 자식 객체들을 복셀로 수집
    /// </summary>
    private void CollectExistingVoxels()
    {
        voxelChildren.Clear();
        originalPositions.Clear();

        // 모든 자식 객체 수집
        foreach (Transform child in transform)
        {
            voxelChildren.Add(child);
            // 현재 위치를 원래 플로팅 위치로 저장
            originalPositions.Add(child.localPosition);
        }

        // 정육면체 위치들 계산
        CalculateFormationPositions();

        Debug.Log($"수집된 복셀 개수: {voxelChildren.Count}");
    }

    /// <summary>
    /// 중심점 기준 정육면체 위치들 계산
    /// </summary>
    private void CalculateFormationPositions()
    {
        cubePositions.Clear();
        compactPositions.Clear();
        expandedPositions.Clear();

        int positionIndex = 0;
        int totalPositions = 0;

        // 먼저 총 위치 개수 계산
        for (int x = 0; x < cubeSize.x; x++)
        {
            for (int y = 0; y < cubeSize.y; y++)
            {
                for (int z = 0; z < cubeSize.z; z++)
                {
                    bool isEdge = (x == 0 || x == cubeSize.x - 1) ||
                                  (y == 0 || y == cubeSize.y - 1) ||
                                  (z == 0 || z == cubeSize.z - 1);
                    if (isEdge) totalPositions++;
                }
            }
        }

        // 실제 위치 계산
        for (int x = 0; x < cubeSize.x; x++)
        {
            for (int y = 0; y < cubeSize.y; y++)
            {
                for (int z = 0; z < cubeSize.z; z++)
                {
                    bool isEdge = (x == 0 || x == cubeSize.x - 1) ||
                                  (y == 0 || y == cubeSize.y - 1) ||
                                  (z == 0 || z == cubeSize.z - 1);

                    if (isEdge)
                    {
                        // 중심점 기준으로 정육면체 위치 계산
                        Vector3 cubePos = new Vector3(
                            (x - cubeSize.x / 2f + 0.5f) * voxelSpacing,
                            (y - cubeSize.y / 2f + 0.5f) * voxelSpacing,
                            (z - cubeSize.z / 2f + 0.5f) * voxelSpacing
                        );

                        cubePositions.Add(cubePos);

                        // 압축 위치
                        Vector3 compactPos = cubePos * compactScale;
                        compactPositions.Add(compactPos);

                        // 향상된 퍼짐 위치 계산
                        Vector3 expandPos;
                        if (useEnhancedSpread && spreadCalculator != null)
                        {
                            Debug.Log("퍼짐 상태 계산");
                            expandPos = spreadCalculator.CalculateSpreadPosition(cubePos, positionIndex, totalPositions);
                        }
                        else
                        {
                            // 기본 퍼짐 (균등하게)
                            Vector3 direction = cubePos.normalized;
                            expandPos = direction * expandScale;
                        }

                        expandedPositions.Add(expandPos);
                        positionIndex++;
                    }
                }
            }
        }

        Debug.Log($"계산된 큐브 위치 개수: {cubePositions.Count}");
    }

    /// <summary>
    /// 1단계: 정육면체 형성
    /// </summary>
    private IEnumerator FormCubePhase()
    {
        PlaySound(formationSound);
        Debug.Log("정육면체 형성 시작");

        float elapsed = 0f;
        while (elapsed < formationTime)
        {
            float progress = elapsed / formationTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // 각 복셀을 정육면체 위치로 이동
            for (int i = 0; i < voxelChildren.Count; i++)
            {
                Vector3 startPos = originalPositions[i];              // 원래 랜덤 위치
                Vector3 targetPos = i < cubePositions.Count ? cubePositions[i] : Vector3.zero; // 정육면체 위치

                voxelChildren[i].localPosition = Vector3.Lerp(startPos, targetPos, easedProgress);

                // 형성되면서 회전
                voxelChildren[i].Rotate(Vector3.up, Time.deltaTime * 180f * (1f - easedProgress));

                // 색상 변화
                Color lerpColor = Color.Lerp(formationColor, chargingColor, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("정육면체 형성 완료");
    }

    /// <summary>
    /// 2단계: 압축 및 차징
    /// </summary>
    private IEnumerator CompactAndChargePhase()
    {
        PlaySound(chargeSound);
        Debug.Log("압축 및 차징 시작");

        // 차징 파티클 시작
        if (chargeEffect != null)
        {
            chargeEffect.gameObject.SetActive(true);
            chargeEffect.Play();
        }

        // 코어 라이트 활성화
        if (coreLight != null)
        {
            coreLight.enabled = true;
            coreLight.color = chargingColor;
            coreLight.transform.localPosition = Vector3.zero; // 중심에 배치
        }

        float elapsed = 0f;
        while (elapsed < compactTime)
        {
            float progress = elapsed / compactTime;
            float intensity = Mathf.PingPong(elapsed * 4f, 1f);

            // 정육면체를 중심으로 압축
            for (int i = 0; i < voxelChildren.Count; i++)
            {
                Vector3 cubePos = i < cubePositions.Count ? cubePositions[i] : Vector3.zero;
                Vector3 compactPos = i < compactPositions.Count ? compactPositions[i] : Vector3.zero;

                voxelChildren[i].localPosition = Vector3.Lerp(cubePos, compactPos, progress);

                // 차징 색상 펄스
                Color pulseColor = Color.Lerp(chargingColor, attackColor, intensity);
            }

            // 라이트 강도 조절
            if (coreLight != null)
            {
                coreLight.intensity = 2f + intensity * 3f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("압축 및 차징 완료");
    }

    /// <summary>
    /// 3단계: 펼침 및 레이저 발사
    /// </summary>
    private IEnumerator ExpandAndLaserPhase()
    {
        PlaySound(expandSound);
        Debug.Log("펼침 및 레이저 발사 시작");

        // 차징 효과 정지
        if (chargeEffect != null)
            chargeEffect.Stop();

        // 펼침 효과 시작
        if (expandEffect != null)
        {
            expandEffect.gameObject.SetActive(true);
            expandEffect.Play();
        }

        // 빠른 펼침 애니메이션
        float elapsed = 0f;
        while (elapsed < expandTime)
        {
            float progress = elapsed / expandTime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f); // 가속 곡선

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                Vector3 compactPos = i < compactPositions.Count ? compactPositions[i] : Vector3.zero;
                Vector3 expandPos = i < expandedPositions.Count ? expandedPositions[i] : Vector3.zero;

                voxelChildren[i].localPosition = Vector3.Lerp(compactPos, expandPos, easedProgress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 4단계: 원래 랜덤 플로팅 상태로 복귀
    /// </summary>
    private IEnumerator ReturnToFloatingPhase()
    {
        Debug.Log("원래 플로팅 상태로 복귀 시작");

        float elapsed = 0f;
        while (elapsed < returnTime)
        {
            float progress = elapsed / returnTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                // 현재 펼침 위치에서 원래 랜덤 플로팅 위치로 복귀
                Vector3 expandPos = i < expandedPositions.Count ? expandedPositions[i] : voxelChildren[i].localPosition;
                Vector3 originalPos = i < originalPositions.Count ? originalPositions[i] : Vector3.zero;

                voxelChildren[i].localPosition = Vector3.Lerp(expandPos, originalPos, easedProgress);

                // 스케일 복구
                float scale = Mathf.Lerp(voxelChildren[i].localScale.x, 1f, easedProgress);
                voxelChildren[i].localScale = Vector3.one * scale;

                // 복귀하면서 부드러운 회전
                voxelChildren[i].Rotate(Vector3.up, Time.deltaTime * 90f * (1f - progress));
            }

            // 라이트 점진적 꺼짐
            if (coreLight != null)
            {
                coreLight.intensity = Mathf.Lerp(5f, 0f, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 완전히 원래 상태로 복구
        RestoreOriginalFloatingState();

        Debug.Log("원래 플로팅 상태 복귀 완료");
    }

    /// <summary>
    /// 원래 플로팅 상태로 완전 복구
    /// </summary>
    private void RestoreOriginalFloatingState()
    {
        for (int i = 0; i < voxelChildren.Count && i < originalPositions.Count; i++)
        {
            // 위치 완전 복구
            voxelChildren[i].localPosition = originalPositions[i];

            // 스케일 복구
            voxelChildren[i].localScale = Vector3.one;

        }

        // 이펙트 정리
        if (coreLight != null)
            coreLight.enabled = false;

        if (chargeEffect != null)
            chargeEffect.gameObject.SetActive(false);

        if (expandEffect != null)
            expandEffect.gameObject.SetActive(false);

        // 플로팅 효과 재활성화
        if (floatEffect != null)
        {
            floatEffect.enabled = true;
            floatEffect.SetFloatIntensity(1f);
        }
    }


    /// <summary>
    /// 5단계: 소멸 (복귀하지 않을 때)
    /// </summary>
    private IEnumerator DissolvePhase()
    {
        Debug.Log("바이러스 큐브 소멸 시작");

        float elapsed = 0f;
        while (elapsed < returnTime)
        {
            float progress = elapsed / returnTime;

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                // 스케일 감소
                float scale = Mathf.Lerp(1f, 0f, progress);
                voxelChildren[i].localScale = Vector3.one * scale;

                // 랜덤 회전
                voxelChildren[i].Rotate(Random.insideUnitSphere, Time.deltaTime * 360f);

                // 랜덤하게 흩어짐
                Vector3 randomOffset = Random.insideUnitSphere * progress * 3f;
                Vector3 expandPos = i < expandedPositions.Count ? expandedPositions[i] : voxelChildren[i].localPosition;
                voxelChildren[i].localPosition = expandPos + randomOffset;
            }

            // 라이트 페이드아웃
            if (coreLight != null)
            {
                coreLight.intensity = Mathf.Lerp(5f, 0f, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 정리
        if (coreLight != null)
            coreLight.enabled = false;

        // 객체 비활성화
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopEffect()
    {
        StopAllCoroutines();
        isExecuting = false;

        if (shouldReturnToOriginal)
        {
            RestoreOriginalFloatingState();
        }
    }

    public void SetReturnMode(bool shouldReturn)
    {
        shouldReturnToOriginal = shouldReturn;
    }

    /// <summary>
    /// 에디터 기즈모 - 정육면체 형성 위치 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 큐브 중심점 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // 정육면체 영역 표시
        Gizmos.color = Color.cyan;
        Vector3 cubeExtents = Vector3.one * cubeSize.x * voxelSpacing;
        Gizmos.DrawWireCube(transform.position, cubeExtents);

        // 압축 영역 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, cubeExtents * compactScale);

        // 펼침 영역 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, cubeExtents * expandScale);
    }
}