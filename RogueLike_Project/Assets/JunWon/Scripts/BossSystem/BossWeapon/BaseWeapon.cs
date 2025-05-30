using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseWeapon : MonoBehaviour, IWeapon
{
    [Header("무기 기본 설정")]
    [SerializeField] protected string weaponName;
    [SerializeField] protected Collider weaponCollider;
    [SerializeField] protected bool isCollisionEnabled = false;
    [SerializeField] protected LayerMask targetLayers; // 플레이어 레이어
    [SerializeField] protected ParticleSystem hitEffect;
    [SerializeField] protected AudioClip hitSound;

    [Header("데미지 설정")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float damageMultiplier = 1f;

    [Header("효과 설정")]

    protected float currentDamage;
    protected IBossEntity bossOwner;
    protected AudioSource audioSource;
    [SerializeField] protected List<GameObject> hitTargets = new List<GameObject>();

    protected virtual void Awake()
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

    protected virtual void Start()
    {
        // 상위 계층에서 보스 찾기
        FindBossOwner();

        // 데미지 초기화
        UpdateDamageFromSource();

        // 기본 상태는 비활성화
        DisableCollision();
    }

    protected virtual void FindBossOwner()
    {
        // 부모 오브젝트에서 IBossEntity 인터페이스 구현체 찾기
        Transform current = transform.parent;

        while (current != null)
        {
            IBossEntity boss = current.GetComponent<IBossEntity>();
            if (boss != null)
            {
                bossOwner = boss;
                break;
            }
            current = current.parent;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("Meele Attack is activated");
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
            playerHealth.DecreaseHealth(CalculateDamage());
            hitTargets.Add(other.gameObject);

            // 타격 효과
            ApplyHitEffect(other.ClosestPoint(transform.position), other.gameObject);

            // 추가 효과 적용
            ApplyWeaponEffects(other.gameObject, other.ClosestPoint(transform.position));
        }
    }

    public virtual void EnableCollision()
    {
        isCollisionEnabled = true;
        hitTargets.Clear();
    }

    public virtual void DisableCollision()
    {
        isCollisionEnabled = false;
    }

    public virtual void SetDamage(float damage)
    {
        currentDamage = damage;
    }

    public virtual void UpdateDamageFromSource()
    {
        if (bossOwner != null)
        {
            // 보스로부터 기본 데미지 가져오기
            baseDamage = bossOwner.GetBaseDamage();

            // 보스의 현재 페이즈/상태에 따른 멀티플라이어 적용
            //damageMultiplier = bossOwner.GetDamageMultiplier();

            // 최종 데미지 계산
            currentDamage = baseDamage * damageMultiplier;
        }
    }

    protected virtual float CalculateDamage()
    {
        float finalDamage = currentDamage;

        // 보스가 특수 상태면 추가 데미지 보정
        if (bossOwner != null && bossOwner.IsInSpecialState())
        {
            finalDamage *= 1.3f;
        }

        return finalDamage;
    }

    public virtual void ApplyHitEffect(Vector3 hitPoint, GameObject target)
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

    protected virtual void ApplyWeaponEffects(GameObject target, Vector3 hitPoint)
    {
        //foreach (WeaponEffect effect in weaponEffects)
        //{
        //    effect.ApplyEffect(target, hitPoint);
        //}
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    // 디버깅용 기즈모
    protected virtual void OnDrawGizmos()
    {
        if (weaponCollider != null && isCollisionEnabled)
        {
            Gizmos.color = Color.red;

            if (weaponCollider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (weaponCollider is CapsuleCollider capsuleCollider)
            {
                // 캡슐 콜라이더 시각화
                // (간략화를 위해 생략)
            }
        }
    }



}