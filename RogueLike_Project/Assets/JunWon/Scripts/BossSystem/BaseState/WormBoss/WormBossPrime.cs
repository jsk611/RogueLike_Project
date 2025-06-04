using InfimaGames.LowPolyShooterPack;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.Services.Analytics.Platform;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class WormBossPrime : BossBase
{
    [Header("StateMachine")]
    [SerializeField] protected StateMachine<WormBossPrime> fsm;

    [Header("Minion Settings")]
    public List<GameObject> minions = new List<GameObject>();
    public List<GameObject> summoned = new List<GameObject>();

    [SerializeField] private float summonInterval;
    [SerializeField] private float shootInterval;
    [SerializeField] private float chaseInterval;
    [SerializeField] private float digInterval;
    private float summonTimer = 0f;
    private float shootTimer = 0f;
    private float chaseTimer = 0f;
    private float digTimer = 0f;

    private bool SumToWanTrigger = false;
    private bool ShtToWanTrigger = false;
    private bool ChsToWanTrigger = false;
    private bool DigToWanTrigger = false;

    private bool isPartitioned = false;
    WormBossBodyMovement wormBossBodyMovement;


    #region ReadOnlyFunc 

    public List<GameObject> Summoned => summoned;
    
    public bool ATKTOWANDER => ShtToWanTrigger;
    public bool SUMTOWANDER => SumToWanTrigger;
    public WormBossBodyMovement WormBossBodyMovement => wormBossBodyMovement;
    public bool ISPARTITIONED => isPartitioned;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        wormBossBodyMovement = GetComponent<WormBossBodyMovement>();
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponent();
        InitializeFSM();


    }

    void Update()
    {
        fsm.Update();
        Debug.Log(fsm.CurrentState);

        summonTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;
        chaseTimer += Time.deltaTime;
        digTimer += Time.deltaTime;

    }

    private void InitializeFSM()
    {
        var introState = new IntroState_WormBoss(this);
        var chaseState = new ChaseState_WormBoss(this);
        var summonState = new SummonState_WormBoss(this);
        var wanderState = new WanderingStateWormBoss(this);
        var shootState = new ShootState_WormBoss(this);
        var digState = new DigState_WormBoss(this);

        

        fsm = new StateMachine<WormBossPrime>(introState);

        Transition<WormBossPrime> IntroToWander;

        Transition<WormBossPrime> WanderToSummon;
        Transition<WormBossPrime> SummonToWander;

        Transition<WormBossPrime> WanderToShoot;
        Transition<WormBossPrime> ShootToWander;

        Transition<WormBossPrime> WanderToChase;
        Transition<WormBossPrime> ChaseToWander;

        Transition<WormBossPrime> WanderToDig;
        Transition<WormBossPrime> DigToWander;

        
        IntroToWander = new Transition<WormBossPrime>(
            introState,
            wanderState,
            () => true
        );
        WanderToSummon = new Transition<WormBossPrime>(
            wanderState,
            summonState,
            () => summonTimer >= summonInterval
        );
        SummonToWander = new Transition<WormBossPrime>(
            summonState,
            wanderState,
            () => SumToWanTrigger
        );
        WanderToShoot = new Transition<WormBossPrime>(
            wanderState,
            shootState,
            () => shootTimer >= shootInterval
        );
        ShootToWander = new Transition<WormBossPrime>(
            shootState,
            wanderState,
            () => ShtToWanTrigger
        );
        WanderToChase = new Transition<WormBossPrime>(
            wanderState,
            chaseState,
            () => chaseTimer >= chaseInterval
        );
        ChaseToWander = new Transition<WormBossPrime>(
            chaseState,
            wanderState,
            () => ChsToWanTrigger
        );
        WanderToDig = new Transition<WormBossPrime>(
            wanderState,
            digState,
            () => digTimer >= digInterval
        );
        DigToWander = new Transition<WormBossPrime>(
            digState,
            wanderState,
            () => DigToWanTrigger
        );




        fsm.AddTransition(IntroToWander);
        fsm.AddTransition(WanderToSummon);
        fsm.AddTransition(SummonToWander);
        fsm.AddTransition(WanderToShoot);
        fsm.AddTransition(ShootToWander);
        fsm.AddTransition(WanderToChase);
        fsm.AddTransition(ChaseToWander);
        fsm.AddTransition(WanderToDig);
        fsm.AddTransition(DigToWander);
    }

    private void InitializeComponent()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        bossStatus = GetComponent<BossStatus>();
        fov = GetComponent<FieldOfView>();
    }

    public void SummonToWander()
    {
        summonTimer = 0f;
        SumToWanTrigger = !SumToWanTrigger;
    }
    public void ShootToWander()
    {
        shootTimer = 0f;
        ShtToWanTrigger = !ShtToWanTrigger;
    }
    public void ChaseToWander()
    {
        chaseTimer = 0f;
        ChsToWanTrigger = !ChsToWanTrigger;
    }
    public void DigToWander()
    {
        digTimer = 0f;
        DigToWanTrigger = !DigToWanTrigger;
    }
    
    public override void TakeDamage(float damage, bool showDamage = true)
    {
        bossStatus.DecreaseHealth(damage);
        
        

        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, wormBossBodyMovement.WormHead.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;

        if (bossStatus.GetHealth() <= bossStatus.GetMaxHealth()/2 && !isPartitioned)
        {
            enemyCountData.enemyCount++;
            WormPartition();
        }
        
        if (bossStatus.GetHealth() <= 0)
        {
            var dieState = new DieState_WormBoss(this);
            Transition<WormBossPrime> AnyToDeath;
            AnyToDeath = new Transition<WormBossPrime>(
              null,
              dieState,
              () => true
            );
            fsm.AddTransition(AnyToDeath);
        }
    }
    public void CoroutineRunner(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    private void WormPartition()
    {
        ShootToWander();
        isPartitioned = true;
        EnemySpawnLogic logic = EnemySpawnLogic.instance;
        GameObject prefab = logic.GetEnemyPrefab(EnemyType.Wormboss);
        Debug.Log(prefab);

        List<Transform> bodyList = WormBossBodyMovement.BodyList;

        Transform partition = bodyList[bodyList.Count / 2];
        GameObject subWorm = GameObject.Instantiate(prefab, partition.position,partition.rotation,null);
        WormBossPrime subWormPrime = subWorm.GetComponent<WormBossPrime>();
        WormBossBodyMovement subBody = subWorm.GetComponent<WormBossBodyMovement>();

        summoned.Add(subWorm);

        

        subWormPrime.isPartitioned = true;
        for (int i = 0; i < subBody.BodyList.Count/2; i++)
        {
            subBody.BodyList[i].position = bodyList[i+bodyList.Count/2].position;
            subBody.BodyList[i].rotation = bodyList[i+bodyList.Count/2].rotation;
            Destroy(bodyList[i + bodyList.Count / 2].gameObject);
            Destroy(subBody.BodyList[i+subBody.BodyList.Count/2].gameObject);
        }
        subBody.BodyList.RemoveRange(subBody.BodyList.Count/2, subBody.BodyList.Count-bodyList.Count/2);
        bodyList.RemoveRange(bodyList.Count/2,bodyList.Count-bodyList.Count/2);
        


        Debug.Log("origin dead");
      
    }

    #region Reset
    // 보스 상태 초기화 메서드
    public override void ResetBoss()
    {
        // 상태 변수들 초기화
        ResetBossState();

        if (bossStatus != null)
        {
            bossStatus.SetHealth(bossStatus.GetMaxHealth());
        }

        // 소환된 몬스터들 정리
        ClearSummonedMonsters();

        // 애니메이터 상태 초기화
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        // NavMeshAgent 초기화
        if (nmAgent != null)
        {
            nmAgent.isStopped = false;
            nmAgent.ResetPath();
        }

        // FSM을 처음 상태로 되돌리기
        ResetStateMachine();

        // lurkHeathBoundary 복구 (원본 데이터 백업 필요)
    }

    private void ResetBossState()
    {
      summonTimer = 0f;
      shootTimer = 0f;
      chaseTimer = 0f;
      digTimer = 0f;

      SumToWanTrigger = false;
      ShtToWanTrigger = false;
      ChsToWanTrigger = false;
      DigToWanTrigger = false;

      isPartitioned = false;
    }

    private void ClearSummonedMonsters()
    {
       foreach (GameObject monster in summoned)
       {
            if (monster != null)
                Destroy(monster);
       }

    }

    private void ResetStateMachine()
    {
        if (fsm != null)
        {
            // FSM을 새로 초기화
            InitializeFSM();
        }
    }
    #endregion



}
