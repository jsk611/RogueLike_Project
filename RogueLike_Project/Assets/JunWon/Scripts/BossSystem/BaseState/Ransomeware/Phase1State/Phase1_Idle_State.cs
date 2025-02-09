using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Idle_State : BossPhaseBase<Ransomware>
{
    public Phase1_Idle_State(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
        owner.Animator.SetTrigger("Idle");
        owner.NmAgent.isStopped = true;
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
    }
}
