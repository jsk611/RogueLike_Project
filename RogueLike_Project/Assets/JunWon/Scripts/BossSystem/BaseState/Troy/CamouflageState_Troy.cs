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
    float summonEnemyCount;


    float bombThrowInterval = 2f;
    float bombThrowTimer = 0f;

    // Start is called before the first frame update
    public override void Enter()
    {
        if (enemyManager == null) enemyManager = GameObject.FindAnyObjectByType<EnemySpawnLogic>();
        StatusBehaviour[] monsterList = enemyManager.GetComponentsInChildren<StatusBehaviour>();
        if (monsterList.Length > 0)
            CamouflageObject = monsterList[Random.Range(0, monsterList.Length)];
        else
        {
            summonEnemyCount = owner.SUMMONAMOUNT;
            while (summonEnemyCount > 0)
            {
                summonEnemyCount--;
                Vector3 randomPos = owner.transform.position + new Vector3(Random.Range(-4f,4f),0,Random.Range(-4f,4f));
                GameObject enemy = GameObject.Instantiate(enemyManager.GetEnemyPrefab(EnemyType.MeeleeSoldier), randomPos, Quaternion.identity,enemyManager.transform);
                enemy.GetComponent<MonsterBase>().summonedMonster = true;
                owner.SUMMONEDMONSTERS.Add(enemy);
            }
        }
        CamouflageObject = monsterList[Random.Range(0, monsterList.Length)];

        owner.IdleToCamouflage();
    }
    public override void Update()
    {
        bombThrowTimer += Time.deltaTime;
        if (bombThrowTimer >= bombThrowInterval)
        {
            Debug.Log(bombThrowInterval);
            bombThrowTimer = 0f;
            GameObject.Instantiate(owner.TROYBOMB, owner.Player.position+Vector3.up, owner.transform.rotation).GetComponent<Rigidbody>().velocity = Vector3.up*3;
        }
        if (CamouflageObject == null)
        {
            owner.IdleToCamouflage();
            return;
        }

        else if (CamouflageObject == null || CamouflageObject.GetHealth() <= 0)
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