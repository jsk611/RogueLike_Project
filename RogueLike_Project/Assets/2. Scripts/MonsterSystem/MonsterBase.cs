using DG.Tweening;
using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.GraphicsBuffer;
using static WaveManager;


public abstract class MonsterBase : MonoBehaviour
{
    #region Serialized Fields
    [Header("General Settings")]
    [SerializeField] protected Transform target;
    [SerializeField] private Transform body; // Character body (XZ rotation)
    [SerializeField] private Transform head; // Head or torso (vertical rotation)
   // [SerializeField] private float maxVerticalAngle = 60f; // Maximum vertical angle for head rotation
    [SerializeField] protected float rotateSpeed = 2.0f; // Rotation speed
    [SerializeField] protected float maxSpeedRange = 10f;
    [SerializeField] protected float minSpeedRange = 6f;

    public Summoner master = null;

    [Header("Components")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] protected MonsterStatus monsterStatus;
    [SerializeField] private Rigidbody playerRigidBody;
    
    [Header("Adaptive Aggression System")]
    [SerializeField] private bool useAdaptiveAggression = true; // 적응형 적극성 시스템 사용 여부
    protected OptimizedAggressionSystem optimizedAggressionSystem;


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
    public GameObject UIDamaged;

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
        
        // 적응형 적극성 시스템 초기화 (최적화된 버전만 사용)
        if (useAdaptiveAggression)
        {
            optimizedAggressionSystem = GetComponent<OptimizedAggressionSystem>();
            if (optimizedAggressionSystem == null)
            {
                optimizedAggressionSystem = gameObject.AddComponent<OptimizedAggressionSystem>();
            }
        }
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

        float HPenforce = 1 + waveManager.monsterEnforceVar * waveManager.HP_enforceRate;
        float ATKenforce = 1 + waveManager.monsterEnforceVar * waveManager.ATK_enforceRate;
        monsterStatus.SetMaxHealth(hp * HPenforce);
        monsterStatus.SetHealth(hp * HPenforce);
        monsterStatus.SetAttackDamage(dmg * ATKenforce);

        WaveRandomEnforce.EnemyWaveEnforce t = WaveRandomEnforce.enemyBuff;
        switch (t)
        {
            case WaveRandomEnforce.EnemyWaveEnforce.healthEnforce:
                hp = monsterStatus.GetHealth();
                monsterStatus.SetMaxHealth(hp * (1 + WaveRandomEnforce.enemyBuffVal[t]));
                monsterStatus.SetHealth(hp * (1 + WaveRandomEnforce.enemyBuffVal[t]));
                break;
            case WaveRandomEnforce.EnemyWaveEnforce.attackEnforce:
                monsterStatus.SetAttackDamage(monsterStatus.GetAttackDamage() * (1 + WaveRandomEnforce.enemyBuffVal[t]));
                break;
            case WaveRandomEnforce.EnemyWaveEnforce.speedEnforce:
                monsterStatus.SetMovementSpeed(monsterStatus.GetMovementSpeed() * (1 + WaveRandomEnforce.enemyBuffVal[t]));
                break;
            case WaveRandomEnforce.EnemyWaveEnforce.none:
                break;
        }

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
        
        // 적응형 적극성 시스템이 활성화된 경우 OptimizedAggressionSystem에서 처리
        if (!useAdaptiveAggression || optimizedAggressionSystem == null)
        {
            // 기존 거리 기반 속도 조절 로직
            float distance = Vector3.Distance(transform.position, target.position);
            distance = Mathf.Clamp(distance, minSpeedRange, maxSpeedRange);
            chaseSpeed = monsterStatus.GetMovementSpeed();
            chaseSpeed *= distance / minSpeedRange;
        }
        else
        {
            // 최적화된 시스템에서 속도가 이미 조절되므로 현재 NavMeshAgent 속도를 사용
            chaseSpeed = nmAgent.speed;
        }
       
        ExecuteStateAction();
        
