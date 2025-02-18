using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState_WormBoss : State<WormBossPrime>
{
    public DieState_WormBoss(WormBossPrime owner) : base(owner) {}
    WormBossBodyMovement wormBossBodyMovement;

    float deadTimer = 0f;
    bool deadCounted = false;
    // Start is called before the first frame update
    public override void Enter()
    {
        wormBossBodyMovement = owner.GetComponent<WormBossBodyMovement>();
        wormBossBodyMovement.ChangeState(WormBossBodyMovement.actionType.Dying, owner.BossStatus.GetMovementSpeed()/2);
  
        owner.Summoned.RemoveAll(x => x == null);
    }
    public override void Update()
    {
        deadTimer += Time.deltaTime;
        if(deadTimer>=6f) { 
            foreach (GameObject minion in owner.Summoned)
            {
                minion?.GetComponent<MonsterBase>().TakeDamage(9999, false);
            }

            if(!deadCounted)
            {
                deadCounted = true;
                owner.EnemyCountData.enemyCount--;
            }
            GameObject.Destroy(owner.gameObject,0.2f);
        }
    }
    public override void Exit()
    {
    }
}
