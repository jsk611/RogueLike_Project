using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState_Troy : State<Troy>
{
    public DieState_Troy(Troy owner) : base(owner) { }

    public override void Enter()
    {
        base.Enter();

        GameObject.Destroy(owner);
    }
    public override void Update()
    {
        base.Update();
    }
    public override void Exit()
    {
        base.Exit();
    }
}
