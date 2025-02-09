using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigState_WormBoss : State<WormBossPrime>
{
    public DigState_WormBoss(WormBossPrime owner) : base(owner) { }
    WormBossBodyMovement wormBossBodyMovement;
    Transform wormHead;
    float thrustTimer = 0f;
    public override void Enter()
    {
        if (wormBossBodyMovement == null)
        {
            wormBossBodyMovement = owner.GetComponent<WormBossBodyMovement>();
            wormHead = wormBossBodyMovement.WormHead;
        }
        thrustTimer = 0f;
        wormBossBodyMovement.ChangeState(WormBossBodyMovement.actionType.Digging, owner.BossStatus.GetMovementSpeed());
    }
    public override void Update()
    {
        thrustTimer += Time.deltaTime;
        if(Physics.Raycast(wormHead.position,Vector3.up,80,LayerMask.GetMask("Character")) || thrustTimer >=4f)
        {
            wormBossBodyMovement.ChangeState(WormBossBodyMovement.actionType.Flying, owner.BossStatus.GetMovementSpeed()*2);
        }
 
        if (wormHead.position.y >= 20f) owner.DigToWander();
    }
    public override void Exit()
    {
        owner.DigToWander();
    }
}
