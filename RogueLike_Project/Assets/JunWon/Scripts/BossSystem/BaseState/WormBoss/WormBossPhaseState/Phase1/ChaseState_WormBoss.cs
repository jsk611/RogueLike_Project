using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState_WormBoss : BossPhaseBase<WormBossPrime>
{
    public ChaseState_WormBoss(WormBossPrime owner) : base(owner) { }

    WormBossBodyMovement wormBodyMovement = null;

    float InertiaTimer = 0f;


    public override void Enter()
    {
        if (wormBodyMovement == null) wormBodyMovement = owner.GetComponent<WormBossBodyMovement>();
        wormBodyMovement.currentActionType = WormBossBodyMovement.actionType.Flying;
    }
    public override void Update()
    { 
        if (wormBodyMovement.currentActionType == WormBossBodyMovement.actionType.Inertia)
        {
            InertiaTimer += Time.deltaTime;
            if (InertiaTimer > 2f)
                owner.ChaseToWander();
        }
    }
    public override void Exit()
    {
        InertiaTimer = 0f;
        owner.ChaseToWander();
    }
}
