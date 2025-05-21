using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

using UnityEngine;

public class Phase1_BasicRangedAttack_State : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase1_BasicRangedAttack_State(Ransomware owner) : base(owner) {
        owner.SetRangedAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("[Phase1_BasicRangedAttack_State] Enter");
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("RangedAttack");
            owner.AbilityManager.UseAbility("BasicRangedAttack");
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true; // 공격 불가능할 경우 바로 상태 전환
        }
    }

   

    public void FireProjectile()
    {
        Vector3 firePos = owner.FirePoint.position;
        Vector3 directionToPlayer = (owner.Player.position - firePos).normalized;

        GameObject projectile = GameObject.Instantiate(
            owner.DataPacket,
            firePos,
            Quaternion.LookRotation(directionToPlayer)
        );

        if (projectile.TryGetComponent<MProjectile>(out var mProjectile))
        {
            mProjectile.SetBulletDamage(owner.AbilityManager.GetAbiltiyDmg("BasicRangedAttack"));
            mProjectile.SetDirection(directionToPlayer);
            Debug.Log("Ranged attack projectile fired!");
        }
    }


    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RangedAttack"); 
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null &&
               owner.DataPacket != null &&
               owner.FirePoint != null;
    }

    // 애니메이션 이벤트에서 호출될 메서드
    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
