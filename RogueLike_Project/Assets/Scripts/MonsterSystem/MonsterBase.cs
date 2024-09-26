using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class MonsterBase : MonoBehaviour, ICombatant
{
    [Header("NormalStats Fields")]
    protected float hp;
    protected float def;
    [SerializeField] protected Transform target;

    [Header("Preset Fields")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected NavMeshAgent nmAgent;


    [Header("Delay(CoolTime)")]
    [SerializeField] protected float transitionDelay;

    protected MonsterStatus monsterStatus;

    protected Coroutine stateMachineCoroutine;

    protected enum State
    {
        IDLE,
        SEARCH,
        ATTACK,
        CHASE,
        AIMING,
        SHOT,
        KILL,
    }

    protected State state;

    [SerializeField] EnemyCountData enemyCountData;
    bool isDie = false;
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();

        hp = monsterStatus.GetHealth(); // 기본 체력
        //def = monsterStatus.GetDefence();

        state = State.IDLE;
        stateMachineCoroutine = StartCoroutine(StateMachine());
    }

    protected abstract IEnumerator StateMachine();

    public virtual void TakeDamage(float damage)
    {
        monsterStatus.DecreaseHealth(damage);
        hp = monsterStatus.GetHealth();

        if (hp > 0)
        {
            ChangeState(State.CHASE);
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        else
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
        // 적이 사망하면 수행할 동작 (예: 애니메이션 재생, 오브젝트 비활성화 등)
        if (!isDie)
        {
            enemyCountData.enemyCount--;
            Debug.Log("Enemy Died, 남은 적 : " + enemyCountData.enemyCount);
            isDie = true;
        }
        Destroy(gameObject);
    }

    protected void ChangeState(State newState)
    {
        state = newState;
    }

    protected IEnumerator Crowd_Control(Transform target)
    {
        target.GetComponent<PlayerControl>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        target.GetComponent <PlayerControl>().enabled = true;
    }
}
