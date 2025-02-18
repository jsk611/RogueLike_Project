using System.Buffers;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class Ransomware : BossBase
{
    #region Components
    [Header("Basic Components")]
    [SerializeField] private AbilityManager abilityManager;

    [Header("Transform References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform explosionPoint;
    #endregion

    #region Combat Settings
    [Header("Combat")]
    [SerializeField] private GameObject dataPacket;
    private bool canRotate = true;
    #endregion

    #region Status & State
    [Header("State")]
    [SerializeField] private StateMachine<Ransomware> fsm;
    public bool IsIntroAnimFinished { get; set; } = false;
    #endregion

    #region State References
    private Phase1_Attack_State meeleAttackState;
    private Phase1_BasicRangedAttack_State rangedAttackState;
    private Phase1_SpeacialAttack_State specialAttackState;
    private Phase2State_Ransomeware phase2State;
    private Phase2_DataBlink_State blinkState;
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
    public void FireProjectileFromAnimation()
    {
        if (rangedAttackState != null)
        {
            rangedAttackState.FireProjectile();
        }
    }

    public void OnSpecialAttackFinished()
    {
        if (specialAttackState != null)
        {
            specialAttackState.OnAttackFinished();
        }
    }

    public void DataExplodeFromAnimation()
    {
        if (specialAttackState != null)
        {
            specialAttackState.ExplodeData();
        }
    }

    public void OnDataBlinkFinished()
    {
        if (blinkState != null)
        {
            blinkState.OnAttackFinished();
        }
    }
    public void DataBlinkFromAnimation()
    {
        if (blinkState != null)
        {
            blinkState.OnTeleport();
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
    public void SetDataBlinkState(Phase2_DataBlink_State state)
    {
        blinkState = state;
    }
    #endregion


    #region Public Properties
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public BossStatus MonsterStatus => bossStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManager => abilityManager;
    public Transform FirePoint => firePoint;
    public GameObject DataPacket => dataPacket;
    public Transform ExplosionPoint => explosionPoint;
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
            () => bossStatus.GetHealth() <= 0.5f * bossStatus.GetMaxHealth()));

        fsm.AddTransition(new Transition<Ransomware>(
            states.phase1State,
            states.deadState,
            () => bossStatus.GetHealth() <= 0f));
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
    public override void TakeDamage(float damage, bool showDamage = true)
    {
        bossStatus.DecreaseHealth(damage);

        EventManager.Instance.TriggerMonsterDamagedEvent();
        Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
       
        if (bossStatus.GetHealth() <= 0.5f * bossStatus.GetMaxHealth() && fsm.CurrentState is Phase1State_Ransomware)
        {
            InterruptCurrentAction(InterruptReason.PhaseTransition);
        }

        if (bossStatus.GetHealth() <= 0)
        {
            InterruptCurrentAction(InterruptReason.ForcedInterrupt);
        }
    }
    #endregion


    #region Interrupt Handling
    public enum InterruptReason
    {
        PhaseTransition,
        Stunned,
        PlayerDeath,
        ForcedInterrupt
    }

    public void InterruptCurrentAction(InterruptReason reason)
    {
        // 현재 State에 따른 Interrupt 처리
        if (fsm.CurrentState is Phase1State_Ransomware phase1)
        {
            phase1.Interrupt();
        }
        else if (fsm.CurrentState is Phase2State_Ransomeware phase2)
        {
            phase2.Interrupt();
        }

        // 애니메이션 리셋
        ResetAllAnimationTriggers();

        // 특정 인터럽트 사유에 따른 추가 처리
        switch (reason)
        {
            case InterruptReason.PhaseTransition:
                SetRotationLock(true);
                NmAgent.isStopped = true;
                break;

            case InterruptReason.Stunned:
                SetRotationLock(true);
                NmAgent.isStopped = true;
                Animator.SetTrigger("Stunned");
                break;

            case InterruptReason.PlayerDeath:
                SetRotationLock(true);
                NmAgent.isStopped = true;
                Animator.SetTrigger("PlayerDeath");
                break;
        }
    }

    private void ResetAllAnimationTriggers()
    {
        // 모든 공격 관련 트리거 리셋
        Animator.ResetTrigger("Attack");
        Animator.ResetTrigger("RangedAttack");
        Animator.ResetTrigger("SpecialAttack");
        Animator.ResetTrigger("DataBlink");

        // 현재 진행중인 액션 중단
        Animator.SetTrigger("InterruptAction");
    }

    #endregion
}
