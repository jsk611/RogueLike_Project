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
        Debug.Log("wandering state enter");
    }
    public override void Update()
    {
        wormBodyMovement.currentActionType = WormBossBodyMovement.actionType.Wandering;
    }
    public override void Exit() 
    { 
    }
}
