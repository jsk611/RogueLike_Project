using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State<Ransomware>
{
    public IdleState(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
    }

    public override void Update()
    {
        // 플레이어를 감지하면 ChaseState로 전환
        if (true)
        {
        }
    }

    public override void Exit()
    {
        Debug.Log("Idle State Exit");
    }
}
