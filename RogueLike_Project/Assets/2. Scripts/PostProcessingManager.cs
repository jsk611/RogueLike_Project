using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    Volume volume;
    Vignette vignette;
    ChromaticAberration chromatic;
    bool isCritical = false;

    // 글리치 효과 관련 변수
    private Coroutine glitchCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
        Vignette tmp;
        if (volume.profile.TryGet<Vignette>(out tmp))
        {
            vignette = tmp;
        }
        ChromaticAberration tmp1;
        if (volume.profile.TryGet<ChromaticAberration>(out tmp1))
        {
            chromatic = tmp1;
        }
        StartCoroutine(VignetteAnimation());
    }

    IEnumerator VignetteAnimation()
    {
        while (true)
        {
            if (isCritical)
            {
                while (vignette.intensity.value < 0.4f)
                {
                    vignette.intensity.value += Time.deltaTime * 0.2f;
                    yield return null;
                }
                while (vignette.intensity.value > 0.2f)
                {
                    vignette.intensity.value -= Time.deltaTime * 0.2f;
                    yield return null;
                }
            }
            else
            {
                while (vignette.intensity.value > 0f)
                {
                    vignette.intensity.value -= Time.deltaTime * 0.3f;
                    yield return null;
                }
            }
            yield return null;
        }
    }

    public void ChangeVignetteColor(Color color)
    {
        vignette.color.value = color;
    }

    public void ChangeChromaticAberrationActive(bool active)
    {
        chromatic.active = active;
        isCritical = active;
    }

    public void DamagedEffect(float intensity)
    {
        if (vignette.color.value == Color.white)
        {
            vignette.color.value = Color.red;
        }
        if (vignette.intensity.value <= 0.02f) vignette.intensity.value = 0.3f;
        if (vignette.intensity.value <= 0.6f) vignette.intensity.value += intensity;
    }

    public void DamagedEffect(float intensity, Color color)
    {
        if (vignette.color.value == Color.white)
        {
            vignette.color.value = color;
        }
        if (vignette.intensity.value <= 0.02f) vignette.intensity.value = 0.3f;
        if (vignette.intensity.value <= 0.6f) vignette.intensity.value += intensity;
    }

    // 새로 추가된 글리치 효과 관련 함수
    public void EnableGlitchEffect(float intensity)
    {
        // 크로마틱 애버레이션 강도 설정
        if (chromatic != null)
        {
            chromatic.active = true;
            chromatic.intensity.value = Mathf.Clamp01(intensity);
        }
    }

    public void DisableGlitchEffect()
    {
        // 진행 중인 글리치 효과가 있다면 중지
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }

        // 크리티컬 상태가 아니면 크로마틱 애버레이션 비활성화
        if (chromatic != null && !isCritical)
        {
            chromatic.active = false;
            chromatic.intensity.value = 0f;
        }
    }

    public void TriggerGlitchEffect(float duration)
    {
        // 기존에 실행 중인 글리치 코루틴이 있다면 중지
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }

        // 새로운 글리치 효과 코루틴 시작
        glitchCoroutine = StartCoroutine(GlitchEffectRoutine(duration));
    }

    private IEnumerator GlitchEffectRoutine(float duration)
    {
        if (chromatic == null) yield break;

        // 원래 크로마틱 애버레이션 상태 저장
        bool originalActive = chromatic.active;
        float originalIntensity = chromatic.intensity.value;

        // 글리치 효과 활성화
        chromatic.active = true;

        // 글리치 효과 강도 증가 (0.2초)
        float startTime = Time.time;
        float riseTime = 0.2f;

        while (Time.time - startTime < riseTime)
        {
            float t = (Time.time - startTime) / riseTime;
            chromatic.intensity.value = Mathf.Lerp(originalIntensity, 1.0f, t);
            yield return null;
        }

        // 최대 강도로 유지
        chromatic.intensity.value = 1.0f;

        // 지속 시간의 60% 동안 유지
        float holdTime = duration * 0.6f - riseTime;
        if (holdTime > 0)
            yield return new WaitForSeconds(holdTime);

        // 다시 원래 상태로 감소 (0.2초)
        startTime = Time.time;
        float fallTime = 0.2f;

        while (Time.time - startTime < fallTime)
        {
            float t = (Time.time - startTime) / fallTime;
            chromatic.intensity.value = Mathf.Lerp(1.0f, originalIntensity, t);
            yield return null;
        }

        // 원래 상태로 복원 (isCritical에 따라)
        if (!isCritical)
        {
            chromatic.active = originalActive;
        }
        chromatic.intensity.value = originalIntensity;

        glitchCoroutine = null;
    }
}