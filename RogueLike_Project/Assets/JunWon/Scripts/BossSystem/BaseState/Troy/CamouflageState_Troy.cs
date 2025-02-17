using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CamouflageState_Troy : State<Troy>
{
    public CamouflageState_Troy(Troy owner) : base(owner) { }
    //select enemy in field
    //enforce it
    //being immune
    EnemySpawnLogic enemyManager;
    StatusBehaviour CamouflageObject;

    // Start is called before the first frame update
    public override void Enter()
    {
        if (enemyManager == null) enemyManager = GameObject.FindAnyObjectByType<EnemySpawnLogic>();
        MonsterStatus[] monsterList = enemyManager.GetComponentsInChildren<MonsterStatus>();
        CamouflageObject = monsterList[Random.Range(0, monsterList.Length)];
        owner.ISCAMOUFLAGED = true;
    }
    public override void Update()
    {
        if (CamouflageObject == null || CamouflageObject.GetHealth() <= 0)
        {
            owner.ISCAMOUFLAGED = false;
            owner.TakeDamage(owner.BossStatus.GetHealth() * 0.2f);
        }
    }
    public override void Exit()
    {

    }
}