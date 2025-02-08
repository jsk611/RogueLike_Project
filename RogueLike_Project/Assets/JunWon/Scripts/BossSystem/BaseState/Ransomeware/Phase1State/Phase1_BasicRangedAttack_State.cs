using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Phase1_BasicRangedAttack_State : State<Ransomware>
{
    public Phase1_BasicRangedAttack_State(Ransomware owner) : base(owner) { }

    Vector3 playerPos;
    Vector3 firePos;
    Quaternion fireRot;
    GameObject packet;
    public override void Enter()
    {
        playerPos = owner.Player.transform.position;
        firePos = owner.FirePoint.transform.position;
        fireRot = owner.FirePoint.transform.rotation;
        packet = owner.DataPacket;
        Debug.Log("[Phase1_BasicRangedAttack_State] Enter");
        owner.NmAgent.isStopped = true;

        if (owner.AbilityManger.UseAbility("BasicRangedAttack"))
        {
            FireProjectile();
        }
    }

    public override void Update()
    {

    }

    void FireProjectile()
    {
        if (playerPos != null && packet != null && firePos != null)
        {
            Vector3 directionToPlayer = (playerPos - firePos).normalized;
            GameObject projectile = GameObject.Instantiate(owner.DataPacket, firePos, fireRot);
            projectile.GetComponent<MProjectile>().SetBulletDamage(owner.AbilityManger.GetAbiltiyDmg("BasicRangedAttack")); // 몬스터 데이터에서 데미지 값 가져오도록 수정 (예시)
            projectile.GetComponent<MProjectile>().SetDirection(directionToPlayer);
            Debug.Log("원거리 구체 발사!");
        }   
        else
        {
            Debug.LogWarning("구체 발사에 필요한 프리팹 또는 발사 지점이 설정되지 않음.");
        }
    }

    public bool IsAnimationFinished()
    {
        // 예시: 애니메이터의 현재 애니메이션 상태가 "Attack" 애니메이션이 아니고, 전환 중이 아닐 때
        return true;
        //return !owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialAttack") && !owner.Animator.IsInTransition(0);
    }
}
