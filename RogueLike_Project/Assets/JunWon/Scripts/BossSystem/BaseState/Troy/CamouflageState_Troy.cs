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
        StatusBehaviour[] monsterList = enemyManager.GetComponentsInChildren<StatusBehaviour>();
        CamouflageObject = (monsterList.Length>0)? monsterList[Random.Range(0, monsterList.Length)] : null;
        owner.IdleToCamouflage();
    }
    public override void Update()
    {

        if (CamouflageObject == null)
        {
            owner.IdleToCamouflage();
            return;
        }

        else if (CamouflageObject.GetHealth() <= 0)
        {
            owner.IdleToCamouflage();
            owner.TakeDamage(owner.BossStatus.GetHealth() * 0.2f);
        }
        Debug.Log(CamouflageObject.name);
    }
    public override void Exit()
    {

    }
}