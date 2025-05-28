using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrowableWeapon : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private LayerMask targetLayers = -1; // 모든 레이어 기본값

    [Header("Visual & Audio")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float cameraShakeIntensity = 0.5f;
    [SerializeField] private float cameraShakeDuration = 0.3f;

    [Header("Advanced Settings")]
    [SerializeField] private bool destroyOnImpact = true;
    [SerializeField] private bool applyKnockback = true;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0.3f);
    [SerializeField] private float minDamagePercent = 0.2f;

    private bool hasExploded = false;
    private Rigidbody rb;
    private AudioSource audioSource;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // 오디오 소스가 없으면 생성
        if (audioSource == null && explosionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥이나 벽에 충돌 시 폭발
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Boom!");
            Explode(collision.contacts[0].point);
        }
    }

    // 외부에서 폭발을 트리거할 수 있는 메서드
    public void TriggerExplosion()
    {
        Explode(transform.position);
    }

    private void Explode(Vector3 explosionPoint)
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"Explosion at {explosionPoint} with radius {explosionRadius}");

        // 1. 폭발 범위 내 모든 콜라이더 탐지
        Collider[] hitColliders = Physics.OverlapSphere(explosionPoint, explosionRadius, targetLayers);

        List<GameObject> affectedObjects = new List<GameObject>();

        foreach (Collider hitCollider in hitColliders)
        {
            // 자기 자신은 무시
            if (hitCollider.gameObject == gameObject) continue;

            // 중복 처리 방지 (같은 오브젝트의 여러 콜라이더)
            if (affectedObjects.Contains(hitCollider.gameObject)) continue;
            affectedObjects.Add(hitCollider.gameObject);

            // 폭발 지점으로부터의 거리 계산
            float distance = Vector3.Distance(explosionPoint, hitCollider.transform.position);
            float normalizedDistance = Mathf.Clamp01(distance / explosionRadius);

            // 거리에 따른 데미지 감소 적용
            float damageMultiplier = damageFalloff.Evaluate(normalizedDistance);
            damageMultiplier = Mathf.Max(damageMultiplier, minDamagePercent);

            float finalDamage = explosionDamage * damageMultiplier;

            // 2. 플레이어 데미지 처리
            PlayerStatus playerStatus = hitCollider.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.DecreaseHealth(finalDamage);
                Debug.Log($"Player takes {finalDamage} explosion damage");
            }

            // 3. 다른 몬스터들에게도 데미지 (팀킬 가능)
            //MonsterBase monster = hitCollider.GetComponent<MonsterBase>();
            //if (monster != null)
            //{
            //    monster.TakeDamage(finalDamage * 0.5f, true); // 몬스터는 절반 데미지
            //    Debug.Log($"Monster {monster.name} takes {finalDamage * 0.5f} explosion damage");
            //}

            // 4. 넉백 효과 적용
            if (applyKnockback)
            {
                ApplyKnockback(hitCollider, explosionPoint, normalizedDistance);
            }

            // 5. 파괴 가능한 오브젝트 처리
            //DestructibleObject destructible = hitCollider.GetComponent<DestructibleObject>();
            //if (destructible != null)
            //{
            //    destructible.TakeDamage(finalDamage);
            //}
        }

        // 6. 시각적 효과
        CreateExplosionEffect(explosionPoint);

        // 7. 오디오 효과
        PlayExplosionSound();

        // 8. 카메라 쉐이크 (있다면)
        TriggerCameraShake();

        // 9. 폭탄 오브젝트 제거
        if (destroyOnImpact)
        {
            StartCoroutine(DestroyAfterEffect());
        }

    }

    private void ApplyKnockback(Collider target, Vector3 explosionPoint, float normalizedDistance)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // 폭발 지점에서 타겟으로의 방향 계산
            Vector3 knockbackDirection = (target.transform.position - explosionPoint).normalized;

            // Y축 성분을 약간 추가해서 위로 튀어오르는 효과
            knockbackDirection.y = Mathf.Max(knockbackDirection.y, 0.3f);

            // 거리에 따른 힘 감소
            float knockbackForce = explosionForce * (1f - normalizedDistance);

            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

        // 플레이어 특수 처리 (에어본 효과 등)
        PlayerControl playerControl = target.GetComponent<PlayerControl>();
        if (playerControl != null)
        {
            Vector3 airborneDirection = (target.transform.position - explosionPoint).normalized;
            StartCoroutine(playerControl.AirBorne(airborneDirection));
        }
    }

    private void CreateExplosionEffect(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab, position, Quaternion.identity);

            // 이펙트 스케일 조정 (폭발 반경에 따라)
            float scale = explosionRadius / 5f; // 기본 반경 5를 기준으로 스케일 조정
            effect.transform.localScale = Vector3.one * scale;

            // 이펙트 자동 제거
            Destroy(effect, 3f);
        }
    }

    private void PlayExplosionSound()
    {
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }

    private void TriggerCameraShake()
    {
        // 카메라 쉐이크 시스템이 있다면 호출
        // CameraShakeManager.Instance?.Shake(cameraShakeIntensity, cameraShakeDuration);

        // 또는 이벤트로 처리
        // EventManager.Instance?.TriggerCameraShake(cameraShakeIntensity, cameraShakeDuration);
    }

    private IEnumerator DestroyAfterEffect()
    {
        // 폭발 이펙트와 사운드가 재생될 시간을 기다림
        yield return new WaitForSeconds(0.1f);

        // 렌더러 비활성화 (시각적으로 사라짐)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // 콜라이더 비활성화 (추가 충돌 방지)
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // 사운드 재생 완료 후 완전 제거
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // 디버그용 기즈모 그리기 (에디터에서만)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius * 0.5f);
    }

    // 런타임에서 폭발 범위 설정
    public void SetExplosionRadius(float radius)
    {
        explosionRadius = radius;
    }

    public void SetExplosionDamage(float damage)
    {
        explosionDamage = damage;
    }

    public void SetExplosionForce(float force)
    {
        explosionForce = force;
    }
}
