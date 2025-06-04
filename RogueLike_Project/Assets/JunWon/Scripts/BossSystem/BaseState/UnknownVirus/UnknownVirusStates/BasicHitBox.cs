using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHitBox : MonoBehaviour
{
    [Header("히트박스 설정")]
    [SerializeField] private BossStatus bossStatus; // 보스 체력 상태 참조
    [SerializeField] private string partName = "Body"; // 이 히트박스가 담당하는 부위 이름
    [SerializeField] private float damageMultiplier = 1.0f; // 부위별 데미지 배율
    [SerializeField] private bool autoFindBossStatus = true; // 자동으로 보스 상태 찾기

    [Header("시각적 피드백")]
    [SerializeField] private GameObject hitEffect; // 피격 이펙트
    [SerializeField] private AudioClip hitSound; // 피격 사운드

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    private PlayerStatus playerStatus;
    private AudioSource audioSource;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // BossStatus 자동 탐지
        if (autoFindBossStatus && bossStatus == null)
        {
            // 부모 오브젝트에서 BossStatus 찾기
            bossStatus = GetComponentInParent<BossStatus>();

            // 부모에서 못 찾으면 같은 오브젝트에서 찾기
            if (bossStatus == null)
            {
                bossStatus = GetComponent<BossStatus>();
            }

            // 그래도 못 찾으면 씬에서 찾기 (마지막 수단)
            if (bossStatus == null)
            {
                var bosses = FindObjectsOfType<BossStatus>();
                if (bosses.Length > 0)
                {
                    bossStatus = bosses[0]; // 첫 번째 보스 사용
                    Debug.LogWarning($"[BossPartHitBox] {gameObject.name}에서 여러 보스 발견, 첫 번째 보스 사용: {bossStatus.name}");
                }
            }

            if (bossStatus == null)
            {
                Debug.LogError($"[BossPartHitBox] {gameObject.name}에서 BossStatus를 찾을 수 없습니다!");
            }
        }

        // 플레이어 상태 가져오기
        playerStatus = ServiceLocator.Current.Get<IGameModeService>()
            .GetPlayerCharacter().GetComponent<PlayerStatus>();

        // 오디오 소스 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Debug.Log("Hit by " + other.gameObject.name);
            float bulletDamage = other.gameObject.GetComponent<Projectile>().bulletDamage;
            float totalDamage = bulletDamage * playerStatus.GetAttackDamage() / 100 * playerStatus.CalculateCriticalHit();
            ApplyDamage(totalDamage, other.transform.position);
        }
    }

    private void ApplyDamage(float damage, Vector3 hitPosition)
    {
        // 보스 체력 감소
        bossStatus.DecreaseHealth(damage);

        // 시각적/청각적 피드백
        PlayHitFeedback(hitPosition);

        // 이벤트 트리거
        EventManager.Instance.TriggerMonsterDamagedEvent();

        // 디버그 정보
        if (showDebugInfo)
        {
            Debug.Log($"[BossPartHitBox] {partName} 부위에 {damage:F1} 데미지! " +
                     $"배율: {damageMultiplier}x, 남은 체력: {bossStatus.GetHealth():F1}/{bossStatus.GetMaxHealth():F1}");
        }
    }

    /// <summary>
    /// 피격 피드백 재생
    /// </summary>
    private void PlayHitFeedback(Vector3 hitPosition)
    {
        // 이펙트 재생
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 사운드 재생
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 부위별 특별 피드백
        PlayPartSpecificFeedback();
    }

    /// <summary>
    /// 부위별 특별한 피드백
    /// </summary>
    private void PlayPartSpecificFeedback()
    {
        switch (partName.ToLower())
        {
            case "head":
                // 헤드샷 특별 이펙트
                Debug.Log($"💥 헤드샷! {damageMultiplier}x 데미지!");
                break;
            case "weakpoint":
            case "core":
                // 약점 공격 특별 이펙트
                Debug.Log($"🎯 약점 공격! {damageMultiplier}x 데미지!");
                break;
            case "shield":
            case "armor":
                // 방어구 공격 특별 이펙트
                Debug.Log($"🛡️ 방어구 피격! {damageMultiplier}x 데미지!");
                break;
        }
    }

    /// <summary>
    /// 런타임에 데미지 배율 변경
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    /// <summary>
    /// 런타임에 부위 이름 변경
    /// </summary>
    public void SetPartName(string newPartName)
    {
        partName = newPartName;
    }

    /// <summary>
    /// 히트박스 활성화/비활성화
    /// </summary>
    public void SetHitBoxActive(bool active)
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = active;
        }
    }
}

