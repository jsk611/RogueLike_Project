using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState_Troy : State<Troy>
{
    public RunState_Troy(Troy owner) : base(owner)
    {
    }
    public override void Enter()
    {
        Debug.Log("RUNNN");
    }
    public override void Update()
    {
        Vector3 dir = owner.transform.position - owner.Player.position;
        owner.NmAgent.SetDestination(owner.transform.position + dir);
    }
    public override void Exit()
    {
    }
}

