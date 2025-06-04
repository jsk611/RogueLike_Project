using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using static UnityEngine.UI.GridLayoutGroup;
public class Troy : BossBase
{
    [Header("StateMachine")]
    [SerializeField] protected StateMachine<Troy> fsm;
    private bool isCamouflaged = false;
    private bool isRushing = false;
    private bool isLurked = false;

    [Header("Camouflage")]
    public List<StatusBehaviour> monsterList = new List<StatusBehaviour>();


    [SerializeField] float runInterval = 20f;
    [SerializeField] float enemySummonAmount = 4f;
    float runTimer = 0f;
    


    [Header("Lurk")]
    [SerializeField]
    [Range(0, 100)] List<float> lurkHeathBoundary;
    [SerializeField] float lurkInterval = 20f;
    [SerializeField] GameObject bombEffect;
    [SerializeField] float copyChain = 3f;
    public bool isCopied = false;
    float lurkTimer = 0f;

    [Header("BombThrow")]
    [SerializeField] GameObject bomb;

    public bool ISCAMOUFLAGED { get => isCamouflaged; set => isCamouflaged = value; }
    public float SUMMONAMOUNT => enemySummonAmount;
    public List<StatusBehaviour> SUMMONEDMONSTERS => monsterList;
    public bool ISLURKED { get => isLurked; set => isLurked = value; }
    public float COPYCHAIN { get => copyChain; set => copyChain = value; }
    public GameObject TROYBOMB => bomb;
    public GameObject BOMBEFFECT => bombEffect;


    private void Start()
    {
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponents();
        InitializeFSM();
        ResetBoss();
    }

    private void Update()
    {
   
        runTimer += Time.deltaTime;

        lurkTimer += Time.deltaTime;

        fsm.Update();

        Debug.Log(fsm.CurrentState);
    }

    private void InitializeFSM()
    {
        var introState = new IntroState_Troy(this);
        var idleState = new IdleState_Troy(this);
        var runState = new RunState_Troy(this);
        //     var summonState = new SummonState_Troy(this);
        var lurkState = new LurkState_Troy(this);
        var camouflageState = new CamouflageState_Troy(this);

        var dieState = new DieState_Troy(this);
        
        

        Transition<Troy> introToIdle = new Transition<Troy>(introState, idleState, () => true);

        Transition<Troy> idleToLurk = new Transition<Troy>(idleState, lurkState, () => LurkStateCheck());
        Transition<Troy> lurkToIdle = new Transition<Troy>(lurkState, idleState, () => !isLurked);
        Transition<Troy> lurkToCamouflage = new Transition<Troy>(lurkState, camouflageState, ()=> !isLurked);

        Transition<Troy> idleToCamouflage = new Transition<Troy>(idleState, camouflageState, () => CamouflageStateCheck());
        Transition<Troy> camouflageToIdle = new Transition<Troy>(camouflageState, idleState, () => !isCamouflaged);

        Transition<Troy> idleToRun = new Transition<Troy>(idleState, runState, () => Vector3.Distance(transform.position, Player.position) <= 10f);
        Transition<Troy> runToIdle = new Transition<Troy>(runState, idleState, () => Vector3.Distance(transform.position, Player.position) > 10f);


        Transition<Troy> death = new Transition<Troy>(idleState, dieState, () => bossStatus.GetHealth()<=0);
        fsm = new StateMachine<Troy>(introState);

        fsm.AddTransition(introToIdle);

        fsm.AddTransition(idleToRun);
        fsm.AddTransition(runToIdle);

        fsm.AddTransition(idleToLurk);
        fsm.AddTransition(lurkToIdle);
        fsm.AddTransition(lurkToCamouflage);

        fsm.AddTransition(idleToCamouflage);
        fsm.AddTransition(camouflageToIdle);

        fsm.AddTransition(death);
    }
    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        bossStatus = GetComponent<BossStatus>();
    }

    public override void TakeDamage(float damage, bool showDamage = true)
    {
        if (isCamouflaged || isLurked) return;

        bossStatus.DecreaseHealth(damage);

        if(lurkHeathBoundary.Count>0)
        bossStatus.SetHealth(Mathf.Max(bossStatus.GetHealth(), lurkHeathBoundary[lurkHeathBoundary.Count-1]/100f*bossStatus.GetMaxHealth()));

        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
    }
    public void SetCopied(float health)
    {
        copyChain = 0;
        while (lurkHeathBoundary.Count > 0) lurkHeathBoundary.RemoveAt(lurkHeathBoundary.Count - 1);
        bossStatus.SetHealth(health);
    }
    public bool LurkStateCheck()
    {
        if (copyChain < 0) return false;

        if (lurkTimer >= lurkInterval) return true;
        else if (lurkHeathBoundary.Count>0 && bossStatus.GetHealth() <= lurkHeathBoundary[lurkHeathBoundary.Count - 1])
        {
            while (lurkHeathBoundary.Count>0 && bossStatus.GetHealth() <= lurkHeathBoundary[lurkHeathBoundary.Count - 1]) lurkHeathBoundary.RemoveAt(lurkHeathBoundary.Count - 1);
            return true;
        }
        return false;
    }
    public void IdleToLurk()
    {
        isLurked = !isLurked;
        lurkTimer = 0f;
    }

    public bool CamouflageStateCheck()
    {
        if (runTimer >= runInterval) return true;
        return false;
    }
    public void IdleToCamouflage()
    {
        isCamouflaged = !isCamouflaged; 
        runTimer = 0f;
        isRushing = !isRushing;
    }

    public void CoroutineRunner(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
    private void OnDrawGizmos()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.localPosition+new Vector3(0,2.1f,0.3f), collider.size*transform.localScale.x);
        Gizmos.DrawCube(transform.position,Vector3.one*0.1f);
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
        isLurked = false;
        runTimer = 0f;
        lurkTimer = 0f;
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

    // lurkHeathBoundary 원본 데이터 백업 및 복구
    [SerializeField] private List<float> originalLurkHeathBoundary = new List<float>();

    private void Awake()
    {
        // 원본 데이터 백업
        originalLurkHeathBoundary = new List<float>(lurkHeathBoundary);
    }

    private void RestoreLurkHealthBoundary()
    {
        lurkHeathBoundary = new List<float>(originalLurkHeathBoundary);
    }
    #endregion


}
