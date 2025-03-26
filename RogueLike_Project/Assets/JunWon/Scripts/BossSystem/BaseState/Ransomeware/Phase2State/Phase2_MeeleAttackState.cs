using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2_MeeleAttackState : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase2_MeeleAttackState(Ransomware owner) : base(owner)
    {
        owner.SetMeeleAttackState(this);
    }
    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_MeeleAttack_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("MeeleAttack2");
            if (owner.AbilityManager.UseAbility("BasicMeeleAttack"))
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
