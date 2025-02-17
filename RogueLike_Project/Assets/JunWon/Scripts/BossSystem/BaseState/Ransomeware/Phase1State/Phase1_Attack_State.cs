using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Attack_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase1_Attack_State(Ransomware owner) : base(owner) { 
        owner.SetMeeleAttackState(this);
    }

    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase1_BasicMeeleAttack_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("MeeleAttack");
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
