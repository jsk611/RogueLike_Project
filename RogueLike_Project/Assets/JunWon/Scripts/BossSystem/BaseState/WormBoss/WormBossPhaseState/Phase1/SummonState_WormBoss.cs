using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonState_WormBoss : State<WormBossPrime>
{
    public SummonState_WormBoss(WormBossPrime owner) : base(owner) { }
    // Start is called before the first frame update
    public override void Enter()
    {
        owner.summonTimer = 0;
        int range = Random.Range(0, owner.minions.Count);
        GameObject minion = GameObject.Instantiate(owner.minions[range], owner.transform.position, Quaternion.identity);
        owner.Summoned.Add(minion);
        Debug.Log("Summon State Enter");
    }
    public override void Update()
    {
        owner.minions.RemoveAll(item => item == null);
        Debug.Log("Summon State Update");
    }
    public override void Exit()
    {
        Debug.Log("Summon State Exit");
    }
}
