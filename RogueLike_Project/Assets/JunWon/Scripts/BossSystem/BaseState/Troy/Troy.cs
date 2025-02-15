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

    [Header("Camouflage")]
    [SerializeField] List<GameObject> monsterList;


    [SerializeField] GameObject TroyBomb;

    public bool ISCAMOUFLAGED { get => isCamouflaged; set => isCamouflaged = value; }

    private void Start()
    {
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponents();
        InitializeFSM();
    }

    private void Update()
    {
        fsm.Update();
    }

    private void InitializeFSM()
    {
        var introState = new IntroState_Troy(this);
        var idleState = new IdleState_Troy(this);
        var chaseState = new ChaseState_Troy(this);
   //     var summonState = new SummonState_Troy(this);
        var lurkState = new LurkState_Troy(this);

        fsm = new StateMachine<Troy>(introState);
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

    }
}
