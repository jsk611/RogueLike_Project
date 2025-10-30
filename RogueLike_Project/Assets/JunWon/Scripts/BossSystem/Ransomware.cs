using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class Ransomware : BossBase, IBossEntity
{
    #region Components
    [Header("Basic Components")]
    [SerializeField] private AbilityManager abilityManager;

    [Header("Transform References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform explosionPoint;

    [Header("Summoned")]
    [SerializeField] private List<GameObject> summonedObjects = new List<GameObject>();

    [Header("Basic Skill Lock")]
    private Dictionary<SkillType, float> skillUnlockTimes = new Dictionary<SkillType, float>();

    [Header("Summon Object")]
    [SerializeField] private GameObject shadowPrefab;
    private bool shadowsSpawned = false;
    public bool ShadowsSpawned { get => shadowsSpawned; set => shadowsSpawned = value; }
    private bool isPhase3End = false;
    #endregion

    #region Combat Settings
    [Header("Combat")]
    [SerializeField] private GameObject dataPacket;
    private bool canRotate = true;

    [SerializeField] List<BaseWeapon> weaponsList = new List<BaseWeapon>();

    [SerializeField] float meleeAttackRange;
    [SerializeField] float rangedAttackRange;
    #endregion

    #region Status & State
    [Header("State")]
    [SerializeField] private StateMachine<Ransomware> fsm;
    public bool IsIntroAnimFinished { get; set; } = false;
    #endregion

    #region State References
    private Phase1_Attack_State meleeAttackState;
    private Phase1_BasicRangedAttack_State rangedAttackState;
    private Phase1_SpeacialAttack_State specialAttackState;
    private Phase2_MeleeAttackState meleeAttackState2;
    private Phase2_RangedAttackState rangedAttackState2;
    private Phase2_DataBlink_State blinkState;
    private Phase2_DigitalShadow_State summonState;
    private Phase2_RansomLock_State lockState;
    private DefeatedState_Ransomeware defeatedState;
    #endregion

    #region Animation Event Handlers
    // �⺻ ���Ÿ� ���� �ִϸ��̼� �̺�Ʈ

    public void OnMeleeAttackFinished()
    {
        if (meleeAttackState != null)
        {
            meleeAttackState.OnAttackFinished();
        }

        if (meleeAttackState2 != null)
        {
            meleeAttackState2.OnAttackFinished();
        }
    }

    public void OnRangedAttackFinished()
    {
        if (rangedAttackState != null)
        {
            rangedAttackState.OnAttackFinished();
        }
        if (rangedAttackState2 != null){

            rangedAttackState2.OnAttackFinished();
        }
    }
    public void FireProjectileFromAnimation()
    {
        if (rangedAttackState != null)
        {
            rangedAttackState.FireProjectile();
        }
        if (rangedAttackState2 != null)
        {
            rangedAttackState2.FireProjectile();
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

    public void OnLockFinished()
    {
        if (lockState != null)
        {
            lockState.OnAttackFinished();
        }
    }
    public void LockFromAnimation()
    {
        if (lockState != null)
        {
            lockState.OnStartLockEffect();
        }
    }

    public void OnSummonFinished()
    {
        if (summonState != null)
        {
            summonState.OnAttackFinished();
        }
    }
    public void SummonFromAnimation()
    {
        if (summonState != null)
        {
            summonState.ActivateLastStandSplit();
        }
    }

    public void OnDeadFinished()
    {
        if (defeatedState != null)
        {
            defeatedState.OnDeathAnimationFinished();
        }
    }
    #endregion

    #region State Setters
    public void SetMeleeAttackState(Phase1_Attack_State state)
    {
        meleeAttackState = state;
    }
    public void SetMeleeAttackState(Phase2_MeleeAttackState state)
    {
        meleeAttackState2 = state;
    }
    public void SetRangedAttackState(Phase1_BasicRangedAttack_State state)
    {
        rangedAttackState = state;
    }
    public void SetRangedAttackState(Phase2_RangedAttackState state)
    {
        rangedAttackState2 = state;
    }
    public void SetSpecialAttackState(Phase1_SpeacialAttack_State state)
    {
        specialAttackState = state;
    }
    public void SetDataBlinkState(Phase2_DataBlink_State state)
    {
        blinkState = state;
    }

    public void SetDigitalShadowState(Phase2_DigitalShadow_State state)
    {
        summonState = state;
    }

    public void SetLockState(Phase2_RansomLock_State state)
    {
        lockState = state;
    }
    public void SetDefeatedState(DefeatedState_Ransomeware state)
    {
        defeatedState = state;
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

    public List<GameObject> SummonedMonsters => summonedObjects;

    public float MeleeAttackRange => meleeAttackRange;
    public float RangedAttackRange => rangedAttackRange;

    public GameObject Shadow => shadowPrefab;
    #endregion

    public void DigitalShadowsFinished()
    {
        isPhase3End = true;
    }

    #region Unity Lifecycle
    private void Start()
    {
        InitializeComponents();
        InitializeFSM();
        InitializeWeapons();
        EnableMeleeWeapon();
        anim.SetLayerWeight(anim.GetLayerIndex("Phase1"), 0);
        anim.SetLayerWeight(anim.GetLayerIndex("Phase2"), 0);
    }

    private void Update()
    {
        fsm.Update();
        CheckLockedSkills();
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
        shadowsSpawned = false;
        meleeAttackRange = 5f;
        rangedAttackRange = 30f;
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
        Phase3State_Ransomware phase3State,
        DefeatedState_Ransomeware deadState
    ) CreateStates()
    {
        return (
            new IntroState_Ransomeware(this),
            new Phase1State_Ransomware(this),
            new Phase2State_Ransomeware(this),
            new Phase3State_Ransomware(this),
            new DefeatedState_Ransomeware(this)
        );
    }

    private void SetupTransitions((
        IntroState_Ransomeware introState,
        Phase1State_Ransomware phase1State,
        Phase2State_Ransomeware phase2State,
        Phase3State_Ransomware phase3State,
        DefeatedState_Ransomeware deadState) states)
    {
        fsm.AddTransition(new Transition<Ransomware>(
            states.introState,
            states.phase1State,
            () => true)
        );

        fsm.AddTransition(new Transition<Ransomware>(
            states.phase1State,
            states.phase2State,
            () => bossStatus.GetHealth() <= 0.5f * bossStatus.GetMaxHealth())
        );

        fsm.AddTransition(new Transition<Ransomware>(
            states.phase2State,
            states.phase3State,
            () => bossStatus.GetHealth() <= 0.1f * bossStatus.GetMaxHealth())
        );

        fsm.AddTransition(new Transition<Ransomware>(
            states.phase3State,
            states.deadState,
            () =>
            {
                if (!shadowsSpawned) return false; // �����찡 ���� �������� �ʾ����� ��ȯ���� ����

                bool allShadowsDestroyed = FindObjectsOfType<DigitalShadow>().Length == 0;
                return bossStatus.GetHealth() <= 0f || allShadowsDestroyed;
            }
        ));


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

    public void ApplyVulnerability(float duration)
    {
        StartCoroutine(VulnerabilityRoutine(duration));
    }

    private IEnumerator VulnerabilityRoutine(float duration)
    {
        // Apply visual effect to show vulnerability
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Material[] originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }


        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Restore normal state
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }


    }

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
        // ���� State�� ���� Interrupt ó��
        if (fsm.CurrentState is Phase1State_Ransomware phase1)
        {
            phase1.Interrupt();
        }
        else if (fsm.CurrentState is Phase2State_Ransomeware phase2)
        {
            phase2.Interrupt();
        }
        else if (fsm.CurrentState is Phase3State_Ransomware phase3)
        {
            phase3.Interrupt();
        }

        // �ִϸ��̼� ����
        ResetAllAnimationTriggers();

        // Ư�� ���ͷ�Ʈ ������ ���� �߰� ó��
        switch (reason)
        {
            case InterruptReason.PhaseTransition:
                SetRotationLock(true);
                NmAgent.isStopped = true;
                Animator.SetTrigger("PhaseTransition");
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

            case InterruptReason.ForcedInterrupt:
                SetRotationLock(true);
                NmAgent.isStopped = true;
                // ���� �ߴ� �� ���� ������ ����
                break;
        }
    }

    private void ResetAllAnimationTriggers()
    {
        // ���� ���� ���� Ʈ���� ����
        Animator.ResetTrigger("meleeAttack");
        Animator.ResetTrigger("RangedAttack");
        Animator.ResetTrigger("SpecialAttack");
        Animator.ResetTrigger("DataBlink");

        // ���� �������� �׼� �ߴ�
        Animator.SetTrigger("InterruptAction");
    }

    #endregion

    #region LockSystem
    public void LockPlayerSkill(SkillType skillType, float duration)
    {
        PlayerControl playerControl = GameObject.FindObjectOfType<PlayerControl>();
        if (playerControl != null)
        {
            // ��ų ����
            playerControl.SetSkillEnabled(skillType, false);

            // ���� �ð� ���� (���� �ð� + ���� �ð�)
            skillUnlockTimes[skillType] = Time.time + duration;
        }
    }
    private void CheckLockedSkills()
    {
        float currentTime = Time.time;
        List<SkillType> skillsToUnlock = new List<SkillType>();

        // ���� �ð��� �� ��ų Ȯ��
        foreach (var entry in skillUnlockTimes)
        {
            if (currentTime >= entry.Value)
            {
                skillsToUnlock.Add(entry.Key);
            }
        }

        // ���� �ð��� �� ��ų ����
        PlayerControl playerControl = GameObject.FindObjectOfType<PlayerControl>();
        if (playerControl != null && skillsToUnlock.Count > 0)
        {
            foreach (SkillType skill in skillsToUnlock)
            {
                playerControl.SetSkillEnabled(skill, true);
                skillUnlockTimes.Remove(skill);
            }
        }
    }

    #endregion

    #region WeaponMethod
    // ���� �ʱ�ȭ (Start�� InitializeComponents �޼��忡�� ȣ��)
    private void InitializeWeapons()
    {
        // �ʿ��� ���� ���� ������Ʈ���� ã�Ƽ� ����Ʈ�� �߰�
        weaponsList.Clear();
        BaseWeapon[] foundWeapons = GetComponentsInChildren<BaseWeapon>();
        foreach (var weapon in foundWeapons)
        {
            weaponsList.Add(weapon);
            weapon.DisableCollision(); // ���� �� ���� ���� �ݶ��̴� ��Ȱ��ȭ
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �޼�����
    public void EnableMeleeWeapon()
    {
        foreach (var weapon in weaponsList)
        {
            if (weapon is MeeleWeapon)
            {
                weapon.EnableCollision();
            }
        }
    }

    public void DisableMeleeWeapon()
    {
        foreach (var weapon in weaponsList)
        {
            if (weapon is MeeleWeapon)
            {
                weapon.DisableCollision();
            }
        }
    }
    #endregion

    // �������̽� �޼��� �߰�
    public float GetBaseDamage()
    {
        // �⺻ ������ �� ��ȯ
        return 20f; // �Ǵ� �ʿ��� �⺻ ������ ��
    }

    public float GetDamageMultiplier()
    {
        // ���� ����� ���¿� ���� ���� ��ȯ
        return fsm.CurrentState is Phase2State_Ransomeware ? 1.5f : 1.0f;
    }

    public bool IsInSpecialState()
    {
        // ������ Ư�� ������ ���� true ��ȯ
        return false; // �⺻���� false �Ǵ� ���� ����
    }

    #region Reset
    // ���� ���� �ʱ�ȭ �޼���
    public override void ResetBoss()
    {
        // ���� ������ �ʱ�ȭ
        ResetBossState();

        if (bossStatus != null)
        {
            bossStatus.SetHealth(bossStatus.GetMaxHealth());
        }

        ClearSummonedMonsters();

        // �ִϸ����� ���� �ʱ�ȭ
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        // NavMeshAgent �ʱ�ȭ
        if (nmAgent != null)
        {
            nmAgent.isStopped = false;
            nmAgent.ResetPath();
        }

        // FSM�� ó�� ���·� �ǵ�����
        ResetStateMachine();

    }

    private void ResetBossState()
    {
    }

    private void ClearSummonedMonsters()
    {
        foreach (GameObject monster in summonedObjects)
        {
            if (monster != null)
                Destroy(monster);
        }
        fsm.CurrentState.Interrupt();
    }

    private void ResetStateMachine()
    {
        if (fsm != null)
        {
            // FSM�� ���� �ʱ�ȭ
            InitializeFSM();
        }
    }

    #endregion



}
