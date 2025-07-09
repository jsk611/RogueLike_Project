using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.GraphicsBuffer;


public abstract class MonsterBase : MonoBehaviour
{
    #region Serialized Fields
    [Header("General Settings")]
    [SerializeField] protected Transform target;
    [SerializeField] private Transform body; // Character body (XZ rotation)
    [SerializeField] private Transform head; // Head or torso (vertical rotation)
    [SerializeField] private float maxVerticalAngle = 60f; // Maximum vertical angle for head rotation
    [SerializeField] protected float rotateSpeed = 2.0f; // Rotation speed

    public Summoner master = null;

    [Header("Components")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] protected MonsterStatus monsterStatus;
    [SerializeField] private Rigidbody playerRigidBody;


    [Header("Stats")]
    [SerializeField] protected float attackRange = 5.0f;
    [SerializeField] protected float attackCooldown = 3.0f;

    [Header("Effects")]
    [SerializeField] private GameObject splashFx;
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private Material startMaterial;
    [SerializeField] private Material baseMaterial;

    [SerializeField] private float height = 5f;


    [Header("Item Drop")]
    public bool summonedMonster = false;
    public bool dropDNA = true;
    public bool dropItem = true;
    [SerializeField] private GameObject[] items;
    [SerializeField] private int[] itemProbability = { 50, 25, 0 };
    [SerializeField] private int DNADrop = 0;

    [Header("UI")]
    [SerializeField] public EnemyHPBar HPBar;
    [SerializeField] private GameObject UIDamaged;

    [Header("Timings")]
    [SerializeField] private float hitCooldown = 1.0f;
    [SerializeField] private float hitDuration = 0.8f;
    [SerializeField] private float dieDuration = 0f;
    [SerializeField] private float transitionCooldown = 0.3f;
    [SerializeField] private float teleportCooldown = 4f;
    private float teleportTimer = 0f;

    [Header("External Data")]
    [SerializeField] private EnemyCountData enemyCountData;

    [Header("Effect")]
    [SerializeField] private GameObject binaryDeathEffectObject;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip monsterAttackSound;
    [SerializeField] private AudioClip monsterHitSound;
    [SerializeField] private AudioClip[] extraSounds;

    public AudioClip AttackSound => monsterAttackSound;
    public AudioClip HitSound => monsterHitSound;
    public AudioClip[] ExtraSounds => extraSounds;
    public NavMeshAgent NmAgent => nmAgent;
    #endregion

    #region Private Fields
    protected float hp;
    protected float dmg;
    protected float chaseSpeed;
    protected float attackTimer = 0f;
    protected float hitTimer = 0f;
    protected float dieTimer = 0f;
    protected float lastTransitionTime = 0f;
    protected bool isDie = false;
    protected WaveManager waveManager;
    #endregion

    #region State Management
    protected enum State
    {
        IDLE,
        CHASE,
        ATTACK,
        HIT,
        DIE,
        SEARCH,
        AIM,
        KILL,
        COOLDOWN,
        CAST,
    }

    [SerializeField]
    protected State state;
    protected Coroutine stateMachineCoroutine;
    protected Dictionary<State, Action> stateActions;
    #endregion

   

