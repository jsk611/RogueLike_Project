using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SocialPlatforms;
using static UnityEngine.UI.GridLayoutGroup;
public class Troy : BossBase
{
    [Header("StateMachine")]
    [SerializeField] protected StateMachine<Troy> fsm;
    private bool isCamouflaged = false;
    private bool isRushing = false;

    [Header("Camouflage")]
    public List<StatusBehaviour> monsterList = new List<StatusBehaviour>();




    [SerializeField] float runInterval = 2f;
    [SerializeField] float camouflageInterval = 10f;
    [SerializeField] float lurkInterval = 20f;
    [SerializeField] float stunInterval = 5f;

    [SerializeField] float enemySummonAmount = 4f;
    [SerializeField] Animator bossAnimator;
    private static readonly int HashState = Animator.StringToHash("State");
    [SerializeField] GameObject horseMesh;



    //[Header("Lurk")]
    //[SerializeField]
    //[Range(0, 100)] List<float> lurkHeathBoundary;
    //[SerializeField] float lurkInterval = 20f;
    [SerializeField] GameObject bombEffect;
    public GameObject BOMBEFFECT => bombEffect;
    //[SerializeField] float copyChain = 3f;
    //public bool isCopied = false;
    //float lurkTimer = 0f;


    public bool ISCAMOUFLAGED { get => isCamouflaged; set => isCamouflaged = value; }
    public float SUMMONAMOUNT => enemySummonAmount;
    public List<StatusBehaviour> SUMMONEDMONSTERS => monsterList;
 //   public bool ISLURKED { get => isLurked; set => isLurked = value; }
 //   public float COPYCHAIN { get => copyChain; set => copyChain = value; }
    public float RUNINTERVAL => runInterval;
    public float CAMOUFLAGEINTERVAL => camouflageInterval;
    public float LURKINTERVAL => lurkInterval;
    public float STUNINVTERVAL => stunInterval;

    private EnemySpawnLogic enemyManager;

    public enum AnimatorState { 
        Idle = 0,
        Rush = 1,
        Walk = 2,
        Stunned = 3,
        Standby = 4,
        Camouflage = 5,
        WakeUp = 6,
        Lurk = 7,
        Crash = 8,
    }
    private AnimatorState curState;
    public AnimatorState CURSTATE => curState;

    [HideInInspector] public bool lurkPhase;
    [HideInInspector] public bool crashPhase;

    public float lurkCut;
    public float crashCut;
    private float accumulatedDamage;
    [HideInInspector] public bool crash;

    private void Awake()
    {
        enemyManager = FindAnyObjectByType<EnemySpawnLogic>();
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponents();
        InitializeFSM();
        ResetBoss();
    }

    private void Update()
    {
        fsm.Update();
        nmAgent.speed = bossStatus.GetMovementSpeed();
        Debug.Log(fsm.CurrentState);
    }

