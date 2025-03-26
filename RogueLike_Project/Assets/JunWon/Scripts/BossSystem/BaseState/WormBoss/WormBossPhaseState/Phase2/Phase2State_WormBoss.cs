using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2State_WormBoss : BossPhaseBase<WormBossPrime>
{
    public Phase2State_WormBoss(WormBossPrime owner) : base(owner) { }

    Transform paritition1, partition2;
    // Start is called before the first frame update
    public override void Enter()
    {
        EnemySpawnLogic logic = EnemySpawnLogic.instance;
        GameObject prefab = logic.GetEnemyPrefab(EnemyType.Wormboss);
        Debug.Log(prefab);

        List<Transform> bodyList = owner.WormBossBodyMovement.BodyList;
        partition2 = bodyList[bodyList.Count / 2];
        GameObject subWorm = GameObject.Instantiate(prefab,partition2.position,partition2.rotation,null);
        WormBossPrime subWormPrime = subWorm.GetComponent<WormBossPrime>();
        for (int i = 0; i < bodyList.Count / 2; i++)
        {
            Transform bodyPart = subWormPrime.WormBossBodyMovement.BodyList[i];
            bodyPart.position = bodyList[i].position;
            bodyPart.rotation = bodyList[i].rotation;
        }
        for (int i = bodyList.Count / 2; i < bodyList.Count; i++)
            subWormPrime.WormBossBodyMovement.BodyList[i].gameObject.SetActive(false);

        Debug.Log("origin dead");
    }
    public override void Update()
    {
    }
    public override void Exit() { base.Exit(); }
}
