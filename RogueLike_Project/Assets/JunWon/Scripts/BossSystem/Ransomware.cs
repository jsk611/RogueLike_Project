using System.Buffers;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class Ransomware : MonoBehaviour
{
    #region Components
    [Header("Basic Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent nmAgent;
    [SerializeField] private FieldOfView fov;
    [SerializeField] private MonsterStatus monsterStatus;
    [SerializeField] private AbilityManager abilityManger;

    [Header("Transform References")]
    [SerializeField] protected Transform target;
    [SerializeField] private Transform body;
    [SerializeField] private Transform head;
    [SerializeField] private Transform firePoint;
    #endregion

    #region Combat Settings
    [Header("Combat")]
    [SerializeField] private GameObject dataPacket;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] protected float rotateSpeed = 2.0f;
    private bool canRotate = true;
    #endregion

    #region Status & State
    [Header("State")]
    [SerializeField] private StateMachine<Ransomware> fsm;
    public bool IsIntroAnimFinished { get; set; } = false;
    public bool summonedMonster = false;
    public Summoner master = null;
    #endregion

    #region State References
    private Phase1_Attack_State meeleAttackState;
    private Phase1_BasicRangedAttack_State rangedAttackState;
    private Phase1_SpeacialAttack_State specialAttackState;
    private Phase2State_Ransomeware phase2State;
    #endregion

    #region Animation Event Handlers
    // 기본 원거리 공격 애니메이션 이벤트

    public void OnMeeleAttackFinished()
    {
        if (meeleAttackState != null)
        {
            meeleAttackState.OnAttackFinished();
        }
    }

    public void OnRangedAttackFinished()
    {
        if (rangedAttackState != null)
        {
            rangedAttackState.OnAttackFinished();
        }
    }

    




    #endregion

    #region State Setters
    public void SetMeeleAttackState(Phase1_Attack_State state)
    {
        meeleAttackState = state;
    }
    public void SetRangedAttackState(Phase1_BasicRangedAttack_State state)
    {
        rangedAttackState = state;
    }
    public void SetSpecialAttackState(Phase1_SpeacialAttack_State state)
    {
        specialAttackState = state;
    }
    public void SetPhase2State(Phase2State_Ransomeware state)
    {
        phase2State = state;
    }
    #endregion

    #region Effects & UI
    [Header("Visual Effects")]
    [SerializeField] private GameObject splashFx;
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private Material startMaterial;
    [SerializeField] private Material baseMaterial;

    [Header("UI Elements")]
    [SerializeField] public EnemyHPBar HPBar;
    [SerializeField] private GameObject UIDamaged;
    #endregion

    #region Drops & External
    [Header("Drop Settings")]
    [SerializeField] private GameObject[] items;
    [SerializeField] private int[] itemProbability = { 50, 25, 0 };
    [SerializeField] private float height = 5f;
    [SerializeField] private int DNADrop = 0;

    [Header("External References")]
    [SerializeField] private EnemyCountData enemyCountData;
    #endregion

    #region Public Properties
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public MonsterStatus MonsterStatus => monsterStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManger => abilityManger;
    public Transform FirePoint => firePoint;
    public GameObject DataPacket => dataPacket;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeComponents();
        InitializeFSM();
    }

    private void Update()
    {
        fsm.Update();
    }

    private void LateUpdate()
    {
        if (target != null && canRotate)
        {
            UpdateRotation();
        }
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        target = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();
    }

    private void InitializeFSM()
    {
        var states = CreateStates();
        fsm = new StateMachine<Ransomware>(states.introState);
        SetupTransitions(states);
    }

    private (
        IntroState_Ransomeware introState,
        Phase1State_Ransomware phase1State,
        Phase2State_Ransomeware phase2State,
        DefeatedState_Ransomeware deadState
    ) CreateStates()
    {
        return (
            new IntroState_Ransomeware(this),
            new Phase1State_Ransomware(this),
            new Phase2State_Ransomeware(this),
            new DefeatedState_Ransomeware(this)
        );
    }

    private void SetupTransitions((
        IntroState_Ransomeware introState,
        Phase1State_Ransomware phase1State,
        Phase2State_Ransomeware phase2State,
        DefeatedState_Ransomeware deadState) states)
    {
        fsm.AddTransition(new Transition<Ransomware>(
            states.introState,
            states.phase1State,
            () => true));

        fsm.AddTransition(new Transition<Ransomware>(
            states.phase1State,
            states.phase2State,
            () => monsterStatus.GetHealth() <= 0.5f * monsterStatus.GetMaxHealth()));

        fsm.AddTransition(new Transition<Ransomware>(
            states.phase1State,
            states.deadState,
            () => monsterStatus.GetHealth() <= 0f));
    }
    #endregion

    #region Rotation Handling
    private void UpdateRotation()
    {
        UpdateBodyRotation();
        UpdateHeadRotation();
    }

    private void UpdateBodyRotation()
    {
        Vector3 targetDirection = target.position - body.position;
        targetDirection.y = 0f;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            body.rotation = Quaternion.Slerp(body.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    private void UpdateHeadRotation()
    {
        if (head != null)
        {
            Vector3 headTargetDirection = target.position - head.position;
            float verticalAngle = CalculateVerticalAngle(headTargetDirection);
            verticalAngle = Mathf.Clamp(verticalAngle, -maxVerticalAngle, maxVerticalAngle);

            Quaternion headTargetRotation = Quaternion.Euler(-verticalAngle, 0f, 0f);
            head.localRotation = Quaternion.Slerp(
                head.localRotation,
                headTargetRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    private float CalculateVerticalAngle(Vector3 direction)
    {
        return Vector3.SignedAngle(
            Vector3.ProjectOnPlane(direction, Vector3.up),
            direction,
            Vector3.Cross(direction, Vector3.up)
        );
    }
    #endregion

    #region Public Methods
    public void SetRotationLock(bool locked) => canRotate = !locked;
    public void SetRotationUnlock(bool locked) => canRotate = !locked;
    public void TakeDamage(float dmg) { }
    #endregion
}
