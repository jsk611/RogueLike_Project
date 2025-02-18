using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState_WormBoss : State<WormBossPrime>
{
    public DieState_WormBoss(WormBossPrime owner) : base(owner) {}
    WormBossBodyMovement wormBossBodyMovement;

    float deadTimer = 0f;
    // Start is called before the first frame update
    public override void Enter()
    {
        wormBossBodyMovement = owner.GetComponent<WormBossBodyMovement>();
        wormBossBodyMovement.ChangeState(WormBossBodyMovement.actionType.Dying, owner.BossStatus.GetMovementSpeed()/2);

    }
    public override void Update()
    {
        deadTimer += Time.deltaTime;
        if (deadTimer >= 6f)
        {
            EventManager.Instance.TriggerMonsterKilledEvent(true);
            owner.EnemyCountData.enemyCount--;
            foreach (GameObject minion in owner.Summoned)
            {
                minion.GetComponent<MonsterBase>()?.TakeDamage(9999, false);
            }
            GameObject.Destroy(GameObject.FindObjectOfType<WormBossPrime>());
        }
            
    }
    public override void Exit()
    {
    }
}
