using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 변신 복귀 시 특별한 floating 효과 - 변신 복귀 시 특별한 floating 효과 시작
/// </summary>
public class VoxelFloatEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatAmplitude = 0.3f;    // 진폭
    public float floatSpeed = 1f;          // 주기
    public float randomOffset = 0.2f;      // 랜덤 오프셋

    [Header("Advanced Effects")]
    public bool enableOrbitalMotion = true;        // 궤도 회전
    public float orbitalRadius = 0.1f;             // 궤도 반경
    public float orbitalSpeed = 2f;                // 궤도 주기
    public bool enablePulseEffect = true;          // 펄스 효과
    public float pulseIntensity = 0.05f;           // 펄스 강도
    public bool enableGlitchFloat = false;         // 노이즈 효과
    public float glitchChance = 0.1f;              // 노이즈 확률
    public float glitchDuration = 0.2f;            // 노이즈 지속 시간

    [Header("Virus Effects")]
    public bool enableVirusCorruption = true;     // 바이러스 오염 효과
    public float corruptionIntensity = 0.15f;     // 오염 강도
    public float corruptionSpeed = 3f;            // 오염 주기

    // 원래 위치 저장
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> voxelOffsets = new Dictionary<Transform, float>();
    private Dictionary<Transform, Vector3> orbitalOffsets = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> glitchTimers = new Dictionary<Transform, float>();
    private Dictionary<Transform, Vector3> glitchTargets = new Dictionary<Transform, Vector3>();

    // 효과 강도 곱수
    private float globalIntensityMultiplier = 1f;
    private bool isPaused = false;

    void Start()
    {
        // 자식 객체들의 원래 위치 저장
        InitializeVoxelPositions();
    }

    void Update()
    {
        if (!isPaused)
        {
            FloatVoxels();
        }
    }

    private void InitializeVoxelPositions()
    {
        // 모든 자식 객체들의 원래 위치와 오프셋 저장
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeInHierarchy) continue;
            
            originalPositions[child] = child.localPosition;
            voxelOffsets[child] = Random.Range(0f, 2f * Mathf.PI); // 랜덤 오프셋

            // 궤도 회전을 위한 기본 위치 설정
            orbitalOffsets[child] = Random.insideUnitSphere * 0.1f;

            // 노이즈 효과를 위한 초기화
            glitchTimers[child] = 0f;
            glitchTargets[child] = Vector3.zero;
        }
    }

    private void FloatVoxels()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel == null) continue;

            // 랜덤 오프셋을 이용한 오프셋 계산
            float timeOffset = voxelOffsets[voxel];
            Vector3 finalOffset = Vector3.zero;

            // 1. 기본 효과
            finalOffset += CalculateBasicFloat(timeOffset);

            // 2. 궤도 회전 효과
            if (enableOrbitalMotion)
            {
                finalOffset += CalculateOrbitalMotion(voxel, timeOffset);
            }

            // 3. 펄스 효과
            if (enablePulseEffect)
            {
                finalOffset += CalculatePulseEffect(timeOffset);
            }

            // 4. 노이즈 효과
            if (enableGlitchFloat)
            {
                finalOffset += CalculateGlitchEffect(voxel);
            }

            // 5. 바이러스 오염 효과
            if (enableVirusCorruption)
            {
                finalOffset += CalculateVirusCorruption(voxel, timeOffset);
            }

            // 효과 강도 곱산
            finalOffset *= globalIntensityMultiplier;

            // 최종 위치 계산
            Vector3 targetPosition = originalPositions[voxel] + finalOffset;
            voxel.localPosition = Vector3.Lerp(voxel.localPosition, targetPosition, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 기본 효과 계산
    /// </summary>
    private Vector3 CalculateBasicFloat(float timeOffset)
    {
        // Y 방향 진폭
        float floatY = Mathf.Sin((Time.time * floatSpeed) + timeOffset) * floatAmplitude;

        // X, Z 방향 진폭 (Y 방향 주기 참고)
        float floatX = Mathf.Cos((Time.time * floatSpeed * 0.7f) + timeOffset) * (floatAmplitude * 0.3f);
        float floatZ = Mathf.Sin((Time.time * floatSpeed * 0.5f) + timeOffset) * (floatAmplitude * 0.3f);

        return new Vector3(floatX, floatY, floatZ) * randomOffset;
    }

    /// <summary>
    /// 궤도 회전 효과
    /// </summary>
    private Vector3 CalculateOrbitalMotion(Transform voxel, float timeOffset)
    {
        Vector3 orbitalBase = orbitalOffsets[voxel];
        float orbitalTime = Time.time * orbitalSpeed + timeOffset;

        Vector3 orbital = new Vector3(
            Mathf.Cos(orbitalTime) * orbitalRadius,
            Mathf.Sin(orbitalTime * 1.3f) * orbitalRadius * 0.5f,
            Mathf.Sin(orbitalTime) * orbitalRadius
        );

        return orbital + orbitalBase;
    }

    /// <summary>
    /// 펄스 효과 계산
    /// </summary>
    private Vector3 CalculatePulseEffect(float timeOffset)
    {
        float pulse = Mathf.Sin(Time.time * 4f + timeOffset) * pulseIntensity;
        return Vector3.one * pulse;
    }

    /// <summary>
    /// 노이즈 효과 계산
    /// </summary>
    private Vector3 CalculateGlitchEffect(Transform voxel)
    {
        // 노이즈 효과를 위한 타이머 감소
        glitchTimers[voxel] -= Time.deltaTime;

        // 랜덤 노이즈 확률 체크
        if (glitchTimers[voxel] <= 0f && Random.value < glitchChance * Time.deltaTime)
        {
            glitchTimers[voxel] = glitchDuration;
            glitchTargets[voxel] = Random.insideUnitSphere * 0.3f;
        }

        // 노이즈 효과 계산
        if (glitchTimers[voxel] > 0f)
        {
            float intensity = glitchTimers[voxel] / glitchDuration;
            return glitchTargets[voxel] * intensity;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 바이러스 오염 효과 계산
    /// </summary>
    private Vector3 CalculateVirusCorruption(Transform voxel, float timeOffset)
    {
        float corruptionTime = Time.time * corruptionSpeed + timeOffset;

        // 오염 효과를 위한 랜덤 노이즈 계산
        Vector3 corruption = new Vector3(
            Mathf.PerlinNoise(corruptionTime, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, corruptionTime) - 0.5f,
            Mathf.PerlinNoise(corruptionTime, corruptionTime) - 0.5f
        );

        return corruption * corruptionIntensity;
    }

    // 진폭 강도 조절 (모든 효과에 영향)
    public void SetFloatIntensity(float intensity)
    {
        globalIntensityMultiplier = intensity;
        floatAmplitude = 0.3f * intensity;
        floatSpeed = 1f * intensity;
        randomOffset = 0.2f * intensity;
    }

    /// <summary>
    /// 노이즈 효과 활성화/비활성화
    /// </summary>
    public void SetGlitchMode(bool enabled, float intensity = 1f)
    {
        enableGlitchFloat = enabled;
        if (enabled)
        {
            glitchChance = 0.1f * intensity;
            glitchDuration = 0.2f / intensity;
        }
    }

    /// <summary>
    /// 바이러스 오염 효과 활성화/비활성화
    /// </summary>
    public void SetVirusCorruption(bool enabled, float intensity = 1f)
    {
        enableVirusCorruption = enabled;
        corruptionIntensity = 0.15f * intensity;
        corruptionSpeed = 3f * intensity;
    }

    /// <summary>
    /// 궤도 회전 활성화/비활성화
    /// </summary>
    public void SetOrbitalMotion(bool enabled, float radius = 0.1f, float speed = 2f)
    {
        enableOrbitalMotion = enabled;
        orbitalRadius = radius;
        orbitalSpeed = speed;
    }

    /// <summary>
    /// 효과 일시정지/재생
    /// </summary>
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    /// <summary>
    /// 충전 모드 설정 (충전 중)
    /// </summary>
    public void SetChargingMode(bool charging)
    {
        if (charging)
        {
            SetFloatIntensity(2f);
            SetGlitchMode(true, 2f);
            enablePulseEffect = true;
            pulseIntensity = 0.1f;
        }
        else
        {
            SetFloatIntensity(1f);
            SetGlitchMode(false);
            enablePulseEffect = true;
            pulseIntensity = 0.05f;
        }
    }

    /// <summary>
    /// 공격 모드 설정
    /// </summary>
    public void SetAttackMode(bool attacking)
    {
        if (attacking)
        {
            SetFloatIntensity(0.2f); // 강렬한 시작을 위한 진폭 감소
            SetGlitchMode(false);
            enableOrbitalMotion = false;
        }
        else
        {
            SetFloatIntensity(1f);
            enableOrbitalMotion = true;
        }
    }

    /// <summary>
    /// 해체 모드 설정
    /// </summary>
    public void SetDissolveMode(bool dissolving)
    {
        if (dissolving)
        {
            SetFloatIntensity(3f);
            SetGlitchMode(true, 3f);
            SetVirusCorruption(true, 2f);
        }
    }

    /// <summary>
    /// 원래 위치로 리셋
    /// </summary>
    public void ResetToOriginalPositions()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel != null)
            {
                voxel.localPosition = originalPositions[voxel];
            }
        }
    }

    /// <summary>
    /// 모든 자식 객체들의 원래 위치 초기화
    /// </summary>
    public void RefreshVoxelList()
    {
        // 모든 저장 정보 초기화
        originalPositions.Clear();
        voxelOffsets.Clear();
        orbitalOffsets.Clear();
        glitchTimers.Clear();
        glitchTargets.Clear();

        // 초기화
        InitializeVoxelPositions();
    }

    /// <summary>
    /// 디버그용 위치 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (originalPositions.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in originalPositions.Values)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(pos), 0.05f);
            }
        }
    }

    /// <summary>
    /// 변신 복귀 시 특별한 floating 효과 시작
    /// </summary>
    public void StartFloatingEffect()
    {
        Debug.Log("[VoxelFloatEffect] 변신 복귀 floating 효과 시작");
        
        // 모든 효과 활성화
        enableOrbitalMotion = true;
        enablePulseEffect = true;
        enableVirusCorruption = true;
        enableGlitchFloat = true;
        
        // 효과 강도 증가
        SetFloatIntensity(1.5f);
        SetGlitchMode(true, 2f);
        SetVirusCorruption(true, 1.2f);
        
        // 일시정지 해제
        SetPaused(false);
        
        // 원래 위치로 리셋 후 새로운 floating 시작
        RefreshVoxelList();
        
        // 특별한 시작 효과 (코루틴)
        StartCoroutine(FloatingStartSequence());
    }
    
    /// <summary>
    /// 변신 복귀 시 특별한 시작 시퀀스
    /// </summary>
    private IEnumerator FloatingStartSequence()
    {
        // 1단계: 강렬한 시작 (2초)
        float originalAmplitude = floatAmplitude;
        floatAmplitude = originalAmplitude * 2f;
        globalIntensityMultiplier = 2f;
        
        yield return new WaitForSeconds(2f);
        
        // 2단계: 점진적 안정화 (3초)
        float stabilizeTime = 3f;
        float elapsed = 0f;
        
        while (elapsed < stabilizeTime)
        {
            float progress = elapsed / stabilizeTime;
            
            // 진폭과 강도를 점진적으로 원래대로
            floatAmplitude = Mathf.Lerp(originalAmplitude * 2f, originalAmplitude, progress);
            globalIntensityMultiplier = Mathf.Lerp(2f, 1f, progress);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 3단계: 정상 상태로 복귀
        floatAmplitude = originalAmplitude;
        globalIntensityMultiplier = 1f;
        
        Debug.Log("[VoxelFloatEffect] 변신 복귀 floating 효과 완료 - 정상 상태");
    }

    public void StartDropEffect()
    {
        Debug.Log("[VoxelFloatEffect] 드롭 효과 시작");
        
        // 드롭 시퀀스 시작
        StartCoroutine(DropSequence());
    }

    public IEnumerator DropSequence()
    {
        Debug.Log("[VoxelFloatEffect] 드롭 시퀀스 시작");

        // 1단계: 조각들을 바닥으로 흘러내리기
        yield return StartCoroutine(DropFragmentsToGround());

        // 2단계: 잠시 대기 (바닥에서 안정화) - 더 빠르게
        yield return new WaitForSeconds(3.0f); // 1f → 0.5f로 단축
        
        Debug.Log("[VoxelFloatEffect] 드롭 시퀀스 완료");
    }

    public void StartRiseEffect()
    {
        Debug.Log("[VoxelFloatEffect] 라이즈 효과 시작");
        
        // 라이즈 시퀀스 시작
        StartCoroutine(EpicGroupRise());
    }

    private IEnumerator RiseSequence()
    {
        Debug.Log("[VoxelFloatEffect] 라이즈 시퀀스 시작");
        
        // 상태 초기화
        SetPaused(true);
        
        // EpicGroupRise 실행
        yield return StartCoroutine(EpicGroupRise());

        // 정상 floating 효과 재개 (더 빠르게)
        yield return new WaitForSeconds(0.2f);
        StartFloatingEffect();

        Debug.Log("[VoxelFloatEffect] 라이즈 시퀀스 완료");
    }

    private IEnumerator DropFragmentsToGround()
    {
        Debug.Log("[VoxelFloatEffect] 조각들을 얌전히 가라앉히기");

        SetPaused(true);

        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel == null) continue;

            // 개별 가라앉기 효과 시작
            StartCoroutine(GentleDropAndSink(voxel));

            yield return null; // 더 빠르게
        }

        yield return null; 
    }

    private IEnumerator GentleDropAndSink(Transform voxel)
    {
        Rigidbody rb = voxel.GetComponent<Rigidbody>();
        // 부드러운 물리 설정
        rb.mass = UnityEngine.Random.Range(0.6f, 1.2f);  // 살짝 다양성
        rb.drag = 4f;
        rb.angularDrag = 10f;

        // 바닥에 닿으면 물리 중단하고 가라앉기 효과
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;

        Vector3 offset = new Vector3(
            UnityEngine.Random.Range(-5f, 5f),  // 좀 더 적당히
            0.0f,
            UnityEngine.Random.Range(-5f, 5f)
        );

        // 가라앉기 애니메이션 (더 빠르게)
        Vector3 midPos = new Vector3(voxel.position.x, -0.2f, voxel.position.z) + offset * 0.5f;
        Vector3 finalPos = new Vector3(voxel.position.x, -0.8f, voxel.position.z) + offset;

        // 바로 최종 위치로 (더 빠르게)
        voxel.transform.DOMove(finalPos, 0.6f).SetEase(Ease.OutQuad); // 1.0f → 0.6f

        // 회전은 전체 시간에 걸쳐 (더 빠르게)
        voxel.transform.DORotate(
            new Vector3(
                UnityEngine.Random.Range(-25f, 25f),
                UnityEngine.Random.Range(0f, 360f),
                UnityEngine.Random.Range(-25f, 25f)
            ),
            0.6f  // 1.0f → 0.6f
        ).SetEase(Ease.OutCubic);
        
        // 크기도 살짝 줄이기 (더 빠르게)
        voxel.transform.DOScale(Vector3.one * 0.8f, 0.6f).SetEase(Ease.InQuad); // 1.0f → 0.6f

        yield return null;
    }

    /// <summary>
    /// 조각들을 솟아올려서 큐브 형태로 재배치
    /// </summary>
    private IEnumerator RiseAndFormCube()
    {
        Debug.Log("[VoxelFloatEffect] 조각들을 솟아올려서 큐브 형태로 재배치 시작");

        // 조각들을 리스트로 변환하고 랜덤 순서로 정렬
        List<Transform> voxelList = new List<Transform>(originalPositions.Keys);
        
        // 거리 기준으로 정렬 (가까운 것부터 올라오게)
        Vector3 centerPos = transform.position;
        voxelList.Sort((a, b) => 
            Vector3.Distance(a.position, centerPos).CompareTo(Vector3.Distance(b.position, centerPos))
        );

        // 각 조각을 순차적으로 솟아올리기
        for (int i = 0; i < voxelList.Count; i++)
        {
            Transform voxel = voxelList[i];
            if (voxel == null) continue;
            
            voxel.GetComponent<Rigidbody>().isKinematic = true;
            
            // 개별 솟아오르기 시작 (순차적 딜레이)
            StartCoroutine(SpectacularRiseAnimation(voxel, i * 0.00001f));
        }
        
        // 모든 조각이 위치할 때까지 대기 (마지막 조각 + 애니메이션 시간)
        float totalTime = (voxelList.Count * 0.15f) + 3f;
        yield return new WaitForSeconds(totalTime);

        SetPaused(false);
        
        Debug.Log("[VoxelFloatEffect] 큐브 형태 재배치 완료");
    }
    
    /// <summary>
    /// 개별 조각의 멋진 솟아오르기 애니메이션
    /// </summary>
    private IEnumerator SpectacularRiseAnimation(Transform voxel, float delay)
    {
        // 딜레이 대기
        yield return new WaitForSeconds(delay);
        
        Vector3 startPos = voxel.position;
        Vector3 targetPos = transform.TransformPoint(originalPositions[voxel]);
        
        // 1단계: 땅에서 솟아오르기 준비 (약간 더 깊이 들어가기)
        Vector3 preparePos = startPos + Vector3.down * 0.3f;
        voxel.transform.DOMove(preparePos, 0.3f).SetEase(Ease.InQuad);
        
        // 크기 살짝 줄이기 (에너지 충전 느낌)
        voxel.transform.DOScale(Vector3.one * 0.6f, 0.3f).SetEase(Ease.InQuad);
        
        yield return new WaitForSeconds(0.3f);
        
        // 2단계: 폭발적으로 솟아오르기
        Vector3 overshootPos = targetPos + Vector3.up * UnityEngine.Random.Range(1f, 2f);
        
        // 곡선 경로로 솟아오르기 (중간 지점 경유)
        Vector3[] path = new Vector3[] {
            preparePos,
            preparePos + new Vector3(
                UnityEngine.Random.Range(-2f, 2f), 
                UnityEngine.Random.Range(2f, 4f), 
                UnityEngine.Random.Range(-2f, 2f)
            ),
            overshootPos
        };
        
        // DOPath로 곡선 이동
        voxel.transform.DOPath(path, 1.5f, PathType.CatmullRom)
            .SetEase(Ease.OutQuart);
        
        // 솟아오르면서 크기 복구 + 회전
        voxel.transform.DOScale(Vector3.one * 1.1f, 1.5f).SetEase(Ease.OutBack);
        
        // 랜덤 회전 (솟아오르면서)
        Vector3 randomRotation = new Vector3(
            UnityEngine.Random.Range(-360f, 360f),
            UnityEngine.Random.Range(-360f, 360f),
            UnityEngine.Random.Range(-360f, 360f)
        );
        voxel.transform.DORotate(randomRotation, 1.5f, RotateMode.FastBeyond360);
        
        yield return new WaitForSeconds(1.5f);
        
        // 3단계: 목표 위치로 정확히 착지
        voxel.transform.DOMove(targetPos, 0.8f).SetEase(Ease.InOutBounce);
        voxel.transform.DORotate(Vector3.zero, 0.8f).SetEase(Ease.OutQuart);
        voxel.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBounce);
        
        yield return new WaitForSeconds(0.8f);
        
        // 4단계: 최종 안정화 (살짝 진동)
        voxel.transform.DOPunchPosition(Vector3.up * 0.1f, 0.5f, 3, 0.5f);
        voxel.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f, 2, 0.3f);
    }
    
    /// <summary>
    /// 더 드라마틱한 단체 솟아오르기 (대안)
    /// </summary>
    public IEnumerator EpicGroupRise()
    {
        Debug.Log("[VoxelFloatEffect] 에픽 단체 솟아오르기 시작");
        
        List<Transform> voxelList = new List<Transform>(originalPositions.Keys);
        
        // 1단계: 모든 조각을 더 깊이 매장 (더 빠르게)
        foreach (Transform voxel in voxelList)
        {
            if (voxel == null) continue;
            
            voxel.GetComponent<Rigidbody>().isKinematic = true;
            
            Vector3 buriedPos = voxel.position + Vector3.down * 1f;
            voxel.transform.DOMove(buriedPos, 0.5f).SetEase(Ease.InQuad); // 더 빠르게
            voxel.transform.DOScale(Vector3.one * 0.3f, 0.5f).SetEase(Ease.InQuad); // 더 빠르게
        }
        
        yield return new WaitForSeconds(0.8f); // 더 빠르게
        
        // 2단계: 파도처럼 순차적으로 솟아오르기 (더 빠르게)
        for (int i = 0; i < voxelList.Count; i++)
        {
            Transform voxel = voxelList[i];
            if (voxel == null) continue;
            
            Vector3 targetPos = transform.TransformPoint(originalPositions[voxel]);
            
            // 높이 오버슈트
            Vector3 overshootPos = targetPos + Vector3.up * 3f;
            
            // 순차적으로 솟아오르기 (더 빠르게)
            voxel.transform.DOMove(overshootPos, 0.5f) // 더 빠르게
                .SetEase(Ease.OutQuart)
                .SetDelay(i * 0.005f); // 더 빠르게
            
            voxel.transform.DOScale(Vector3.one * 1.2f, 0.5f) // 더 빠르게
                .SetEase(Ease.OutBack)
                .SetDelay(i * 0.005f); // 더 빠르게
            
            // 회전
            voxel.transform.DORotate(
                new Vector3(0, UnityEngine.Random.Range(0f, 360f), 0), 
                0.5f // 더 빠르게
            ).SetDelay(i * 0.005f); // 더 빠르게
        }
        
        yield return new WaitForSeconds(0.6f); // 더 빠르게
        
        // 3단계: 정확한 위치로 착지 (더 빠르게)
        for (int i = 0; i < voxelList.Count; i++)
        {
            Transform voxel = voxelList[i];
            if (voxel == null) continue;
            
            Vector3 targetPos = transform.TransformPoint(originalPositions[voxel]);
            
            voxel.transform.DOMove(targetPos, 0.6f) // 더 빠르게
                .SetEase(Ease.OutBounce)
                .SetDelay(i * 0.005f); // 더 빠르게
            
            voxel.transform.DORotate(Vector3.zero, 0.6f) // 더 빠르게
                .SetEase(Ease.OutQuart)
                .SetDelay(i * 0.005f); // 더 빠르게
            
            voxel.transform.DOScale(Vector3.one, 0.6f) // 더 빠르게
                .SetEase(Ease.OutBounce)
                .SetDelay(i * 0.005f); // 더 빠르게
        }
        
        yield return new WaitForSeconds(0.7f); // 더 빠르게
        
        SetPaused(false);
        Debug.Log("[VoxelFloatEffect] 에픽 단체 솟아오르기 완료");
    }
    
    public void ResetToCubeForm()
    {
        Debug.Log("[VoxelFloatEffect] 즉시 큐브 형태로 리셋");
        
        // 원본 voxel들 활성화 및 원래 위치로
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel != null)
            {
                voxel.gameObject.SetActive(true);
                voxel.localPosition = originalPositions[voxel];
            }
        }
        
        // 정상 floating 효과 재개
        SetPaused(false);
        RefreshVoxelList();
    }

    /// <summary>
    /// 긴 드롭 효과 (지정된 시간 동안 드롭 상태 유지)
    /// </summary>
    public void StartLongDropEffect(float duration = 30f)
    {
        Debug.Log($"[VoxelFloatEffect] 긴 드롭 효과 시작 - {duration}초 유지");
        
        // 기존 DOTween 애니메이션 중단
        StopAllCoroutines();
        DOTween.KillAll();
        
        // 긴 드롭 시퀀스 시작
        StartCoroutine(LongDropSequence(duration));
    }

    private IEnumerator LongDropSequence(float duration)
    {
        Debug.Log("[VoxelFloatEffect] 긴 드롭 시퀀스 시작");

        // 1단계: 조각들을 바닥으로 흘러내리기
        yield return StartCoroutine(DropFragmentsToGround());

        // 2단계: 지정된 시간 동안 드롭 상태 유지
        Debug.Log($"[VoxelFloatEffect] 드롭 상태 {duration}초 유지 중...");
        yield return new WaitForSeconds(duration);
        
        Debug.Log("[VoxelFloatEffect] 긴 드롭 시퀀스 완료");
    }

    /// <summary>
    /// 드롭 상태에서 즉시 다음 단계로 진행
    /// </summary>
    public void FinishDropEarly()
    {
        Debug.Log("[VoxelFloatEffect] 드롭 효과 조기 종료");
        StopAllCoroutines();
    }
}