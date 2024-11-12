using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class MonsterBase : MonoBehaviour
{
    [SerializeField] protected Transform target;

    [Header("Preset Fields")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] private Rigidbody playerRigidBody;


    [Header("NormalStats Fields")]
    [SerializeField] protected MonsterStatus monsterStatus;
    protected float attackRange = 1.0f; // 공격 범위
    protected float detectionRange = 1.0f; // 감지 범위
    protected float attackCooldown = 1.0f; // 공격 간격
    protected float hp = 0; // 기본 체력
    protected float dmg = 0; // 기본 데미지
    protected float chaseSpeed; // 추적 속도


    [Header("Delay(CoolTime)")]
    [SerializeField] protected float transitionDelay;

    protected Coroutine stateMachineCoroutine;

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
    }

    protected State state;

    [SerializeField] EnemyCountData enemyCountData;
    bool isDie = false;
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();


        hp = monsterStatus.GetHealth(); // 기본 체력
        dmg = monsterStatus.GetAttackDamage(); // 기본 공격력
        chaseSpeed = monsterStatus.GetMovementSpeed(); // 기본 이동 속도
        attackRange = monsterStatus.GetAttackRange(); // 기본 공격 범위

        //def = monsterStatus.GetDefence();

        state = State.IDLE;
        stateMachineCoroutine = StartCoroutine(StateMachine());
    } // 기본 몬스터 세팅


    #region Basic Monster Function
    protected virtual IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            Debug.Log(name + "의 현재 state : " + state);
            yield return StartCoroutine(GetStateCoroutine(state));
        }
    }

    private IEnumerator GetStateCoroutine(State state)
    {
        switch (state)
        {
            case State.IDLE:
                return IDLE();
            case State.CHASE:
                return CHASE();
            case State.AIM:
                return AIM();
            case State.ATTACK:
                return ATTACK();
            case State.HIT:
                return HIT();
            case State.DIE:
                return DIE();
            case State.SEARCH:
                return SEARCH();
            case State.KILL:
                return KILL();
            default:
                return null;
        }
    }

    protected virtual IEnumerator IDLE()
    {
        SetAnimatorState(state);// 상태에 따라 애니메이터 파라미터 설정

        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return new WaitForSeconds(0.3f);
    }

    protected virtual IEnumerator CHASE()
    {
        SetAnimatorState(state); // 상태에 따라 애니메이터 파라미터 설정

        if (target == null)
        {
            ChangeState(State.IDLE);
            yield break;
        }

        // 최소한의 시간 동안 CHASE 상태를 유지
        yield return new WaitForSeconds(0.5f);

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed; // 이전에 계산된 chaseSpeed 사용
        nmAgent.SetDestination(target.position);

        if (!nmAgent.pathPending && nmAgent.remainingDistance <= attackRange)
        {
            if (this is RangedMonster)
                ChangeState(State.AIM);
            else
                ChangeState(State.ATTACK);
        }

        yield return new WaitForSeconds(0.3f);
    }

    protected virtual IEnumerator ATTACK()
    {
        SetAnimatorState(state); // 상태에 따라 애니메이터 파라미터 설정

        nmAgent.isStopped = true; // 공격 시 이동 중지

        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
            yield break;
        }

        yield return null; // 공격 쿨타임 대기

        //if (target != null && Vector3.Distance(target.transform.position, transform.position) <= attackRange + 1)
        //{
        //    //target.GetComponent<PlayerStatus>().DecreaseHealth(dmg * monsterStatus.CalculateCriticalHit());
        //    //StartCoroutine(Crowd_Control(target));
        //    //StartCoroutine(KnockBack());
        //}
    }

    protected virtual IEnumerator HIT()
    {
        // 이동 및 공격 중지
        nmAgent.isStopped = true;

        // 피격 애니메이션 재생
        SetAnimatorState(state);

        // 이동 재개
        nmAgent.isStopped = false;

        // 원하는 상태로 전환 (예: 추적 상태)
        ChangeState(State.CHASE);

        yield return new WaitForSeconds(GetAnimationClipLength("Hit"));
    }

    //protected virtual IEnumerator KnockBack()
    //{
    //    playerRigidBody.isKinematic = false;
    //    playerRigidBody.AddForce((Vector3.up + new Vector3((target.transform.position.x - transform.position.x), 0, (target.transform.position.z - transform.position.z)).normalized) * 10f, ForceMode.Impulse);
    //    yield return new WaitForSeconds(1f);
    //}


    protected virtual IEnumerator AIM()
    {
        if (target == null) yield break;

        SetAnimatorState(state); // 상태에 따라 애니메이터 파라미터 설정
        nmAgent.isStopped = true;

        ChangeState(State.ATTACK);
        yield return new WaitForSeconds(0.3f);
    }

    protected virtual IEnumerator SEARCH() { yield break; }
    protected virtual IEnumerator KILL() { yield break; }


    public virtual void TakeDamage(float damage)
    {
        // 체력 감소 처리
        monsterStatus.DecreaseHealth(damage);
        hp = monsterStatus.GetHealth();

        if (hp > 0)
        {
            // 피격 상태로 전환
            ChangeState(State.HIT);
            target = FindObjectOfType<PlayerStatus>().transform;
        }
        else
        {
            // 죽음 처리
            isDie = true; // 죽음 플래그 설정

            // 애니메이터 파라미터 설정
            if (anim != null)
            {
                anim.SetBool("isDead", true); // 또는 anim.SetTrigger("DieTrigger");
            }

            StartCoroutine(DIE());
        }
    }

    protected virtual IEnumerator DIE()
    {
        // 이동 및 공격 중지
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
        nmAgent.isStopped = true;

        // 콜라이더 비활성화
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 적 카운트 감소 (한 번만 실행)
        if (!isDie)
        {
            enemyCountData.enemyCount--;
            Debug.Log("Enemy Died, 남은 적 : " + enemyCountData.enemyCount);
            isDie = true;
        }

        // 죽음 애니메이션의 길이만큼 대기
        // float deathAnimationLength = GetAnimationClipLength("Die"); // 또는 "Death"
        yield return new WaitForSeconds(1.5f);

        // 오브젝트 비활성화 또는 파괴
        Destroy(gameObject);
    }

    #endregion

    protected void ChangeState(State newState)
    {
        // 죽은 경우 상태 전환하지 않음
        if (isDie) return;

        Debug.Log(transform.name + " 상태 변경: " + state + " → " + newState);
        state = newState;
    }

    #region AnimationSettings
    protected void SetAnimatorState(State state)
    {
        if (anim != null)
        {
            anim.SetInteger("State", (int)state);
        }
    }
    protected float GetAnimationClipLength(string clipName)
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == clipName)
                {
                    return clip.length;
                }
            }
        }
        // 기본값 설정 (애니메이션 클립을 찾지 못한 경우)
        return 1.0f; // 필요에 따라 조정
    }
    #endregion



    //protected IEnumerator Crowd_Control(Transform target)
    //{
    //    target.GetComponent<PlayerControl>().enabled = false;
    //    yield return new WaitForSeconds(0.5f);
    //    target.GetComponent <PlayerControl>().enabled = true;
    //}
}
