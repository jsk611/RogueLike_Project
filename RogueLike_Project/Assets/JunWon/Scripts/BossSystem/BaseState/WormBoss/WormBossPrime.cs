using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using static UnityEngine.GraphicsBuffer;

public class WormBossPrime : MonoBehaviour
{
    #region Serialized Fields
    [Header("General Settings")]
    [SerializeField] protected Transform target;
    [SerializeField] private Transform body; // Character body (XZ rotation)
    [SerializeField] private Transform head; // Head or torso (vertical rotation)
    [SerializeField] private float maxVerticalAngle = 60f; // Maximum vertical angle for head rotation
    [SerializeField] protected float rotateSpeed = 2.0f; // Rotation speed
    public bool summonedMonster = false;
    public Summoner master = null;

    [Header("Minion Settings")]
    public List<GameObject> minions = new List<GameObject>();
    private List<GameObject> summoned = new List<GameObject>();


    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent nmAgent;
    [SerializeField] private FieldOfView fov;
    [SerializeField] private MonsterStatus monsterStatus;
    [SerializeField] private Rigidbody playerRigidBody;


    [Header("Effects")]
    [SerializeField] private GameObject splashFx;
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private Material startMaterial;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private GameObject[] items;
    [SerializeField] private int[] itemProbability = { 50, 25, 0 };
    [SerializeField] private float height = 5f;
    [SerializeField] private int DNADrop = 0;

    [Header("UI")]
    [SerializeField] public EnemyHPBar HPBar;
    [SerializeField] private GameObject UIDamaged;

    [Header("External Data")]
    [SerializeField] private EnemyCountData enemyCountData;

    [Header("StateMachine")]
    [SerializeField] private StateMachine<WormBossPrime> fsm;
    [SerializeField] private float summonInterval;
    [SerializeField] private float attackInterval;
    private float summonTimer = 0f;
    private float attackTimer = 0f;

    private bool SumToWanTrigger = false;
    private bool AtkToWanTrigger = false;
    


    #endregion

    #region ReadOnlyFunc 
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public MonsterStatus MonsterStatus => monsterStatus;
    public FieldOfView FOV => fov;
    public List<GameObject> Summoned => summoned;
    public bool ATKTOWANDER => AtkToWanTrigger;
    public bool SUMTOWANDER => SumToWanTrigger;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponent();
        InitializeFSM();
        
    }

    void Update()
    {
        fsm.Update();
        Debug.Log(fsm.CurrentState);

        summonTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;
    }

    private void InitializeFSM()
    {  
        var introState = new IntroState_WormBoss(this);
        var chaseState = new ChaseState_WormBoss(this);
        var summonState = new SummonState_WormBoss(this);
        var wanderState = new WanderingStateWormBoss(this);
        var shootState = new ShootState_WormBoss(this);

        fsm = new StateMachine<WormBossPrime>(introState);

        Transition<WormBossPrime> IntroToWander;

        Transition<WormBossPrime> WanderToSummon;
        Transition<WormBossPrime> SummonToWander;

        Transition<WormBossPrime> WanderToShoot;
        Transition<WormBossPrime> ShootToWander;

        Transition<WormBossPrime> WanderToChase;
        Transition<WormBossPrime> ChaseToWander;

        IntroToWander = new Transition<WormBossPrime>(
            introState,
            wanderState,
            () =>true
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
            () => attackTimer >= attackInterval
        );
        ShootToWander = new Transition<WormBossPrime>(
            shootState,
            wanderState,
            () => AtkToWanTrigger
        );
        //WanderToChase = new Transition<WormBossPrime>(
        //    wanderState,
        //    chaseState,
        //    () => 
        //);
        ChaseToWander = new Transition<WormBossPrime>(
            chaseState,
            wanderState,
            () => AtkToWanTrigger
        );


        fsm.AddTransition(IntroToWander);
        fsm.AddTransition(WanderToSummon);
        fsm.AddTransition(SummonToWander);
        fsm.AddTransition(WanderToShoot);
        fsm.AddTransition(ShootToWander);
     //   fsm.AddTransition(WanderToChase);
        fsm.AddTransition(ChaseToWander);
    }

    private void InitializeComponent()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();
    }

    public void SummonToWander()
    {
        summonTimer = 0f;
        SumToWanTrigger = !SumToWanTrigger;
    }
    public void FlyToWander()
    {
        attackTimer = 0f;
        AtkToWanTrigger = !AtkToWanTrigger;
    }
    
    
}
