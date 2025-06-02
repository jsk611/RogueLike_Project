using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 강화된 복셀 플로팅 효과 - 바이러스 큐브용
/// </summary>
public class VoxelFloatEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatAmplitude = 0.3f;    // 떠다니는 강도
    public float floatSpeed = 1f;          // 떠다니는 속도
    public float randomOffset = 0.2f;      // 각 복셀마다 다른 움직임

    [Header("Advanced Effects")]
    public bool enableOrbitalMotion = true;        // 궤도 운동 활성화
    public float orbitalRadius = 0.1f;             // 궤도 반지름
    public float orbitalSpeed = 2f;                // 궤도 속도
    public bool enablePulseEffect = true;          // 펄스 효과
    public float pulseIntensity = 0.05f;           // 펄스 강도
    public bool enableGlitchFloat = false;         // 글리치 플로팅
    public float glitchChance = 0.1f;              // 글리치 발생 확률
    public float glitchDuration = 0.2f;            // 글리치 지속 시간

    [Header("Virus Effects")]
    public bool enableVirusCorruption = true;     // 바이러스 부패 효과
    public float corruptionIntensity = 0.15f;     // 부패 강도
    public float corruptionSpeed = 3f;            // 부패 속도

    // 복셀들의 원래 위치 저장
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> voxelOffsets = new Dictionary<Transform, float>();
    private Dictionary<Transform, Vector3> orbitalOffsets = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> glitchTimers = new Dictionary<Transform, float>();
    private Dictionary<Transform, Vector3> glitchTargets = new Dictionary<Transform, Vector3>();

    // 효과 제어 변수
    private float globalIntensityMultiplier = 1f;
    private bool isPaused = false;

    void Start()
    {
        // 자식 객체들(복셀들)의 원래 위치 저장
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
        // 모든 자식 복셀들의 원래 위치와 개별 오프셋 저장
        foreach (Transform child in transform)
        {
            originalPositions[child] = child.localPosition;
            voxelOffsets[child] = Random.Range(0f, 2f * Mathf.PI); // 각각 다른 시작점

            // 궤도 운동을 위한 랜덤 오프셋
            orbitalOffsets[child] = Random.insideUnitSphere * 0.1f;

            // 글리치 타이머 초기화
            glitchTimers[child] = 0f;
            glitchTargets[child] = Vector3.zero;
        }
    }

    private void FloatVoxels()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel == null) continue;

            // 각 복셀마다 다른 패턴으로 떠다니게
            float timeOffset = voxelOffsets[voxel];
            Vector3 finalOffset = Vector3.zero;

            // 1. 기본 플로팅 효과
            finalOffset += CalculateBasicFloat(timeOffset);

            // 2. 궤도 운동 효과
            if (enableOrbitalMotion)
            {
                finalOffset += CalculateOrbitalMotion(voxel, timeOffset);
            }

            // 3. 펄스 효과
            if (enablePulseEffect)
            {
                finalOffset += CalculatePulseEffect(timeOffset);
            }

            // 4. 글리치 효과
            if (enableGlitchFloat)
            {
                finalOffset += CalculateGlitchEffect(voxel);
            }

            // 5. 바이러스 부패 효과
            if (enableVirusCorruption)
            {
                finalOffset += CalculateVirusCorruption(voxel, timeOffset);
            }

            // 글로벌 강도 적용
            finalOffset *= globalIntensityMultiplier;

            // 최종 위치 적용
            Vector3 targetPosition = originalPositions[voxel] + finalOffset;
            voxel.localPosition = Vector3.Lerp(voxel.localPosition, targetPosition, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 기본 플로팅 계산
    /// </summary>
    private Vector3 CalculateBasicFloat(float timeOffset)
    {
        // Y축 상하 움직임
        float floatY = Mathf.Sin((Time.time * floatSpeed) + timeOffset) * floatAmplitude;

        // X, Z축도 살짝 움직임 (더 자연스럽게)
        float floatX = Mathf.Cos((Time.time * floatSpeed * 0.7f) + timeOffset) * (floatAmplitude * 0.3f);
        float floatZ = Mathf.Sin((Time.time * floatSpeed * 0.5f) + timeOffset) * (floatAmplitude * 0.3f);

        return new Vector3(floatX, floatY, floatZ) * randomOffset;
    }

    /// <summary>
    /// 궤도 운동 계산
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
    /// 글리치 효과 계산
    /// </summary>
    private Vector3 CalculateGlitchEffect(Transform voxel)
    {
        // 글리치 타이머 업데이트
        glitchTimers[voxel] -= Time.deltaTime;

        // 새로운 글리치 시작 체크
        if (glitchTimers[voxel] <= 0f && Random.value < glitchChance * Time.deltaTime)
        {
            glitchTimers[voxel] = glitchDuration;
            glitchTargets[voxel] = Random.insideUnitSphere * 0.3f;
        }

        // 글리치 효과 적용
        if (glitchTimers[voxel] > 0f)
        {
            float intensity = glitchTimers[voxel] / glitchDuration;
            return glitchTargets[voxel] * intensity;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 바이러스 부패 효과 계산
    /// </summary>
    private Vector3 CalculateVirusCorruption(Transform voxel, float timeOffset)
    {
        float corruptionTime = Time.time * corruptionSpeed + timeOffset;

        // 불규칙한 진동과 왜곡
        Vector3 corruption = new Vector3(
            Mathf.PerlinNoise(corruptionTime, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, corruptionTime) - 0.5f,
            Mathf.PerlinNoise(corruptionTime, corruptionTime) - 0.5f
        );

        return corruption * corruptionIntensity;
    }

    // 떠다니는 강도 조절 (외부에서 호출 가능)
    public void SetFloatIntensity(float intensity)
    {
        globalIntensityMultiplier = intensity;
        floatAmplitude = 0.3f * intensity;
        floatSpeed = 1f * intensity;
        randomOffset = 0.2f * intensity;
    }

    /// <summary>
    /// 글리치 모드 토글
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
    /// 바이러스 부패 모드 설정
    /// </summary>
    public void SetVirusCorruption(bool enabled, float intensity = 1f)
    {
        enableVirusCorruption = enabled;
        corruptionIntensity = 0.15f * intensity;
        corruptionSpeed = 3f * intensity;
    }

    /// <summary>
    /// 궤도 운동 설정
    /// </summary>
    public void SetOrbitalMotion(bool enabled, float radius = 0.1f, float speed = 2f)
    {
        enableOrbitalMotion = enabled;
        orbitalRadius = radius;
        orbitalSpeed = speed;
    }

    /// <summary>
    /// 모든 효과 일시 정지/재개
    /// </summary>
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    /// <summary>
    /// 차징 상태 설정 (공격 준비시)
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
            SetFloatIntensity(0.2f); // 공격시에는 떠다니는 효과 최소화
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
    /// 소멸 모드 설정
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
    /// 모든 복셀을 원래 위치로 리셋
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
    /// 새로 추가된 자식 복셀들을 초기화
    /// </summary>
    public void RefreshVoxelList()
    {
        // 기존 데이터 클리어
        originalPositions.Clear();
        voxelOffsets.Clear();
        orbitalOffsets.Clear();
        glitchTimers.Clear();
        glitchTargets.Clear();

        // 재초기화
        InitializeVoxelPositions();
    }

    /// <summary>
    /// 디버그용 기즈모
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
}