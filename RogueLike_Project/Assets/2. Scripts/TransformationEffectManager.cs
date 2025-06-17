using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 변신 과정의 부자연스러움을 줄이는 파티클 및 쉐이더 연출 매니저
/// </summary>
public class TransformationEffectManager : MonoBehaviour
{
    [Header("Dissolve Effects")]
    [SerializeField] private ParticleSystem dissolveDustEffect;
    [SerializeField] private ParticleSystem digitalFragmentEffect;
    [SerializeField] private ParticleSystem energyRippleEffect;
    
    [Header("Emergence Effects")]
    [SerializeField] private ParticleSystem groundCrackEffect;
    [SerializeField] private ParticleSystem earthDebrisEffect;
    [SerializeField] private ParticleSystem emergenceShockwaveEffect;
    [SerializeField] private ParticleSystem steamEffect;
    
    [Header("Transformation Core Effects")]
    [SerializeField] private ParticleSystem coreEnergyEffect;
    [SerializeField] private ParticleSystem dataStreamEffect;
    [SerializeField] private ParticleSystem hologramGlitchEffect;
    
    [Header("Materials")]
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Material hologramMaterial;
    [SerializeField] private Material emergenceMaterial;
    
    [Header("Settings")]
    [SerializeField] private float effectDuration = 2f;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useScreenSpaceEffect = true;
    
