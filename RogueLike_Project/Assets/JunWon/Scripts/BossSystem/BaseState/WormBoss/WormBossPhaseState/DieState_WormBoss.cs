using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState_WormBoss : State<WormBossPrime>
{
    public DieState_WormBoss(WormBossPrime owner) : base(owner) {}
    // Start is called before the first frame update
    public override void Enter()
    {
        owner.Animator.SetTrigger("dieTrigger");
        EventManager.Instance.TriggerMonsterKilledEvent(true);
        owner.EnemyCountData.enemyCount--;
        foreach (GameObject minion in owner.Summoned)
        {
            minion.GetComponent<MonsterBase>()?.TakeDamage(9999, false);
        }
        GameObject.Destroy(owner.gameObject);
    }
    public override void Update()
    {
        
    }
    public override void Exit()
    {
        base.Exit();
    }
}
