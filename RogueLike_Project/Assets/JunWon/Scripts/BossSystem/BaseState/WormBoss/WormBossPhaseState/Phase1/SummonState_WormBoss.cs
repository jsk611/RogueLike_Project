using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonState_WormBoss : State<WormBossPrime>
{
    public SummonState_WormBoss(WormBossPrime owner) : base(owner) { }
    // Start is called before the first frame update
    public override void Enter()
    {
        int range = Random.Range(0, owner.minions.Count);
        Vector3 randomPosition = new Vector3(Random.Range(0,5),0,Random.Range(0,5));
        //  GameObject minion = GameObject.Instantiate(owner.minions[range], owner.Player.position+randomPosition, Quaternion.identity);
        GameObject minion = EnemySpawnLogic.instance.SpawnEnemy(TileManager.mapSize / 2, TileManager.mapSize / 2, EnemyType.Hoverbot);
        owner.Summoned.Add(minion);
        minion.GetComponent<MonsterBase>().summonedMonster = true;
     //   Debug.Log("Summon State Enter");
    }
    public override void Update()
    {
        owner.minions.RemoveAll(item => item == null);
        owner.SummonToWander();
      //  Debug.Log("Summon State Update"); 
    }
    public override void Exit()
    {
        owner.SummonToWander();
     //   Debug.Log("Summon State Exit");
    }
}
