using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2_MeleeAttackState : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase2_MeleeAttackState(Ransomware owner) : base(owner)
    {
        owner.SetMeleeAttackState(this);
    }
    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_MeleeAttack_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("MeleeAttack");
            if (owner.AbilityManager.UseAbility("BasicMeleeAttack"))
            {
            }
        }
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("MeeleAttack");
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
