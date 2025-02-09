using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Attack_State : BossPhaseBase<Ransomware>
{
    public Phase1_Attack_State(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("[Phase1_BasicMeeleAttack_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("MeeleAttack");
            if (owner.AbilityManger.UseAbility("BasicMeeleAttack"))
            {
            }
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    public bool IsAnimationFinished()
    {
        // 실제 애니메이션 상태를 체크하는 것이 좋습니다
        return !owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("RangedAttack") &&
               !owner.Animator.IsInTransition(0);
    }
}
