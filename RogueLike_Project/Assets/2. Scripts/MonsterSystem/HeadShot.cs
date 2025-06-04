using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class HeadShot : MonoBehaviour
{

    [SerializeField] MonsterBase monsterBase;
    [SerializeField] private AudioClip hitSound; // 피격 사운드

    [SerializeField] float criticalDamage = 2f;
    private PlayerStatus playerStatus;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeComponents()
    {
        

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
            float totalDamage = bulletDamage * playerStatus.GetAttackDamage() / 100 * criticalDamage;
            ApplyDamage(totalDamage);
        }
    }
    private void ApplyDamage(float damage)
    {
        // 몹 체력 감소
        monsterBase.TakeDamage(damage);

        // 이벤트 트리거
        EventManager.Instance.TriggerMonsterCriticalDamagedEvent();

    }
}
