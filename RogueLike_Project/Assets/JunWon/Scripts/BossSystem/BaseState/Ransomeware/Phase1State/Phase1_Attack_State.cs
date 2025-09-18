using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_Attack_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase1_Attack_State(Ransomware owner) : base(owner) 
    { 
    }

    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase1_MeleeAttack_State] Enter");
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
        owner.Animator.ResetTrigger("MeleeAttack");
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
