using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalShadow : RangedMonster
{
    public delegate void ShadowDestroyedHandler(GameObject shadow);
    public event ShadowDestroyedHandler OnShadowDestroyed;

    [Header("Digital Shadow Settings")]
    [SerializeField] private float health = 30f;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private Material glitchMaterial;

    private float spawnTime;
    private bool isDying = false;
    private Ransomware creator; // 생성한 보스 참조

    // 디지털 섀도우 초기화 메서드
    public void Initialize(Ransomware boss)
    {
        creator = boss;
        spawnTime = Time.time;

        // 글리치 효과 적용
        ApplyGlitchEffect();

        // 생명 시간 후 자동 파괴 코루틴 시작
        StartCoroutine(LifetimeRoutine());
    }

    protected override void Start()
    {
        base.Start();

        // RangedMonster의 Start() 호출 후 추가 설정
        attackCooldown = 3f; // 더 자주 공격하도록 쿨다운 감소
        chaseSpeed = 4.5f; // 약간 더 빠르게 이동
        attackRange = 10f; // 공격 범위 조정

        // 이동 및 애니메이션 설정
        if (nmAgent != null)
        {
            nmAgent.speed = chaseSpeed;
        }
    }

    protected override void Update()
    {
        base.Update();

        // 추가로 디지털 섀도우만의 시각 효과 업데이트 가능
        UpdateGlitchEffect();
    }

    // 데미지 처리 오버라이드
    public virtual void TakeDamage(float damage, bool showDamage = true, bool flagForExecution = false)
    {
        if (isDying) return;

        health -= damage;

        // 데미지 표시 (RangedMonster에서 상속)
        if (showDamage)
        {
            base.TakeDamage(damage, true);
        }

        // 피격 효과
        StartCoroutine(DamageFlashRoutine());

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;

        // 사망 효과 생성
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // 보스에게 섀도우 파괴 알림
        OnShadowDestroyed?.Invoke(gameObject);

        // 파괴 전 애니메이션/효과 표시
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 사망 애니메이션 설정
        SetAnimatorState(State.DIE);

        // NavMeshAgent 중지
        if (nmAgent != null)
        {
            nmAgent.isStopped = true;
        }

        // 1초 대기 (사망 애니메이션 재생)
        yield return new WaitForSeconds(1f);

        // 오브젝트 파괴
        Destroy(gameObject);
    }

    private IEnumerator DamageFlashRoutine()
    {
        // 피격 시 머티리얼 깜빡임
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_FlashIntensity", 1f);
            }
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_FlashIntensity", 0f);
            }
        }
    }

    private void ApplyGlitchEffect()
    {
        // 모든 렌더러에 글리치 셰이더 적용
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            if (glitchMaterial != null)
            {
                // 기존 머티리얼을 글리치 머티리얼로 교체
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterials[i] = new Material(glitchMaterial);
                }
                renderer.materials = newMaterials;
            }
            else
            {
                // 글리치 머티리얼이 없는 경우 기존 머티리얼에 글리치 효과 속성 추가
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_GlitchIntensity"))
                    {
                        material.SetFloat("_GlitchIntensity", 0.3f);
                    }
                    if (material.HasProperty("_DistortionAmount"))
                    {
                        material.SetFloat("_DistortionAmount", 0.02f);
                    }
                }
            }
        }
    }

    private void UpdateGlitchEffect()
    {
        // 남은 생명 시간에 따라 글리치 효과 강화
        float remainingLifetimePercent = 1f - ((Time.time - spawnTime) / lifetime);
        float glitchIntensity = Mathf.Lerp(0.8f, 0.3f, remainingLifetimePercent);

        // 낮은 체력에서도 글리치 효과 강화
        float healthPercent = health / 30f; // 초기 체력 대비 비율
        glitchIntensity = Mathf.Max(glitchIntensity, Mathf.Lerp(0.8f, 0.3f, healthPercent));

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_GlitchIntensity"))
                {
                    material.SetFloat("_GlitchIntensity", glitchIntensity);
                }
            }
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        if (!isDying)
        {
            Die();
        }
    }

    public override void FireEvent()
    {
        base.FireEvent();

        // 추가 효과: 발사 시 글리치 효과 강화
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_GlitchIntensity"))
                {
                    // 발사 순간 글리치 효과 강화
                    StartCoroutine(FireGlitchEffect(material));
                }
            }
        }
    }

    private IEnumerator FireGlitchEffect(Material material)
    {
        float originalIntensity = material.GetFloat("_GlitchIntensity");
        material.SetFloat("_GlitchIntensity", 1.0f);
        yield return new WaitForSeconds(0.2f);
        material.SetFloat("_GlitchIntensity", originalIntensity);
    }
}
