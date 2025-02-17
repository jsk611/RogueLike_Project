using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

public class Troy : BossBase
{
    [Header("StateMachine")]
    [SerializeField] protected StateMachine<Troy> fsm;
    private bool isCamouflaged = false;
    private bool isLurked = false;

    [Header("Camouflage")]
    [SerializeField] List<GameObject> monsterList;

    float runTimer = 0f;
    float lurkTimer = 0f;


    [SerializeField] private GameObject TroyBomb;



    public bool ISCAMOUFLAGED { get => isCamouflaged; set => isCamouflaged = value; }
    public bool ISLURKED { get => isLurked; set => isLurked = value; }
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

        Transition<Troy> idleToLurk = new Transition<Troy>(idleState, lurkState, () => lurkTimer>=10f);
        Transition<Troy> lurkToIdle = new Transition<Troy>(lurkState, idleState, () => !isLurked);

        Transition<Troy> idleToCamouflage = new Transition<Troy>(idleState, camouflageState, ()=> isCamouflaged);
        Transition<Troy> CamouflageToIdle = new Transition<Troy>(camouflageState, idleState, () => !isCamouflaged);

        Transition<Troy> idleToRun = new Transition<Troy>(idleState, runState, () => Vector3.Distance(transform.position,Player.position)<=10f);
        Transition<Troy> runToIdle = new Transition<Troy>(runState, idleState, () => Vector3.Distance(transform.position,Player.position)>10f);

        fsm = new StateMachine<Troy>(introState);

        fsm.AddTransition(introToIdle);
        //    fsm.AddTransition(idleToLurk);
        fsm.AddTransition(idleToRun);
        fsm.AddTransition(runToIdle);

        fsm.AddTransition(idleToLurk);
        fsm.AddTransition(lurkToIdle);
        
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
        if (isCamouflaged) return;

        bossStatus.DecreaseHealth(damage);
        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
    }

    public void IdleToLurk()
    {
        isLurked = !isLurked;
        lurkTimer = 0f;
    }
}
