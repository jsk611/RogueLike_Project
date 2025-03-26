using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DigitalShadow : RangedMonster
{
    public delegate void ShadowDestroyedHandler(GameObject shadow);
    public event ShadowDestroyedHandler OnShadowDestroyed;

    [Header("Digital Shadow Settings")]
    [SerializeField] private float health = 30f;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private Material glitchMaterial;

    [Header("AI Movement Settings")]
    [SerializeField] private float minDistanceFromPlayer = 5f; // 플레이어로부터 최소 거리
    [SerializeField] private float maxDistanceFromPlayer = 10f; // 플레이어로부터 최대 거리
    [SerializeField] private float minDistanceFromOthers = 3f; // 다른 그림자로부터 최소 거리
    [SerializeField] private float positionUpdateInterval = 2f; // 위치 재조정 간격
    [SerializeField] private float flankingOffset = 45f; // 측면 공격을 위한 각도 오프셋

    [Header("Random Speed Settings")]
    [SerializeField] private float minSpeed = 3.5f; // 최소 이동 속도
    [SerializeField] private float maxSpeed = 5.5f; // 최대 이동 속도
    [SerializeField] private float speedChangeInterval = 3f; // 속도 변경 간격 (초)
    [SerializeField] private float speedChangeVariation = 1f; // 간격의 랜덤 변화량

    private float spawnTime;
    private bool isDying = false;
    private Ransomware creator; // 생성한 보스 참조
    private bool isLastStandFragment = false; // 발악 패턴용 분열 조각인지 여부
    private float lastPositionUpdateTime = 0f;
    private int shadowID; // 그림자 고유 ID (생성 순서)
    private static int nextShadowID = 0; // 다음 그림자에 할당할 ID

    // 디지털 섀도우 초기화 메서드
    public void Initialize(Ransomware boss)
    {
        creator = boss;
        spawnTime = Time.time;
        shadowID = nextShadowID++;

        // 글리치 효과 적용
        ApplyGlitchEffect();

        // 자연스러운 움직임을 위한 초기 설정
        InitializeMovementBehavior();

        // 생명 시간 후 자동 파괴 코루틴 시작
        StartCoroutine(LifetimeRoutine());

        // 속도 랜덤화 코루틴 시작
        StartCoroutine(RandomizeSpeedRoutine());
    }

    // 발악 패턴용 분열 조각 설정
    public void SetAsLastStandFragment(bool value)
    {
        isLastStandFragment = value;

        // 발악 패턴용이면 체력과 공격력 증가
        if (isLastStandFragment)
        {
            monsterStatus.SetHealth(1.5f * monsterStatus.GetMaxHealth()); 
            monsterStatus.SetAttackDamage(1.2f * monsterStatus.GetAttackDamage());
            attackCooldown *= 0.8f; // 공격 주기 0.8배 (더 빠른 공격)

            // 시각적 효과 강화
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_GlitchIntensity"))
                    {
                        material.SetFloat("_GlitchIntensity", 0.5f); // 기본 효과보다 더 강한 글리치
                    }

                    // 더 붉은 색조 적용
                    if (material.HasProperty("_Color"))
                    {
                        Color originalColor = material.GetColor("_Color");
                        material.SetColor("_Color", new Color(
                            Mathf.Min(originalColor.r + 0.3f, 1.0f),
                            Mathf.Max(originalColor.g - 0.2f, 0.0f),
                            Mathf.Max(originalColor.b - 0.2f, 0.0f),
                            originalColor.a
                        ));
                    }
                }
            }

            // 파티클 시스템 조정 (있는 경우)
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.startColor = new Color(1.0f, 0.3f, 0.3f); // 더 붉은 파티클
                main.startLifetime = main.startLifetime.constant * 1.5f; // 파티클 지속시간 증가
                main.startSize = main.startSize.constant * 1.2f; // 파티클 크기 증가
            }

            // 발악 패턴에서는 속도 범위를 더 넓게 설정
            minSpeed *= 0.9f;
            maxSpeed *= 1.2f;
        }
    }

    // 속도를 주기적으로 랜덤하게 변경하는 코루틴
    private IEnumerator RandomizeSpeedRoutine()
    {
        // 초기 딜레이 (모든 그림자가 동시에 속도를 변경하지 않도록)
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        while (!isDying)
        {
            if (nmAgent != null && nmAgent.enabled)
            {
                // 새로운 랜덤 속도 설정
                float newSpeed = Random.Range(minSpeed, maxSpeed);
                nmAgent.speed = newSpeed;
                chaseSpeed = newSpeed; // RangedMonster에서 참조하는 변수도 업데이트

                // ID에 따라 약간 다른 속도 패턴 사용
                if (shadowID % 3 == 0)
                {
                    // 속도 변화가 더 급격한 패턴
                    nmAgent.acceleration = Random.Range(8f, 12f);
                }
                else if (shadowID % 3 == 1)
                {
                    // 속도 변화가 부드러운 패턴
                    nmAgent.acceleration = Random.Range(6f, 9f);
                }
                else
                {
                    // 중간 패턴
                    nmAgent.acceleration = Random.Range(7f, 10f);
                }

                // 현재 속도에 따라 애니메이션 속도 조정
                if (anim != null)
                {
                    float animSpeedMultiplier = newSpeed / 4.5f; // 기본 속도 대비
                    anim.SetFloat("SpeedMultiplier", animSpeedMultiplier);
                }

                // 공격 관련 값도 랜덤 조정 (너무 빈번한 공격은 방지)
                attackCooldown = Random.Range(2.5f, 4.0f);
                aimTime = attackCooldown * 0.6f;
                attackTime = attackCooldown * 0.8f;
            }

            // 다음 속도 변경까지 랜덤 시간 대기
            float waitTime = speedChangeInterval + Random.Range(-speedChangeVariation, speedChangeVariation);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void InitializeMovementBehavior()
    {
        // ID에 따른 초기 플랭킹 각도 설정 (각 그림자가 다른 방향에서 접근)
        float initialAngle = shadowID * (360f / 6f) + Random.Range(-20f, 20f);

        // 공격 움직임 패턴 설정
        if (nmAgent != null)
        {
            // 각 그림자의 성격에 따라 초기 속도와 가속도 약간 다르게 설정
            float speedVariation = Random.Range(0.8f, 1.2f);
            nmAgent.speed = chaseSpeed * speedVariation;
            nmAgent.acceleration = nmAgent.acceleration * speedVariation;

            // 경로 재계산 간격을 약간 다르게 설정
            nmAgent.avoidancePriority = Random.Range(30, 70); // 우선순위도 다르게 설정
        }

        // 그림자 움직임 패턴을 ID에 따라 다르게 설정
        StartCoroutine(ApplyMovementPattern(initialAngle));
    }

    private IEnumerator ApplyMovementPattern(float initialAngle)
    {
        // 초기 딜레이 (모든 그림자가 동시에 움직이지 않도록)
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));

        while (!isDying)
        {
            // 플레이어가 있을 때만 실행
            if (target != null && nmAgent != null && nmAgent.enabled)
            {
                // 현재 다른 그림자들과의 거리 확인
                bool tooCloseToOthers = IsCloseToOtherShadows();

                // 새로운 위치 계산 (플레이어 주변에서 분산된 위치)
                Vector3 newPosition;

                if (tooCloseToOthers)
                {
                    // 다른 그림자에서 멀어지는 방향으로 이동
                    newPosition = CalculatePositionAwayFromOthers();
                }
                else
                {
                    // 그림자 ID에 따라 플레이어 주변의 다른 위치로 접근
                    float angle = initialAngle + Time.time * 15f * (shadowID % 2 == 0 ? 1 : -1); // 시간에 따라 각도 변화, 짝수/홀수 ID로 방향 반대로
                    newPosition = CalculateFlankingPosition(angle);
                }

                // NavMesh에 유효한 위치인지 확인
                NavMeshHit hit;
                if (NavMesh.SamplePosition(newPosition, out hit, 5f, NavMesh.AllAreas))
                {
                    // 그림자마다 약간 다른 목적지 설정
                    nmAgent.SetDestination(hit.position);
                }
            }

            // 다음 위치 업데이트까지 대기
            yield return new WaitForSeconds(positionUpdateInterval + Random.Range(-0.5f, 0.5f));
        }
    }

    private bool IsCloseToOtherShadows()
    {
        // 다른 디지털 섀도우들과의 거리 확인
        DigitalShadow[] shadows = FindObjectsOfType<DigitalShadow>();
        foreach (var shadow in shadows)
        {
            if (shadow != this && !shadow.isDying)
            {
                float distance = Vector3.Distance(transform.position, shadow.transform.position);
                if (distance < minDistanceFromOthers)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3 CalculatePositionAwayFromOthers()
    {
        if (target == null) return transform.position;

        // 다른 그림자들로부터 멀어지는 방향 계산
        Vector3 awayDirection = Vector3.zero;
        int shadowCount = 0;

        DigitalShadow[] shadows = FindObjectsOfType<DigitalShadow>();
        foreach (var shadow in shadows)
        {
            if (shadow != this && !shadow.isDying)
            {
                Vector3 directionFromShadow = transform.position - shadow.transform.position;
                float distance = directionFromShadow.magnitude;

                if (distance < minDistanceFromOthers * 2f)
                {
                    // 거리가 가까울수록 더 강하게 밀어냄
                    float repulsionStrength = 1.0f - (distance / (minDistanceFromOthers * 2f));
                    awayDirection += directionFromShadow.normalized * repulsionStrength;
                    shadowCount++;
                }
            }
        }

        if (shadowCount > 0)
        {
            awayDirection /= shadowCount; // 평균 방향

            // 플레이어 방향도 고려 (플레이어에게서 너무 멀어지지 않도록)
            Vector3 playerDirection = target.position - transform.position;
            float distanceToPlayer = playerDirection.magnitude;

            // 플레이어와의 거리에 따라 가중치 조정
            float playerWeight = Mathf.Clamp01((distanceToPlayer - minDistanceFromPlayer) / (maxDistanceFromPlayer - minDistanceFromPlayer));
            Vector3 finalDirection = Vector3.Lerp(awayDirection.normalized, playerDirection.normalized, playerWeight * 0.5f);

            // 최종 위치 계산
            float targetDistance = Mathf.Lerp(minDistanceFromPlayer, maxDistanceFromPlayer, Random.value);
            return transform.position + finalDirection * targetDistance;
        }

        // 다른 그림자가 없으면 플랭킹 위치 사용
        return CalculateFlankingPosition(shadowID * 60f);
    }

    private Vector3 CalculateFlankingPosition(float angle){
        if (target == null) return transform.position;

        // 플레이어 주변의 원형 위치 계산 (측면이나 후방에서 공격하도록)
        Vector3 playerPosition = target.position;
        Vector3 forward = target.forward;

        // 플레이어 전방 벡터를 기준으로 각도 계산
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        Vector3 direction = rotation * forward;

        // 거리 랜덤화 (min~max 사이)
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
        Vector3 targetPosition = playerPosition + direction * distance;

        return targetPosition;
    }

    protected override void Start()
    {
        base.Start();

        // RangedMonster의 Start() 호출 후 추가 설정
        attackCooldown = 3f; // 더 자주 공격하도록 쿨다운 감소
        chaseSpeed = 4.5f; // 약간 더 빠르게 이동
        attackRange = 10f; // 공격 범위 조정

        // 공격 범위 및 쿨다운 랜덤화 (개체마다 약간 다르게)
        attackRange *= Random.Range(0.8f, 1.2f);
        attackCooldown *= Random.Range(0.9f, 1.1f);
        aimTime = attackCooldown * 0.6f;
        attackTime = attackCooldown * 0.8f;

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

        // 발악 패턴용 분열 조각이면 특수 효과 추가
        if (isLastStandFragment)
        {
            // 더 강한 폭발 효과 생성
            GameObject explosion = new GameObject("LastStandExplosion");
            explosion.transform.position = transform.position;

            // 추가 파티클 시스템 또는 이펙트 추가 가능
            ParticleSystem explosionPS = explosion.AddComponent<ParticleSystem>();
            var main = explosionPS.main;
            main.startColor = new Color(1.0f, 0.2f, 0.2f);
            main.startSize = 5.0f;
            main.startLifetime = 2.0f;

            // 일정 시간 후 폭발 오브젝트 제거
            Destroy(explosion, 3.0f);

            // 주변 다른 몬스터에게 데미지를 줄 수도 있음
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5.0f);
            foreach (var collider in colliders)
            {
                // 다른 몬스터에게 데미지 전달 로직
                RangedMonster monster = collider.GetComponent<RangedMonster>();
                if (monster != null && monster != this)
                {
                    monster.TakeDamage(20f, true);
                }
            }
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
                if (material.HasProperty("_FlashIntensity"))
                {
                    material.SetFloat("_FlashIntensity", 1f);
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_FlashIntensity"))
                {
                    material.SetFloat("_FlashIntensity", 0f);
                }
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

        // 발악 모드에서는 글리치 효과 기본값이 더 높음
        if (isLastStandFragment)
        {
            glitchIntensity = Mathf.Max(glitchIntensity, 0.5f);
        }

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

// 플레이어 주변의 원형 위치 계산 (측면이