    private Camera mainCamera;
    private Material originalSkybox;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalSkybox = RenderSettings.skybox;
        }
    }
    
    #region Public API
    
    /// <summary>
    /// 의태식물 스타일 등장 효과
    /// </summary>
    public void PlayMimicPlantEmergence(Vector3 position, float duration = 3f)
    {
        StartCoroutine(ExecuteMimicPlantEmergence(position, duration));
    }
    
    /// <summary>
    /// 디지털 디졸브 효과
    /// </summary>
    public void PlayDigitalDissolve(Transform target, float duration = 2f)
    {
        StartCoroutine(ExecuteDigitalDissolve(target, duration));
    }
    
    /// <summary>
    /// 홀로그램 재조립 효과
    /// </summary>
    public void PlayHologramReassembly(Transform target, Vector3 targetPosition, float duration = 2f)
    {
        StartCoroutine(ExecuteHologramReassembly(target, targetPosition, duration));
    }
    
    #endregion
    
    #region Mimic Plant Emergence
    
    private IEnumerator ExecuteMimicPlantEmergence(Vector3 position, float duration)
    {
        // 1단계: 지면 경고 효과 (0.5초)
        yield return StartCoroutine(GroundWarningPhase(position, duration * 0.15f));
        
        // 2단계: 지면 균열 (0.8초)  
        yield return StartCoroutine(GroundCrackPhase(position, duration * 0.25f));
        
        // 3단계: 폭발적 등장 (1.2초)
        yield return StartCoroutine(ExplosiveEmergencePhase(position, duration * 0.4f));
        
        // 4단계: 안정화 (0.5초)
        yield return StartCoroutine(StabilizationPhase(position, duration * 0.2f));
    }
    
    private IEnumerator GroundWarningPhase(Vector3 position, float duration)
    {
        // 미묘한 진동 효과
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(duration, 0.05f, 20, 90, false, true);
        }
        
        // 에너지 리플 효과
        if (energyRippleEffect != null)
        {
            var ripple = Instantiate(energyRippleEffect, position, Quaternion.identity);
            var main = ripple.main;
            main.startLifetime = duration;
            ripple.Play();
            Destroy(ripple.gameObject, duration + 1f);
        }
        
        yield return new WaitForSeconds(duration);
    }
    
    private IEnumerator GroundCrackPhase(Vector3 position, float duration)
    {
        // 땅 갈라지는 파티클
        if (groundCrackEffect != null)
        {
            var crack = Instantiate(groundCrackEffect, position, Quaternion.identity);
            var main = crack.main;
            main.startLifetime = duration;
            
            // 점진적으로 강해지는 효과
            var emission = crack.emission;
            emission.rateOverTime = 0;
            
            DOTween.To(() => emission.rateOverTime.constant, 
                      x => emission.rateOverTime = x, 
                      50, duration * 0.8f);
            
            crack.Play();
            Destroy(crack.gameObject, duration + 2f);
        }
        
        // 카메라 진동 강화
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(duration, 0.15f, 30, 90, false, true);
        }
        
        yield return new WaitForSeconds(duration);
    }
    
    private IEnumerator ExplosiveEmergencePhase(Vector3 position, float duration)
    {
        // 폭발적 등장 효과
        if (emergenceShockwaveEffect != null)
        {
            var shockwave = Instantiate(emergenceShockwaveEffect, position, Quaternion.identity);
            shockwave.Play();
            Destroy(shockwave.gameObject, 3f);
        }
        
        // 흙/잔해 파티클
        if (earthDebrisEffect != null)
        {
            var debris = Instantiate(earthDebrisEffect, position, Quaternion.identity);
            var velocityOverLifetime = debris.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(5f, 15f);
            
            debris.Play();
            Destroy(debris.gameObject, 4f);
        }
        
        // 스팀/연기 효과
        if (steamEffect != null)
        {
            var steam = Instantiate(steamEffect, position + Vector3.up * 0.5f, Quaternion.identity);
            steam.Play();
            Destroy(steam.gameObject, duration + 2f);
        }
        
        // 강한 카메라 진동
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(0.3f, 0.8f, 50, 90, false, true);
        }
        
        yield return new WaitForSeconds(duration);
    }
    
    private IEnumerator StabilizationPhase(Vector3 position, float duration)
    {
        // 안정화 파티클 (먼지 가라앉기)
        if (dissolveDustEffect != null)
        {
            var dust = Instantiate(dissolveDustEffect, position, Quaternion.identity);
            var main = dust.main;
            main.gravityModifier = 0.5f;
            
            dust.Play();
            Destroy(dust.gameObject, duration + 1f);
        }
        
        yield return new WaitForSeconds(duration);
    }
    
    #endregion
    
    #region Digital Effects
    
    private IEnumerator ExecuteDigitalDissolve(Transform target, float duration)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Material originalMaterial = renderer.material;
        
        // 디졸브 머티리얼 적용
        if (dissolveMaterial != null)
        {
            renderer.material = dissolveMaterial;
            
            // 디졸브 파라미터 애니메이션
            float dissolveAmount = 0f;
            DOTween.To(() => dissolveAmount, x => {
                dissolveAmount = x;
                if (dissolveMaterial.HasProperty("_DissolveAmount"))
                    dissolveMaterial.SetFloat("_DissolveAmount", x);
            }, 1f, duration);
        }
        
        // 디지털 파편 효과
        if (digitalFragmentEffect != null)
        {
            var fragments = Instantiate(digitalFragmentEffect, target.position, target.rotation);
            fragments.Play();
            Destroy(fragments.gameObject, duration + 1f);
        }
        
        yield return new WaitForSeconds(duration);
        
        // 원래 머티리얼 복원
        renderer.material = originalMaterial;
    }
    
    private IEnumerator ExecuteHologramReassembly(Transform target, Vector3 targetPosition, float duration)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Material originalMaterial = renderer.material;
        
        // 홀로그램 효과 시작
        if (hologramMaterial != null)
        {
            renderer.material = hologramMaterial;
            
            // 홀로그램 글리치 효과
            if (hologramGlitchEffect != null)
            {
                var glitch = Instantiate(hologramGlitchEffect, target.position, Quaternion.identity);
                glitch.transform.SetParent(target);
                glitch.Play();
                Destroy(glitch.gameObject, duration);
            }
            
            // 데이터 스트림 효과
            if (dataStreamEffect != null)
            {
                var stream = Instantiate(dataStreamEffect, target.position, Quaternion.identity);
                
                // 스트림이 목표 지점으로 이동
                stream.transform.DOMove(targetPosition, duration * 0.8f);
                stream.Play();
                Destroy(stream.gameObject, duration);
            }
        }
        
        yield return new WaitForSeconds(duration * 0.8f);
        
        // 최종 머티리얼 복원
        renderer.material = originalMaterial;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// 스크린 스페이스 글리치 효과
    /// </summary>
    public void ApplyScreenGlitch(float intensity, float duration)
    {
        StartCoroutine(ExecuteScreenGlitch(intensity, duration));
    }
    
    private IEnumerator ExecuteScreenGlitch(float intensity, float duration)
    {
        if (!useScreenSpaceEffect || mainCamera == null) yield break;
        
        // 후처리 효과나 셰이더를 사용한 화면 글리치
        // 여기서는 간단하게 카메라 셰이크로 대체
        mainCamera.transform.DOShakePosition(duration, intensity * 0.1f, 50, 90, false, true);
        mainCamera.transform.DOShakeRotation(duration, intensity * 2f, 50, 90, true);
        
        yield return new WaitForSeconds(duration);
    }
    
    /// <summary>
    /// 환경 효과 (조명, 안개 등)
    /// </summary>
    public void ApplyEnvironmentEffect(Color fogColor, float fogDensity, float duration)
    {
        StartCoroutine(ExecuteEnvironmentEffect(fogColor, fogDensity, duration));
    }
    
    private IEnumerator ExecuteEnvironmentEffect(Color targetFogColor, float targetFogDensity, float duration)
    {
        Color originalFogColor = RenderSettings.fogColor;
        float originalFogDensity = RenderSettings.fogDensity;
        
        // 안개 색상과 밀도 변화
        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, targetFogColor, duration * 0.3f);
        DOTween.To(() => RenderSettings.fogDensity, x => RenderSettings.fogDensity = x, targetFogDensity, duration * 0.3f);
        
        yield return new WaitForSeconds(duration * 0.6f);
        
        // 원래 상태로 복원
        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, originalFogColor, duration * 0.4f);
        DOTween.To(() => RenderSettings.fogDensity, x => RenderSettings.fogDensity = x, originalFogDensity, duration * 0.4f);
    }
    
    #endregion
} 