using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Phase1_SpeacialAttack_State : State<Ransomware>
{
    private string name = "DataExplode";
    private bool isAttackFinished = false;
    public float radius = 15.0f;
    public float damage = 50f;
    public LayerMask playerLayer; // Inspector에서 Player 레이어만 선택

    public Phase1_SpeacialAttack_State(Ransomware owner) : base(owner) {
        owner.SetSpecialAttackState(this);
        playerLayer = LayerMask.GetMask("Character");
    }


    public override void Enter()
    {
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            // Ability 시스템을 통한 공격 실행
            if (owner.AbilityManager.UseAbility(name))
            {
                owner.Animator.SetTrigger(name);
                LockPlayerSkill();
            }
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true; // 공격 불가능할 경우 바로 상태 전환
        }
    }

    public override void Update()
    {
     
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("DataExplode");

    }

   

    public void ExplodeData()
    {
        // Player 레이어에 있는 오브젝트만 검출
        Collider[] hits = Physics.OverlapSphere(owner.transform.position, radius, playerLayer);
        Debug.Log("Player 적중 " + hits.Length);

        foreach (Collider hit in hits)
        {
            PlayerStatus playerHealth = hit.GetComponent<PlayerStatus>();
            if (playerHealth != null)
            {
                // 거리에 따른 데미지 계산
                float distance = Vector3.Distance(owner.transform.position, hit.transform.position);
                float damageAmount = damage * (1 - (distance / radius));

                playerHealth.DecreaseHealth(damageAmount);
            }
        }

        // 시각적 효과는 모든 오브젝트에 적용 가능
        ShowExplosionEffect();
    }

    void ShowExplosionEffect()
    {
        GameObject explosion = owner.AbilityManager.GetAbilityPrefab(name);
        // 여기에 파티클 효과, 사운드 등 추가
        if (explosion == null)
        {
            Debug.LogWarning("DataExplode 이펙트 프리팹이 없습니다.");
            return;
        }

        // 보스의 현재 위치에서 폭발 이펙트 생성
        GameObject effectInstance = Object.Instantiate(explosion, owner.ExplosionPoint.position, Quaternion.identity);

        // AdvancedExplosion 등 커스텀 스크립트가 붙어 있다면, 파라미터 세팅
        Explosion explosionScript = effectInstance.GetComponent<Explosion>();
        if (explosionScript != null)
        {
        }
    }

    void LockPlayerSkill()
    {
        Character player = owner.Player.GetComponent<Character>();
        if (player != null)
        {
            Debug.Log("IsWeaponExchangeLocked");
            player.IsCursorLocked();
            player.LockChangedWeapon();
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }


    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
