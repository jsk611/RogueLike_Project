using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunnedStateTroy : State<Troy>
{
    public StunnedStateTroy(Troy owner) : base(owner) { }

    float stunTimer = 0;

    public override void Enter()
    {
        stunTimer = 0;
        owner.NmAgent.isStopped = true;
        owner.lurkPhase = false;
        owner.crashPhase = false;
        owner.Animator.Play("Staggered");
    }
    public override void Update()
    {
        stunTimer += Time.deltaTime;
        if (stunTimer >= owner.STUNINVTERVAL) owner.ChangeState(Troy.AnimatorState.WakeUp);
    }
    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
    }  
}
