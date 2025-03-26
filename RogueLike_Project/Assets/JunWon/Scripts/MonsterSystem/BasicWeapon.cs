using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeapon : MonoBehaviour
{
    [Header("무기 기본 설정")]
    [SerializeField] private string weaponName;
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private bool isCollisionEnabled = false;
    [SerializeField] private LayerMask targetLayers; // 플레이어 레이어
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioClip hitSound;

    [Header("데미지 설정")]
    [SerializeField] private float baseDamage = 10f;
    private float currentDamage;

    private Ransomware owner; // 무기의 소유자 (보스)
    private AudioSource audioSource;
    private List<GameObject> hitTargets = new List<GameObject>();

    private void Awake()
    {
        // 콜라이더 확인 또는 추가
        if (weaponCollider == null)
            weaponCollider = GetComponent<Collider>();

        if (weaponCollider != null)
            weaponCollider.isTrigger = true;

        // 오디오 소스 확인 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 초기 데미지 설정
        currentDamage = baseDamage;
    }

    private void Start()
    {
        // 소유자(보스) 찾기
        owner = GetComponentInParent<Ransomware>();

        // 디폴트 상태는 비활성화
        DisableCollision();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollisionEnabled)
            return;

        // 레이어 체크
        if (((1 << other.gameObject.layer) & targetLayers) == 0)
            return;

        // 이미 타격한 대상인지 확인
        if (hitTargets.Contains(other.gameObject))
            return;

        // 플레이어에게 데미지 전달
        PlayerStatus playerHealth = other.GetComponent<PlayerStatus>();
        if (playerHealth != null)
        {
            playerHealth.DecreaseHealth(GetDamage());
            hitTargets.Add(other.gameObject);

            // 타격 효과
            PlayHitEffects(other.ClosestPoint(transform.position));
        }
    }

    // 데미지 계산 (보스의 상태, 특수 효과 등 고려)
    public float GetDamage()
    {
        float finalDamage = currentDamage;

        // 보스가 있고, 보스의 상태에 따른 데미지 보정
        if (owner != null)
        {
            // 보스의 체력이 50% 이하일 때 데미지 증가 (2페이즈)
            if (owner.MonsterStatus.GetHealth() <= owner.MonsterStatus.GetMaxHealth() * 0.5f)
            {
                finalDamage *= 1.5f; // 2페이즈에서 50% 데미지 증가
            }

            // 보스가 특정 스킬 사용 중일 때 데미지 보정
            if (owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialAttack"))
            {
                finalDamage *= 1.3f; // 특수 공격 시 30% 데미지 증가
            }
        }

        return finalDamage;
    }

    // 데미지 설정 (외부에서 호출)
    public void SetDamage(float newDamage)
    {
        currentDamage = newDamage;
    }

    // 데미지 수정 (배율)
    public void ModifyDamage(float multiplier)
    {
        currentDamage = baseDamage * multiplier;
    }

    // 무기 충돌 활성화 (애니메이션 이벤트에서 호출)
    public void EnableCollision()
    {
        isCollisionEnabled = true;
        hitTargets.Clear();
    }

    // 무기 충돌 비활성화 (애니메이션 이벤트에서 호출)
    public void DisableCollision()
    {
        isCollisionEnabled = false;
    }

    // 타격 효과 재생
    private void PlayHitEffects(Vector3 hitPoint)
    {
        // 파티클 효과
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        // 사운드 효과
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
}
