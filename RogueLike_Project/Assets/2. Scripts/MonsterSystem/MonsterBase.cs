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
    [SerializeField] private GameObject[] items;
    [SerializeField] private int[] itemProbability = { 50, 25, 0 };
    [SerializeField] private float height = 5f;

    [Header("UI")]
    [SerializeField] public EnemyHPBar HPBar;
    [SerializeField] private GameObject UIDamaged;

    [Header("Timings")]
    [SerializeField] private float hitCooldown = 1.0f;
    [SerializeField] private float hitDuration = 0.8f;
    [SerializeField] private float dieDuration = 1f;
    [SerializeField] private float transitionCooldown = 0.3f;

    [Header("External Data")]
    [SerializeField] private EnemyCountData enemyCountData;
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
    }

    protected State state;
    protected Coroutine stateMachineCoroutine;
    private Dictionary<State, Action> stateActions;
    #endregion

    protected virtual void Start()
    {
        InitializeComponents();
        InitializeStateMachine();
        InitializeStats();

        state = State.IDLE;
        StartCoroutine(SummonEffect());
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
    }
    #endregion

    private void Update()
    {
        //chaseSpeed를 실시간으로 반영
        chaseSpeed = monsterStatus.GetMovementSpeed();
        Debug.Log($"{name} current state = {state}");
        if (state == State.IDLE) CheckPlayer();
        // 빙결 상태에서 회전 불가
        if ((state == State.CHASE || state == State.ATTACK)&&monsterStatus.currentCon != MonsterStatus.Condition.Frozen) RotateTowardsTarget();
        ExecuteStateAction();
    }

    private void LateUpdate()
    {
        // 빙결 상태에서 회전 불가
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
    protected virtual void UpdateIdle() => ChangeState(State.CHASE);

    protected virtual void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;
        nmAgent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
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

    protected void UpdateDie()
    {
        nmAgent.isStopped = true;

        dieTimer += Time.deltaTime;
        if (dieTimer >= dieDuration)
        {
            HandleDeath();
        }
    }
    #endregion

    #region State Management
    protected void ChangeState(State newState)
    {
        if (Time.time - lastTransitionTime < transitionCooldown) return;

        lastTransitionTime = Time.time;

        if (state != newState || newState == State.HIT)
        {
            Debug.Log($"{transform.name} state change: {state} → {newState}");
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
        else if (state == State.DIE)
        {
            anim.SetTrigger("DIE");
        }
        else
        {
            anim.SetInteger("State", (int)state);
        }
    }
    #endregion

    #region Damage and Death
    public virtual void TakeDamage(float damage)
    {
        if (state == State.DIE) return;
       
        monsterStatus.DecreaseHealth(damage);
        hp = monsterStatus.GetHealth();

        Instantiate(UIDamaged, transform.position + new Vector3(0,UnityEngine.Random.Range(0f,height/2),0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
       // HPBar?.SetRatio(hp, monsterStatus.GetMaxHealth());

        if (hp > 0)
        {
            ChangeState(State.HIT);
        }
        else
        {
            anim.SetTrigger("DieTrigger");
            ChangeState(State.DIE);
        }
    }

    private void HandleDeath()
    {
        enemyCountData.enemyCount--;
        SpawnItem();
        Destroy(gameObject);
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

            currentYPos += Time.deltaTime * (height+2)/2.5f;
            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer == spawnEffect.GetComponentInChildren<Renderer>()) continue;
            renderer.material = materials.Dequeue();
        }

        spawnEffect.SetActive(false);
    }
    #endregion

    //CC기 적용 후 state 초기화?용 메소드
    public void UpdateStateFromAnimationEvent()
    {
       // ChangeState(State.CHASE);
    }
}