        // 디버그 로그를 적극성 시스템 사용 시에만 표시 (성능을 위해 주석 처리)
        // if (useAdaptiveAggression && optimizedAggressionSystem != null)
        // {
        //     Debug.Log($"{name} - Speed: {chaseSpeed:F1}, Aggression: {optimizedAggressionSystem.GetAggressionLevel():F2}");
        // }
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
        if (fov.VisibleTargets.Count > 0)
        {
            target = fov.VisibleTargets[0];
            ChangeState(State.CHASE);
        }
    }
    #endregion

    protected virtual bool DetectedPlayer()
    {
        return fov != null && fov.VisibleTargets.Count > 0;
    }


    #region State Update Methods
    protected virtual void UpdateIdle()
    {
        // 플레이어를 감지하면 추적 상태로 전환
        CheckPlayer();
        
        // 적응형 적극성 시스템이 활성화된 경우 거리에 따라 자동으로 추적 시작
        if (useAdaptiveAggression && target != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            // 일정 거리 내에 플레이어가 있으면 자동으로 추적 시작 (기본 15m)
            if (distanceToPlayer <= 15f)
            {
                ChangeState(State.CHASE);
            }
        }
        // 적응형 시스템이 비활성화된 경우 FieldOfView 기반 감지만 사용
        else if (!useAdaptiveAggression && fov.VisibleTargets.Count > 0)
        {
            ChangeState(State.CHASE);
        }
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

        // NavMeshAgent 상태 확인 및 설정
        if (nmAgent == null)
        {
            Debug.LogError($"{name}: NavMeshAgent is null!");
            return;
        }
        
        if (!nmAgent.enabled)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is disabled!");
            nmAgent.enabled = true;
        }
        
        if (!nmAgent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is not on NavMesh!");
            return;
        }
        
        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;
        
        // 적응형 적극성 시스템 사용 시 예측 이동 고려
        Vector3 targetDestination = target.position;
        if (useAdaptiveAggression && optimizedAggressionSystem != null && optimizedAggressionSystem.IsAggressive())
        {
            Vector3 predictedPos = optimizedAggressionSystem.GetPredictedPlayerPosition();
            float confidence = optimizedAggressionSystem.GetPredictionConfidence();
            
            // 예측 조건을 완화하여 더 자주 발동되도록 수정
            if (confidence > 0.4f && optimizedAggressionSystem.GetAggressionLevel() > 0.3f && predictedPos != Vector3.zero)
            {
                targetDestination = predictedPos;
                // 디버그 로그 제거 (성능 향상)
                // Debug.Log($"{name}: Using predicted position with confidence {confidence:F2}");
            }
        }
        
        // 목표 설정 (디버그 로그 제거로 성능 향상)
        nmAgent.SetDestination(targetDestination);
        
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
        
        // 적극성에 따른 공격 쿨다운 조절
        float effectiveAttackCooldown = attackCooldown;
        if (useAdaptiveAggression && optimizedAggressionSystem != null)
        {
            float aggressionLevel = optimizedAggressionSystem.GetAggressionLevel();
            // 적극성이 높을수록 공격 쿨다운 감소 (최대 30% 감소)
            effectiveAttackCooldown *= (1f - aggressionLevel * 0.3f);
        }
        
        if (attackTimer >= effectiveAttackCooldown)
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
        if (gameObject == null) return;
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
        GameController.Instance.OnMonsterKilled();

        yield return new WaitForSeconds(dieDuration);
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
            if (dropDNA)
            {
                target.GetComponent<PlayerStatus>().IncreaseCoin(DNADrop);
                dropDNA = false;
            }
            
        }
        GetComponent<NavMeshAgent>().enabled = false;
        transform.position = new Vector3(-100, -100, -100);
        yield return new WaitForEndOfFrame();
        Destroy(gameObject, 0.05f);
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

    public void ChangeConditionMaterial(MonsterStatus.Condition condition)
    {
        Material conMaterial = EnemyShader.instance.monsterNormal;
        switch (condition)
        {
            case StatusBehaviour.Condition.normal: conMaterial = EnemyShader.instance.monsterNormal; break;
            case StatusBehaviour.Condition.Blazed: conMaterial = EnemyShader.instance.monsterBlazed; break;
            case StatusBehaviour.Condition.Frozen: conMaterial = EnemyShader.instance.monsterFrozen; break;
            case StatusBehaviour.Condition.Shocked: conMaterial = EnemyShader.instance.monsterShocked; break;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == spawnEffect.GetComponentInChildren<Renderer>()) continue;
            renderer.material = conMaterial;
        }

    }
    //CC?? ???? ?? state ????????? ??????
    public void UpdateStateFromAnimationEvent()
    {
       // ChangeState(State.CHASE);
    }
    public float GetRange() => attackRange;
    
    /// <summary>
    /// 최적화된 적극성 시스템 참조 반환
    /// </summary>
    public OptimizedAggressionSystem GetOptimizedAggressionSystem()
    {
        return optimizedAggressionSystem;
    }
    
    /// <summary>
    /// 현재 적극성 수준 반환
    /// </summary>
    public float GetCurrentAggressionLevel()
    {
        return useAdaptiveAggression && optimizedAggressionSystem != null ? 
               optimizedAggressionSystem.GetAggressionLevel() : 0f;
    }
    
    /// <summary>
    /// 적극적 상태인지 확인
    /// </summary>
    public bool IsCurrentlyAggressive()
    {
        return useAdaptiveAggression && optimizedAggressionSystem != null && 
               optimizedAggressionSystem.IsAggressive();
    }
    
    public void ChangeStateToIdle()
    {
        ChangeState(State.IDLE);
    }
}
