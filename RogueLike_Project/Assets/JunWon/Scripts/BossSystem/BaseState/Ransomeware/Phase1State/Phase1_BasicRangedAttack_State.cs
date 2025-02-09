using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Timeline.Actions;
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
            // 애니메이션 재생
            owner.Animator.SetTrigger("RangedAttack");

            // Ability 시스템을 통한 공격 실행
            if (owner.AbilityManger.UseAbility("BasicRangedAttack"))
            {
                FireProjectile();
            }
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true; // 공격 불가능할 경우 바로 상태 전환
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null &&
               owner.DataPacket != null &&
               owner.FirePoint != null;
    }

    private void FireProjectile()
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
            mProjectile.SetBulletDamage(owner.AbilityManger.GetAbiltiyDmg("BasicRangedAttack"));
            mProjectile.SetDirection(directionToPlayer);
            Debug.Log("Ranged attack projectile fired!");
        }
    }

    // 애니메이션 이벤트에서 호출될 메서드
    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RangedAttack"); 
        isAttackFinished = false;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
