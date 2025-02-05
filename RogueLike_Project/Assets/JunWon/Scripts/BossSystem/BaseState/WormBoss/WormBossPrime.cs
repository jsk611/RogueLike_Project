using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
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
    [SerializeField] private float summonInterval;
    public float summonTimer = 0f;
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

    
    #endregion

    #region ReadOnlyFunc 
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public MonsterStatus MonsterStatus => monsterStatus;
    public FieldOfView FOV => fov;
    public List<GameObject> Summoned => summoned;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        InitializeComponent();
        InitializeFSM();
        
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update();
        Debug.Log(fsm.CurrentState);
    }

    private void InitializeFSM()
    {  
        var introState = new IntroState_WormBoss(this);
        var chaseState = new ChaseState_WormBoss(this);
        var summonState = new SummonState_WormBoss(this);

        fsm = new StateMachine<WormBossPrime>(introState);

        Transition<WormBossPrime> IntroToChase = new Transition<WormBossPrime>(
            introState,
            chaseState,
            () => Vector3.Distance(transform.position, target.position) <= 8
        );
        Transition<WormBossPrime> ChaseToSummon = new Transition<WormBossPrime>(
            chaseState,
            summonState,
            () => summonTimer >= summonInterval
        );
        Transition<WormBossPrime> SummonToIntro = new Transition<WormBossPrime>(
            summonState,
            introState,
            () => summonTimer <= summonInterval
        );
        fsm.AddTransition(IntroToChase);
        fsm.AddTransition(ChaseToSummon);
        fsm.AddTransition(SummonToIntro);
        
    }



    private void InitializeComponent()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();
    }

    
}
