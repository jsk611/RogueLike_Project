using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionDuration = 1.5f;
    [SerializeField] private float maxExplosionRadius = 10.0f;
    [SerializeField] private float baseDistortion = 0.2f;
    [SerializeField] private AnimationCurve explosionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve distortionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Shockwave Settings")]
    [SerializeField] private float shockwaveSpeed = 5f;
    [SerializeField] private float shockwaveThickness = 0.5f;
    [SerializeField] private float shockwaveIntensity = 2f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private GameObject shockwaveRing;
    [SerializeField] private Light explosionLight;

    private Material explosionMaterial;
    private Material shockwaveMaterial;
    private Vector3 originalScale;
    private float startTime;
    private bool hasTriggeredDamage;

    private void Start()
    {
        InitializeComponents();
        SetupExplosion();
        StartCoroutine(ExplosionSequence());
    }

    private void InitializeComponents()
    {
        // 메인 폭발 이펙트 초기화
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            explosionMaterial = renderer.material;
        }
        else
        {
            Debug.LogError("Renderer component not found on explosion prefab!");
            return;
        }

        // 쇼크웨이브 초기화
        if (shockwaveRing != null)
        {
            var shockwaveRenderer = shockwaveRing.GetComponent<Renderer>();
            if (shockwaveRenderer != null)
            {
                shockwaveMaterial = shockwaveRenderer.material;
            }
        }

        originalScale = transform.localScale;
    }

    private void SetupExplosion()
    {
        startTime = Time.time;
        hasTriggeredDamage = false;

        // 폭발 파티클 시스템 설정
        if (explosionParticles != null)
        {
            var main = explosionParticles.main;
            main.duration = explosionDuration;
            explosionParticles.Play();
        }

        // 폭발 라이트 설정
        if (explosionLight != null)
        {
            StartCoroutine(ExplosionLightEffect());
        }
    }

    private IEnumerator ExplosionSequence()
    {
        float elapsedTime = 0f;

        while (elapsedTime < explosionDuration)
        {
            float normalizedTime = elapsedTime / explosionDuration;

            // 폭발 반경 업데이트
            float explosionProgress = explosionCurve.Evaluate(normalizedTime);
            float currentRadius = maxExplosionRadius * explosionProgress;

            // 왜곡 효과 업데이트
            float distortionProgress = distortionCurve.Evaluate(normalizedTime);
            float currentDistortion = baseDistortion * distortionProgress;

            // 머티리얼 프로퍼티 업데이트
            if (explosionMaterial != null)
            {
                explosionMaterial.SetFloat("_ExplosionRadius", currentRadius);
                explosionMaterial.SetFloat("_Distortion", currentDistortion);
                transform.localScale = originalScale * (1.0f + currentRadius);
            }

            // 쇼크웨이브 업데이트
            UpdateShockwave(normalizedTime);

            // 데미지 트리거 (한 번만)
            if (!hasTriggeredDamage && normalizedTime >= 0.1f)
            {
                TriggerExplosionDamage();
                hasTriggeredDamage = true;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 폭발 종료 처리
        OnExplosionComplete();
    }

    private void UpdateShockwave(float progress)
    {
        if (shockwaveMaterial != null)
        {
            float shockwaveRadius = progress * shockwaveSpeed;
            shockwaveMaterial.SetFloat("_Radius", shockwaveRadius);
            shockwaveMaterial.SetFloat("_Thickness", shockwaveThickness * (1 - progress));
            shockwaveMaterial.SetFloat("_Intensity", shockwaveIntensity * (1 - progress));
        }
    }

    private IEnumerator ExplosionLightEffect()
    {
        float intensity = explosionLight.intensity;
        float range = explosionLight.range;

        while (explosionLight.intensity > 0)
        {
            float normalizedTime = (Time.time - startTime) / explosionDuration;
            explosionLight.intensity = Mathf.Lerp(intensity, 0, normalizedTime);
            explosionLight.range = Mathf.Lerp(range, 0, normalizedTime);
            yield return null;
        }
    }

    private void TriggerExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            // 거리에 따른 데미지 계산
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            float damageRatio = 1 - (distance / maxExplosionRadius);
            if (damageRatio > 0)
            {
                var damageable = hitCollider.GetComponent<PlayerStatus>();
                if (damageable != null)
                {
                    float damage = 100 * damageRatio; // 기본 데미지 값 조정 가능
                    damageable.DecreaseHealth(damage);
                }
            }
        }
    }

    private void OnExplosionComplete()
    {
        // 파티클 시스템 정리
        if (explosionParticles != null)
        {
            explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 오브젝트 제거
        Destroy(gameObject, 0.5f); // 파티클이 완전히 사라질 때까지 약간의 딜레이
    }

}