    private void InitializeFSM()
    {
        var introState = new IntroState_Troy(this);
        var idleState = new IdleState_Troy(this);
        var runState = new RunState_Troy(this);
        var camouflageState = new CamouflageState_Troy(this);
        var stunnedState = new StunnedStateTroy(this);
        var lurkState = new LurkState_Troy(this);
        var crashState = new CrashState_Troy(this);
        var dieState = new DieState_Troy(this);

        
        

        Transition<Troy> introToIdle = new Transition<Troy>(introState, idleState, () => true);

        Transition<Troy> idleToCamouflage = new Transition<Troy>(idleState, camouflageState, () => curState==AnimatorState.Camouflage);

        Transition<Troy> runToCamouflage = new Transition<Troy>(runState, camouflageState, () => curState==AnimatorState.Camouflage);
        Transition<Troy> camouflageToStun = new Transition<Troy>(camouflageState, stunnedState, () => curState == AnimatorState.Stunned);
        Transition<Troy> StunToIdle = new Transition<Troy>(stunnedState, idleState, () => curState == AnimatorState.WakeUp);

        Transition<Troy> idleToRun = new Transition<Troy>(idleState, runState, () => curState == AnimatorState.Rush);
        Transition<Troy> runToIdle = new Transition<Troy>(runState, idleState, () => curState == AnimatorState.Idle);

        Transition<Troy> idleToLurk = new Transition<Troy>(idleState, lurkState, () => curState == AnimatorState.Lurk);
        Transition<Troy> runToLurk = new Transition<Troy>(runState, lurkState, () => curState == AnimatorState.Lurk);
        Transition<Troy> lurkToRun = new Transition<Troy>(lurkState, runState, () => curState == AnimatorState.Rush);
        Transition<Troy> lurkToStun = new Transition<Troy>(lurkState, stunnedState, () => curState == AnimatorState.Stunned);

        Transition<Troy> idleToCrash = new Transition<Troy>(idleState, crashState, () => bossStatus.GetHealth() <= (bossStatus.GetMaxHealth()/2f) && crash);
        Transition<Troy> crashToStun = new Transition<Troy>(crashState, stunnedState, () => (curState == AnimatorState.Stunned && !crash));


        fsm = new StateMachine<Troy>(introState);

        fsm.AddTransition(introToIdle);

        fsm.AddTransition(idleToRun);
        fsm.AddTransition(runToIdle);

        fsm.AddTransition(idleToCamouflage);
        fsm.AddTransition(runToCamouflage);

        fsm.AddTransition(camouflageToStun);
        fsm.AddTransition(StunToIdle);

        fsm.AddTransition(idleToLurk);
        fsm.AddTransition(runToLurk);
        fsm.AddTransition(lurkToStun);
        fsm.AddTransition(lurkToRun);

        fsm.AddTransition(idleToCrash);
        fsm.AddTransition(crashToStun);

    }
    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        bossStatus = GetComponent<BossStatus>();
        lurkPhase = false;
        crashPhase = false;
        crash = true;
        accumulatedDamage = 0;
        curState = isBoss? AnimatorState.Idle : AnimatorState.Lurk;
    }

    public override void TakeDamage(float damage, bool showDamage = true)
    {
        if (isCamouflaged) return;
        if (lurkPhase || crashPhase) accumulatedDamage += damage;
        if ((lurkPhase && accumulatedDamage >= lurkCut) || (crashPhase && accumulatedDamage >= crashCut))
        {
            accumulatedDamage = 0;
            lurkPhase = false;
            ChangeState(AnimatorState.Stunned);
        }

            bossStatus.DecreaseHealth(damage);

        //if(lurkHeathBoundary.Count>0 && bossStatus.GetHealth() <= lurkHeathBoundary[lurkHeathBoundary.Count-1])
        //bossStatus.SetHealth(Mathf.Max(bossStatus.GetHealth(), lurkHeathBoundary[lurkHeathBoundary.Count-1]/100f*bossStatus.GetMaxHealth()));

        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;

        if(bossStatus.GetHealth() <=0)
        {
            var death = new DieState_Troy(this);
            fsm.ForcedTransition(death);
        }
    }
 

    public void HideAndSeek(bool val)
    {
        horseMesh.SetActive(val);
        GetComponent<BoxCollider>().enabled = val;
        transform.Find("EnemyIcon").gameObject.SetActive(val);
    }
    public void HideHP(bool val)
    {
        HPBar.gameObject.SetActive(val);
    }
    public void CoroutineRunner(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
    public void ChangeState(AnimatorState state)
    {
        curState = state;
        bossAnimator.SetInteger(HashState, (int)state);
    }
    public void MakeDoll()
    {
        var DollState = new IntroState_Troy(this);
        fsm.ForcedTransition(DollState);
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
        RestoreLurkHealthBoundary();
    }

    private void ResetBossState()
    {
        // 초기 상태값 설정
        isCamouflaged = false;
        isRushing = false;
    }

    private void ClearSummonedMonsters()
    {
        foreach (var monster in monsterList)
        {
            if (monster != null)
            {
                Destroy(monster);
            }
        }


        monsterList.Clear();
    }

    private void ResetStateMachine()
    {
        if (fsm != null)
        {
            // FSM을 새로 초기화
            InitializeFSM();
        }
    }
    public void SummonMinion()
    {
        SUMMONEDMONSTERS.RemoveAll(item => item == null);

        Vector3 randomPos = transform.position + new Vector3(UnityEngine.Random.Range(-4f, 4f), 0, UnityEngine.Random.Range(-4f, 4f));
        GameObject obj = GameObject.Instantiate(enemyManager.GetEnemyPrefab(EnemyType.SpiderMinion), randomPos, Quaternion.identity);
        StatusBehaviour enemy = obj.GetComponent<StatusBehaviour>();
        enemy.SetHealth(100);
        enemy.SetMaxHealth(100);
        enemy.GetComponent<SpiderPrime>().isBoss = false;
        SUMMONEDMONSTERS.Add(enemy.GetComponent<StatusBehaviour>());
    }

    // lurkHeathBoundary 원본 데이터 백업 및 복구
    [SerializeField] private List<float> originalLurkHeathBoundary = new List<float>();

  
    private void RestoreLurkHealthBoundary()
    {
        //lurkHeathBoundary = new List<float>(originalLurkHeathBoundary);
    }
    #endregion


}
