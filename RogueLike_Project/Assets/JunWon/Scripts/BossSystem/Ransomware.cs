using System.Buffers;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class Ransomware : MonoBehaviour
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
    [SerializeField] private StateMachine<Ransomware> fsm;
    #endregion

    [Header("Ability")]
    [SerializeField] private AbilityManager abilityManger;

    [Header("RangedAttackProperty")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject dataPacket;
    public Transform FirePoint => firePoint;
    public GameObject DataPacket => dataPacket;


    

    #region ReadOnlyFunc 
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public MonsterStatus MonsterStatus => monsterStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManger => abilityManger;

    #endregion

    public bool IsIntroAnimFinished = false;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        InitializeComponents();
        InitialzieFSM();
    }

    private void InitialzieFSM()
    {
        var introState = new IntroState_Ransomeware(this);
        var phase1State = new Phase1State_Ransomware(this);
        var deadState = new DefeatedState_Ransomeware(this);
        var HitState = new DefeatedState_Ransomeware(this);

        fsm = new StateMachine<Ransomware>(introState);

        var introToPhase1 = new Transition<Ransomware>(
            introState,
            phase1State,
            () => true);

        var anyToDead = new Transition<Ransomware>(
            phase1State,
            deadState,
            () => monsterStatus.GetHealth() <= 0f);



        fsm.AddTransition(introToPhase1);
        fsm.AddTransition(anyToDead);
    }
    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();
    }

    void Update()
    {
        fsm.Update();
    }

    public void TakeDamage(float dmg)
    {
    }
}
