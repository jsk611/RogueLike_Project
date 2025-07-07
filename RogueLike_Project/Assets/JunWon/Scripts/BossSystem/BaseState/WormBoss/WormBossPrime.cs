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
        var dieState = new DieState_WormBoss(this);

        

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

        Transition<WormBossPrime> WanderToDeath;
        
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
        WanderToDeath = new Transition<WormBossPrime>(
          wanderState,
          dieState,
          () => bossStatus.GetHealth()<=0
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
        fsm.AddTransition(WanderToDeath);
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
        subWormPrime.GetComponent<BossStatus>().SetHealth(bossStatus.GetHealth());

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
    }

    #region Reset
    // ���� ���� �ʱ�ȭ �޼���
    public override void ResetBoss()
    {
        // ���� ������ �ʱ�ȭ
        ResetBossState();

        if (bossStatus != null)
        {
            bossStatus.SetHealth(bossStatus.GetMaxHealth());
        }

        // ��ȯ�� ���͵� ����
        ClearSummonedMonsters();

        // �ִϸ����� ���� �ʱ�ȭ
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        // NavMeshAgent �ʱ�ȭ
        if (nmAgent != null)
        {
            nmAgent.isStopped = false;
            nmAgent.ResetPath();
        }

        // FSM�� ó�� ���·� �ǵ�����
        ResetStateMachine();

        // lurkHeathBoundary ���� (���� ������ ��� �ʿ�)
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
            // FSM�� ���� �ʱ�ȭ
            InitializeFSM();
        }
    }
    #endregion



}
