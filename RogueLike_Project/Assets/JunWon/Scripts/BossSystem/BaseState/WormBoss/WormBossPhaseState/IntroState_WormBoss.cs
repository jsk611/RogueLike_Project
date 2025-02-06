using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroState_WormBoss : IntroState<WormBossPrime>
{
    public IntroState_WormBoss(WormBossPrime owner) : base(owner) { }
    public override void Enter()
    {
      //  Debug.Log("IntroState Wormboss Start");
    }
    public override void Update()
    {
      //  Debug.Log("IntroState Wormboss Update");
    }
    public override void Exit()
    {
      //  Debug.Log("IntroState Wormboss Exit");
    }
}
