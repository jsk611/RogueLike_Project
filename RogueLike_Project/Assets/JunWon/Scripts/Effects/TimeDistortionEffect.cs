using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TimeDistortionEffect : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("감속 시 시간 스케일 값")]
    public float slowdownFactor = 0.05f;
    [Tooltip("전체 감속 지속 시간 (초)")]
    public float slowdownDuration = 0.3f;
    [Tooltip("복구 시 애니메이션 커브 (0: 감속상태, 1: 정상)")]
    public AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Effects")]
    public PostProcessVolume postProcessVolume;
    [Tooltip("최대 크로매틱 어베레이션 강도")]
    public float maxChromaticIntensity = 1f;
    [Tooltip("최대 모션 블러 셔터 앵글")]
    public float maxMotionBlurShutterAngle = 270f;
    [Tooltip("최대 모션 블러 샘플 수")]
    public int maxMotionBlurSampleCount = 10;

    private ChromaticAberration chromaticAberration;
    private MotionBlur motionBlur;

    private float defaultFixedDeltaTime;
    private Coroutine currentEffect;

    void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out motionBlur);
        }
        else
        {
            Debug.LogWarning("PostProcessVolume 또는 프로필이 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 외부에서 이 함수를 호출하면 타임 디스토션 효과를 트리거합니다.
    /// </summary>
    public void TriggerEffect()
    {
        // 이미 효과 실행 중이면 중단 후 새 효과 시작
        if (currentEffect != null)
        {
            StopCoroutine(currentEffect);
        }
        currentEffect = StartCoroutine(TimeDistortionSequence());
    }

    IEnumerator TimeDistortionSequence()
    {
        // --- 감속 및 효과 시작 ---
        // 즉시 시간 감속
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = defaultFixedDeltaTime * slowdownFactor;

        // 포스트 프로세싱 효과 적용
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = maxChromaticIntensity;
        }
        if (motionBlur != null)
        {
            motionBlur.shutterAngle.value = maxMotionBlurShutterAngle;
            motionBlur.sampleCount.value = maxMotionBlurSampleCount;
        }

        // 감속 상태를 잠시 유지 (전체 시간의 절반)
        yield return new WaitForSecondsRealtime(slowdownDuration * 0.5f);

        // --- 복구 단계: 시간 및 시각 효과 부드럽게 복구 ---
        float elapsed = 0f;
        float recoveryDuration = slowdownDuration * 0.5f;
        while (elapsed < recoveryDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / recoveryDuration);
            float curveValue = recoveryCurve.Evaluate(t);

            // 시간 스케일 복구
            Time.timeScale = Mathf.Lerp(slowdownFactor, 1f, curveValue);
            Time.fixedDeltaTime = Mathf.Lerp(defaultFixedDeltaTime * slowdownFactor, defaultFixedDeltaTime, curveValue);

            // 크로매틱 어베레이션 감소
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(maxChromaticIntensity, 0f, curveValue);
            }
            // 모션 블러도 자연스럽게 복구 (원하는 경우)
            if (motionBlur != null)
            {
                motionBlur.shutterAngle.value = Mathf.Lerp(maxMotionBlurShutterAngle, 0f, curveValue);
                // sampleCount는 정수이므로 보간 후 반올림 처리
                motionBlur.sampleCount.value = Mathf.RoundToInt(Mathf.Lerp(maxMotionBlurSampleCount, 0, curveValue));
            }

            yield return null;
        }

        // --- 최종 값 보정 ---
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }
        if (motionBlur != null)
        {
            motionBlur.shutterAngle.value = 0f;
            motionBlur.sampleCount.value = 0;
        }

        currentEffect = null;
    }

    void OnDisable()
    {
        // 스크립트가 비활성화될 때 안전하게 시간 스케일 복구
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