    protected virtual void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        waveManager = FindObjectOfType<WaveManager>();
        InitializeComponents();
        InitializeStateMachine();
        InitializeStats();
        InitializeSummon();
    }

    #region Initialization

    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();
    }

    private void InitializeStateMachine()
    {
        stateActions = new Dictionary<State, Action>
        {
            { State.IDLE, UpdateIdle },
            { State.CHASE, UpdateChase },
            { State.ATTACK, UpdateAttack },
            { State.HIT, UpdateHit },
            { State.DIE, UpdateDie },
        };
    }

    private void InitializeStats()
    {
        hp = monsterStatus.GetHealth();
        dmg = monsterStatus.GetAttackDamage();
        chaseSpeed = monsterStatus.GetMovementSpeed();

        float HPenforce = (8 * (waveManager.currentStage - 1) + waveManager.currentWave) * waveManager.HP_enforceRate;
        monsterStatus.SetMaxHealth(hp * HPenforce);
        monsterStatus.SetHealth(hp * HPenforce);
        float ATKenforce = (4 * (waveManager.currentStage - 1) + waveManager.currentWave) * waveManager.ATK_enforceRate;
        monsterStatus.SetAttackDamage(dmg * ATKenforce);

        HPBar.SetRatio(monsterStatus.GetHealth(),monsterStatus.GetMaxHealth());
    }

    public void InitializeSummon()
    {
        ChangeState(State.IDLE);
        StartCoroutine(SummonEffect());
    }
    #endregion



    protected virtual void Update()
    {
        // Debug.Log($"{name} current state = {state}");
        // if (state == State.IDLE) CheckPlayer();
        // ???? ???????? ???? ????
       // if ((state == State.CHASE || state == State.ATTACK)&&monsterStatus.currentCon != MonsterStatus.Condition.Frozen) RotateTowardsTarget();
        chaseSpeed = monsterStatus.GetMovementSpeed();
        ExecuteStateAction();
        Debug.Log(chaseSpeed);
    }

    private void LateUpdate()
    {
        // ???? ???????? ???? ????
        if ((state == State.CHASE || state == State.ATTACK) && monsterStatus.currentCon != MonsterStatus.Condition.Frozen) RotateTowardsTarget();
    }

    #region UpdateFunc
    private void ExecuteStateAction()
    {
        if (stateActions.TryGetValue(state, out var action))
        {
            action?.Invoke();
        }
        else
        {
            Debug.LogWarning($"State {state} does not have a defined action.");
        }
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
    }

    protected virtual void CheckPlayer()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
    }
    #endregion

    #region State Update Methods
    protected virtual void UpdateIdle()
    {
    }
    protected virtual void UpdateChase()
    {
        teleportTimer += Time.deltaTime;
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }
        if(teleportTimer >= teleportCooldown && Mathf.Abs(target.position.y-transform.position.y) >= 4f)
        {
            Debug.Log("enemy teleport stacked");
            EnemyTeleportManager.instance.GetEnemyToTeleport(this);
            teleportTimer = 0f;
        }

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;
        nmAgent.SetDestination(target.position);
        
        
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            teleportTimer = 0f;
            ChangeState(State.ATTACK);
        }
    }

    protected virtual void UpdateAttack()
    {
        nmAgent.isStopped = true;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            if (Vector3.Distance(transform.position, target.position) > attackRange)
            {
                ChangeState(State.CHASE);
                return;
            }

            attackTimer = 0f;
        }
    }

    protected virtual void UpdateHit()
    {
        nmAgent.isStopped = true;

        hitTimer += Time.deltaTime;
        if (hitTimer >= hitDuration)
        {
            ChangeState(State.CHASE);
            hitTimer = 0f;
        }
    }

    protected virtual void UpdateDie()
    {
        nmAgent.isStopped = true;

        dieTimer += Time.deltaTime;
        StartCoroutine(HandleDeath());
    }
    #endregion

    #region State Management
    protected void ChangeState(State newState)
    {
        if (newState != State.DIE && Time.time - lastTransitionTime < transitionCooldown) return;

        lastTransitionTime = Time.time;

        if (state != newState || newState == State.HIT)
        {
       //     Debug.Log($"{transform.name} state change: {state} ?? {newState}");
            SetAnimatorState(newState);
            state = newState;

            ResetStateTimers();
        }
    }

    private void ResetStateTimers()
    {
        switch (state)
        {
            case State.ATTACK:
                attackTimer = 0f;
                break;
            case State.HIT:
                hitTimer = 0f;
                break;
            case State.DIE:
                dieTimer = 0f;
                break;
        }
    }

    protected virtual void SetAnimatorState(State state)
    {
        if (anim == null) return;

        if (state == State.HIT)
        {
            anim.Play("GetHit", 0, 0f);
        }
        else
        {
            anim.SetInteger("State", (int)state);
        }
    }
    #endregion

    #region Damage and Death
    //public static event Action MonsterDamagedEvent;
    public virtual void TakeDamage(float damage, bool showDamage = true, bool flagForExecution = false)
    {
        if (state == State.DIE)
        {
            return;
        }

        EventManager.Instance.TriggerMonsterDamagedEvent();

        if(!flagForExecution) monsterStatus.DecreaseHealth(damage);
        hp = monsterStatus.GetHealth();

        if(showDamage) Instantiate(UIDamaged, transform.position + new Vector3(0,UnityEngine.Random.Range(0f,height/2),0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
       // HPBar?.SetRatio(hp, monsterStatus.GetMaxHealth());

        if (hp > 0)
        {
            ChangeState(State.HIT);
        }
        else if (state != State.DIE)
        {
            //ServiceLocator.Current.Get<IGameModeService>().GetKillingEffect().KillingSuccess();
            anim.SetTrigger("DieTrigger");
            ChangeState(State.DIE);
            if (!summonedMonster)
            {
                enemyCountData.enemyCount--;
            }
            EventManager.Instance.TriggerMonsterKilledEvent(!summonedMonster);

            GameObject effectInstance = Instantiate(binaryDeathEffectObject, transform.position, Quaternion.identity);
            BinaryDeathEffect effect = effectInstance.GetComponent<BinaryDeathEffect>();
            effect.TriggerDeathEffect(transform.position);

            DigitalStripeDissolveEffect.ApplyDeathEffect(this);

        }
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(dieDuration);
        transform.position = new Vector3(-100, -100, -100);
        if (summonedMonster)
        {
            dropDNA = false;
            dropItem = false;
            master?.summonDead(gameObject);
        }
        else
        { 
            Debug.LogWarning("?? ?????? --");
         //   if (dropItem) SpawnItem();
            if (dropDNA) target.GetComponent<PlayerStatus>().IncreaseCoin(DNADrop);
            //enemyCountData.enemyCount--;
        }

        Destroy(gameObject, 0.1f);
    }

    private void SpawnItem()
    {
        int randNum = UnityEngine.Random.Range(1, 101);
        int itemGrade = Array.FindIndex(itemProbability, prob => (randNum -= prob) <= 0);

        if (itemGrade >= 0)
        {
            Instantiate(items[itemGrade], transform.position + Vector3.up, Quaternion.identity);
        }
    }
    #endregion

    #region Summon Effect
    private IEnumerator SummonEffect()
    {
        nmAgent.speed = 0;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Queue<Material> materials = new Queue<Material>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == spawnEffect.GetComponentInChildren<Renderer>()) continue;
            materials.Enqueue(renderer.material);
            renderer.material = startMaterial;
        }
        float currentYPos = transform.position.y - height/2 - 1f;
        float maxYPos = transform.position.y + height/2 + 1f;
        while (currentYPos < maxYPos)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == spawnEffect.GetComponentInChildren<Renderer>()) continue;
                renderer.material.SetFloat("_CustomTime", currentYPos);
            }
            currentYPos += Time.deltaTime * (height+2) / 2.2f;
            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer == spawnEffect.GetComponentInChildren<Renderer>()) continue;
            renderer.material = materials.Dequeue();
        }

        spawnEffect.SetActive(false);
        ChangeState(State.CHASE);
    }
    #endregion

    //CC?? ???? ?? state ????????? ??????
    public void UpdateStateFromAnimationEvent()
    {
       // ChangeState(State.CHASE);
    }
    public float GetRange() => attackRange;
    
    public void ChangeStateToIdle()
    {
        ChangeState(State.IDLE);
    }
}
