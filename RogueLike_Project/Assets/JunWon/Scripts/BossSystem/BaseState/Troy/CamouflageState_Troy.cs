using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    float rushTimer = 0f;
    bool isRushing = false;
    float rushInterval = 5f;
    float minRushDist = 10f;

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
             //   GameObject enemy = GameObject.Instantiate(owner.gameObject, randomPos, Quaternion.identity, enemyManager.transform);
             //   enemy.transform.localScale = owner.transform.localScale * 0.5f;
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
        rushTimer += Time.deltaTime;

        if(rushTimer >= rushInterval)
        {
            if (!isRushing)
            {
                isRushing = true;
                owner.CoroutineRunner(RushToPlayer());
            }

        }
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
        owner.NmAgent.SetDestination(owner.transform.position);
    }
    void OnCollisionEnter(Collision collision)
    {
        if(isRushing && collision.gameObject == owner.Player.gameObject)
        {
            Debug.Log("collided to player");
        }
    }
    IEnumerator RushToPlayer()
    {
        Collider[] checkPlayer;
        BoxCollider ownerCollider = owner.GetComponent<BoxCollider>();
        float elapsedTime = 0f;
        while(isRushing && elapsedTime <= 2f)
        { 
            Vector3 rushDirection = owner.Player.position;
            checkPlayer = Physics.OverlapBox(owner.transform.position+ new Vector3(0, 2f, 0.3f), ownerCollider.size*1.8f , owner.transform.rotation,LayerMask.GetMask("Character"));
            elapsedTime += Time.deltaTime;
            owner.NmAgent.SetDestination(rushDirection);
            if (checkPlayer.Length > 0)
            {
                owner.Player.GetComponent<PlayerStatus>().DecreaseHealth(owner.BossStatus.GetAttackDamage());
                owner.CoroutineRunner(owner.Player.GetComponent<PlayerControl>()?.AirBorne((owner.Player.position-owner.transform.position).normalized,7,5));
                rushDirection = owner.transform.position + (owner.Player.position - owner.transform.position) * 6;
                owner.NmAgent.SetDestination(rushDirection);
                isRushing = false;
            }
            else if (Vector3.Distance(owner.Player.position,owner.transform.position)<= minRushDist)
            {
                rushDirection = owner.transform.position + (owner.Player.position - owner.transform.position)*6;
                owner.NmAgent.SetDestination(rushDirection);
                isRushing = false;
            }

            yield return null;
        }
        isRushing = false;
        rushTimer = 0f;
    }
    
}