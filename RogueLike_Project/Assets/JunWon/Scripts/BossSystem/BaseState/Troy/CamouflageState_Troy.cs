using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Tutorials.Core.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamouflageState_Troy : State<Troy>
{
    public CamouflageState_Troy(Troy owner) : base(owner) { }
    //select enemy in field
    //enforce it
    //being immune
    EnemySpawnLogic enemyManager;
    StatusBehaviour CamouflageObject;
    PlayerStatus player;
    float summonEnemyCount;


    float bombThrowInterval = 2f;
    float bombThrowTimer = 2f;
    float bombDistance = 3f;

    float rushTimer = 0f;
    bool isRushing = false;
    float rushInterval = 5f;
    float minRushDist = 10f;

   

    // Start is called before the first frame update
    public override void Enter()
    {
        if (enemyManager == null)
        {
            player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
            enemyManager = GameObject.FindAnyObjectByType<EnemySpawnLogic>();
        }
        owner.SUMMONEDMONSTERS.RemoveAll(item => item == null);
        if (owner.SUMMONEDMONSTERS.Count > 0)
            CamouflageObject = owner.SUMMONEDMONSTERS[Random.Range(0, owner.SUMMONEDMONSTERS.Count)];
        else
        {
            for (int i = 0;i<owner.SUMMONAMOUNT;i++)
            {
                Vector3 randomPos = owner.transform.position + new Vector3(Random.Range(-4f,4f),0,Random.Range(-4f,4f));
                GameObject obj = GameObject.Instantiate(enemyManager.GetEnemyPrefab(EnemyType.SpiderMinion), randomPos, Quaternion.identity);
                StatusBehaviour enemy = obj.GetComponent<StatusBehaviour>();
                owner.COPYLIST.Add(obj);
                enemy.SetHealth(100);
                enemy.SetMaxHealth(100);
                enemy.GetComponent<SpiderPrime>().isBoss = false;
                owner.SUMMONEDMONSTERS.Add(enemy.GetComponent<StatusBehaviour>());
            }
            CamouflageObject = owner.SUMMONEDMONSTERS[Random.Range(0, owner.SUMMONEDMONSTERS.Count)];
        }
        owner.IdleToCamouflage();
        owner.HideAndSeek(false);
        owner.CoroutineRunner(FollowPlayer());
        owner.BossStatus.SetMovementSpeed(13);
    }
    public override void Update()
    {
        //bombThrowTimer += Time.deltaTime;
        rushTimer += Time.deltaTime;

    
        //if (bombThrowTimer >= bombThrowInterval)
        //{
        //    Debug.Log(bombThrowInterval);
        //    bombThrowTimer = 0f;
        //    GameObject.Instantiate(owner.TROYBOMB, owner.Player.position+Vector3.up, owner.transform.rotation).GetComponent<Rigidbody>().velocity = Vector3.up*3;
        //}
        if (CamouflageObject == null)
        {
            Debug.Log("Back to Idle");
            owner.IdleToCamouflage();
            owner.TakeDamage(owner.BossStatus.GetHealth() * 0.2f);
        }
    }
    public override void Exit()
    {
        owner.BossStatus.SetMovementSpeed(40);
        owner.NmAgent.SetDestination(owner.transform.position);
        owner.HideAndSeek(true);
    }
    IEnumerator RushToPlayer()
    {
        Collider[] checkPlayer;
        BoxCollider ownerCollider = owner.GetComponent<BoxCollider>();
        float elapsedTime = 0f;
        while(isRushing && elapsedTime <= 2f)
        { 
            Vector3 rushDirection = owner.Player.position;
            checkPlayer = Physics.OverlapBox(owner.transform.position+ new Vector3(0, 2f, 0.3f), ownerCollider.size * 1.6f, owner.transform.rotation,LayerMask.GetMask("Character"));
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
    private IEnumerator FollowPlayer()
    {
        float time = 0f;
        while (owner.ISCAMOUFLAGED)
        {
            owner.NmAgent.SetDestination(owner.Player.position);
            if(Vector3.Distance(owner.Player.position,owner.transform.position) <= bombDistance && time >= bombThrowInterval)
            {
                GameObject.Instantiate(owner.BOMBEFFECT, owner.transform.position, Quaternion.identity);
                Collider[] scan = Physics.OverlapSphere(owner.transform.position, bombDistance, LayerMask.GetMask("Character"));
                if (scan.Length > 0) player.DecreaseHealth(owner.BossStatus.GetAttackDamage());
                time = 0f;
            }
            time += Time.deltaTime;
            yield return null;
        }
    }
}