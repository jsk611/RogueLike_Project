using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState_WormBoss : BossPhaseBase<WormBossPrime>
{
    public ChaseState_WormBoss(WormBossPrime owner) : base(owner) { }

    WormBossBodyMovement wormBodyMovement = null;
    public override void Enter()
    {
        if (wormBodyMovement == null) wormBodyMovement = owner.GetComponent<WormBossBodyMovement>();
       // Debug.Log("ChaseState Wormboss Start");
    }
    public override void Update()
    {
        wormBodyMovement.currentActionType = WormBossBodyMovement.actionType.Rushing;
     //   Debug.Log("ChaseState Wormboss Update");
    }
    public override void Exit()
    {
      //  Debug.Log("ChaseState Wormboss Exit");
    }
}
