using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState_WormBoss : BossPhaseBase<WormBossPrime>
{
    public ChaseState_WormBoss(WormBossPrime owner) : base(owner) { }

    WormBossBodyMovement wormBodyMovement = null;

    float InertiaTimer = 0f;
    float resetStateTimer = 0f;


    public override void Enter()
    {
        if (wormBodyMovement == null) wormBodyMovement = owner.GetComponent<WormBossBodyMovement>();
        wormBodyMovement.ChangeState(WormBossBodyMovement.actionType.Flying,owner.BossStatus.GetMovementSpeed());
    }
    public override void Update()
    {
        resetStateTimer += Time.deltaTime;
        if(wormBodyMovement.WormHead.position.y >= 50)
        {
            wormBodyMovement.MoveDirection = Quaternion.LookRotation(owner.Player.position - wormBodyMovement.WormHead.position);
            wormBodyMovement.ChangeState(WormBossBodyMovement.actionType.Rushing,owner.BossStatus.GetMovementSpeed()*2);
        }
        if (wormBodyMovement.currentActionType == WormBossBodyMovement.actionType.Inertia || resetStateTimer >= 9f)
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
