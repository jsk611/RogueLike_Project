using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;

public class Troy : BossBase
{
    [Header("StateMachine")]
    [SerializeField] protected StateMachine<Troy> fsm;
    private bool isCamouflaged = false;
    private bool isLurked = false;

    [Header("Camouflage")]
    [SerializeField] List<GameObject> monsterList;

    [SerializeField] float runInterval = 20f;
    float runTimer = 0f;
    float lurkTimer = 0f;

    [Header("Lurk")]
    [SerializeField]
    [Range(0, 100)] List<float> lurkHeathBoundary;
    [SerializeField] float lurkInterval = 20f;
    [SerializeField] private GameObject TroyBomb;
    [SerializeField] float copyChain = 3f;
    public bool isCopied = false;



    public bool ISCAMOUFLAGED { get => isCamouflaged; set => isCamouflaged = value; }
    public bool ISLURKED { get => isLurked; set => isLurked = value; }
    public float COPYCHAIN { get => copyChain; set => copyChain = value; }
    public GameObject TROYBOMB => TroyBomb;

    private void Start()
    {
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponents();
        InitializeFSM();
    }

    private void Update()
    {
        runTimer += Time.deltaTime;
        lurkTimer += Time.deltaTime;

        Debug.Log(fsm.CurrentState);
        fsm.Update();
    }

    private void InitializeFSM()
    {
        var introState = new IntroState_Troy(this);
        var idleState = new IdleState_Troy(this);
        var runState = new RunState_Troy(this);
        //     var summonState = new SummonState_Troy(this);
        var lurkState = new LurkState_Troy(this);
        var camouflageState = new CamouflageState_Troy(this);

        Transition<Troy> introToIdle = new Transition<Troy>(introState, idleState, () => true);

        Transition<Troy> idleToLurk = new Transition<Troy>(idleState, lurkState, () => LurkStateCheck());
        Transition<Troy> lurkToIdle = new Transition<Troy>(lurkState, idleState, () => !isLurked);

        Transition<Troy> idleToCamouflage = new Transition<Troy>(idleState, camouflageState, () => CamouflageStateCheck());
        Transition<Troy> camouflageToIdle = new Transition<Troy>(camouflageState, idleState, () => !isCamouflaged);

        Transition<Troy> idleToRun = new Transition<Troy>(idleState, runState, () => Vector3.Distance(transform.position, Player.position) <= 10f);
        Transition<Troy> runToIdle = new Transition<Troy>(runState, idleState, () => Vector3.Distance(transform.position, Player.position) > 10f);

        fsm = new StateMachine<Troy>(introState);

        fsm.AddTransition(introToIdle);

        fsm.AddTransition(idleToRun);
        fsm.AddTransition(runToIdle);

        fsm.AddTransition(idleToLurk);
        fsm.AddTransition(lurkToIdle);

        fsm.AddTransition(idleToCamouflage);
        fsm.AddTransition(camouflageToIdle);
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
        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
        if (bossStatus.GetHealth() <= 0)
        {
            var dieState = new DieState_Troy(this);
            fsm.AddTransition(new Transition<Troy>(null, dieState, () => true));
        }
    }

    public bool LurkStateCheck()
    {
        if (copyChain < 0) return false;

        if (lurkTimer >= lurkInterval) return true;
        else if (bossStatus.GetHealth() <= lurkHeathBoundary[lurkHeathBoundary.Count - 1])
        {
            while (bossStatus.GetHealth() <= lurkHeathBoundary[lurkHeathBoundary.Count - 1]) lurkHeathBoundary.RemoveAt(lurkHeathBoundary.Count - 1);
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
    }
}
