using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderingStateWormBoss : State<WormBossPrime>
{
    public WanderingStateWormBoss(WormBossPrime owner) : base(owner) { }

    WormBossBodyMovement wormBodyMovement = null;
    // Start is called before the first frame update
    public override void Enter()
    {
       if(wormBodyMovement == null) wormBodyMovement = owner.GetComponent<WormBossBodyMovement>();
      //  Debug.Log("wandering state enter");
    }
    public override void Update()
    {
        //  Debug.Log("Wandering");
        wormBodyMovement.ChangeState(WormBossBodyMovement.actionType.Wandering, owner.BossStatus.GetMovementSpeed());
    }
    public override void Exit() 
    {
      //  Debug.Log("Wandering Exit");
    }
}
