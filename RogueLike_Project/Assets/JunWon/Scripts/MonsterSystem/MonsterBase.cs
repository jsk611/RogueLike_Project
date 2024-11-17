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
    [SerializeField] protected Transform target;

    [Header("Preset Fields")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] private Rigidbody playerRigidBody;


    [Header("NormalStats Fields")]
    [SerializeField] protected MonsterStatus monsterStatus;
    [SerializeField] protected float attackRange = 5.0f; // 공격 범위
    [SerializeField] protected float attackCooldown = 3.0f; // 공격 간격
    protected float attackTimer = 0.0f; // 공격 타이머
    protected float hp = 0; // 기본 체력
    protected float dmg = 0; // 기본 데미지
    protected float chaseSpeed; // 추적 속도
    protected float rotateSpeed = 2.0f; // 회전 속도


    [Header("Delay(CoolTime)")]
    [SerializeField] protected float transitionDelay;



    [Header("HitVariable")]
    [SerializeField] private float hitCooldown = 1.0f; // 피격 쿨타임 (초 단위)
    private float lastHitTime = 0.0f; // 마지막으로 피격된 시간
    private float hitTimer = 0f;
    private float hitDuration = 0.8f; // 피격 애니메이션 길이

    [Header("DieVariable")]
    private float dieTimer = 0f;
    private float dieDuration = 5.0f; // 죽음 애니메이션 길이


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
    protected Coroutine stateMachineCoroutine;
    private Dictionary<State, Action> stateActions;


    [SerializeField] EnemyCountData enemyCountData;
    bool isDie = false;
    protected virtual void Start()
    {
        stateActions = new Dictionary<State, Action> 
        {

            { State.IDLE, UpdateIdle },
            { State.CHASE, UpdateChase },
            { State.ATTACK, UpdateAttack },
            { State.HIT, UpdateHit },
            { State.DIE, UpdateDie },
            { State.AIM, UpdateAim },

        };

        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();


        hp = monsterStatus.GetHealth(); // 기본 체력
        dmg = monsterStatus.GetAttackDamage(); // 기본 공격력
        chaseSpeed = monsterStatus.GetMovementSpeed(); // 기본 이동 속도
        // attackRange = monsterStatus.GetAttackRange(); // 기본 공격 범위

        //def = monsterStatus.GetDefence();

        state = State.IDLE;
        // stateMachineCoroutine = StartCoroutine(StateMachine());
    } // 기본 몬스터 세팅

    // 코루틴 스타일
    //#region Basic Monster Function
    //protected virtual IEnumerator StateMachine()
    //{
    //    while (hp > 0)
    //    {
    //        Debug.Log(name + "의 현재 state : " + state);
    //        yield return StartCoroutine(GetStateCoroutine(state));
    //    }
    //}

    //private IEnumerator GetStateCoroutine(State state)
    //{
    //    switch (state)
    //    {
    //        case State.IDLE:
    //            return IDLE();
    //        case State.CHASE:
    //            return CHASE();
    //        case State.AIM:
    //            return AIM();
    //        case State.ATTACK:
    //            return ATTACK();
    //        case State.HIT:
    //            return HIT();
    //        case State.DIE:
    //            return DIE();
    //        case State.SEARCH:
    //            return SEARCH();
    //        case State.KILL:
    //            return KILL();
    //        default:
    //            return null;
    //    }
    //}

    //protected virtual IEnumerator IDLE()
    //{
    //    SetAnimatorState(state);// 상태에 따라 애니메이터 파라미터 설정

    //    if (fov.visibleTargets.Count > 0)
    //    {
    //        target = fov.visibleTargets[0];
    //        ChangeState(State.CHASE);
    //    }
    //    else
    //    {
    //        target = null;
    //    }

    //    yield return new WaitForSeconds(0.3f);
    //}

    //protected virtual IEnumerator CHASE()
    //{
    //    SetAnimatorState(state); // 상태에 따라 애니메이터 파라미터 설정

    //    if (target == null)
    //    {
    //        ChangeState(State.IDLE);
    //        yield break;
    //    }

    //    // 최소한의 시간 동안 CHASE 상태를 유지
    //    yield return new WaitForSeconds(0.5f);

    //    nmAgent.isStopped = false;
    //    nmAgent.speed = chaseSpeed; // 이전에 계산된 chaseSpeed 사용
    //    nmAgent.SetDestination(target.position);

    //    if (!nmAgent.pathPending && nmAgent.remainingDistance <= attackRange)
    //    {
    //        if (this is RangedMonster)
    //            ChangeState(State.AIM);
    //        else
    //            ChangeState(State.ATTACK);
    //    }

    //    yield return new WaitForSeconds(0.3f);
    //}

    //protected virtual IEnumerator ATTACK()
    //{
    //    SetAnimatorState(state); // 상태에 따라 애니메이터 파라미터 설정

    //    nmAgent.isStopped = true; // 공격 시 이동 중지

    //    if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
    //    {
    //        ChangeState(State.CHASE);
    //        yield break;
    //    }

    //    yield return null; // 공격 쿨타임 대기

    //    //if (target != null && Vector3.Distance(target.transform.position, transform.position) <= attackRange + 1)
    //    //{
    //    //    //target.GetComponent<PlayerStatus>().DecreaseHealth(dmg * monsterStatus.CalculateCriticalHit());
    //    //    //StartCoroutine(Crowd_Control(target));
    //    //    //StartCoroutine(KnockBack());
    //    //}
    //}

    //protected virtual IEnumerator HIT()
    //{
    //    // 이동 및 공격 중지
    //    nmAgent.isStopped = true;

    //    // 피격 애니메이션 재생
    //    SetAnimatorState(state);

    //    // 이동 재개
    //    nmAgent.isStopped = false;

    //    // 원하는 상태로 전환 (예: 추적 상태)
    //    ChangeState(State.CHASE);

    //    yield return new WaitForSeconds(GetAnimationClipLength("Hit"));
    //}

    ////protected virtual IEnumerator KnockBack()
    ////{
    ////    playerRigidBody.isKinematic = false;
    ////    playerRigidBody.AddForce((Vector3.up + new Vector3((target.transform.position.x - transform.position.x), 0, (target.transform.position.z - transform.position.z)).normalized) * 10f, ForceMode.Impulse);
    ////    yield return new WaitForSeconds(1f);
    ////}


    //protected virtual IEnumerator AIM()
    //{
    //    if (target == null) yield break;

    //    SetAnimatorState(state); // 상태에 따라 애니메이터 파라미터 설정
    //    nmAgent.isStopped = true;

    //    ChangeState(State.ATTACK);
    //    yield return new WaitForSeconds(0.3f);
    //}

    //protected virtual IEnumerator SEARCH() { yield break; }
    //protected virtual IEnumerator KILL() { yield break; }


    //public virtual void TakeDamage(float damage)
    //{
    //    // 체력 감소 처리
    //    monsterStatus.DecreaseHealth(damage);
    //    hp = monsterStatus.GetHealth();

    //    if (hp > 0)
    //    {
    //        // 피격 상태로 전환
    //        ChangeState(State.HIT);
    //        target = FindObjectOfType<PlayerStatus>().transform;
    //    }
    //    else
    //    {
    //        // 죽음 처리
    //        isDie = true; // 죽음 플래그 설정

    //        // 애니메이터 파라미터 설정
    //        if (anim != null)
    //        {
    //            anim.SetBool("isDead", true); // 또는 anim.SetTrigger("DieTrigger");
    //        }

    //        StartCoroutine(DIE());
    //    }
    //}

    //protected virtual IEnumerator DIE()
    //{
    //    // 이동 및 공격 중지
    //    if (stateMachineCoroutine != null)
    //    {
    //        StopCoroutine(stateMachineCoroutine);
    //    }
    //    nmAgent.isStopped = true;

    //    // 콜라이더 비활성화
    //    Collider collider = GetComponent<Collider>();
    //    if (collider != null)
    //    {
    //        collider.enabled = false;
    //    }

    //    // 적 카운트 감소 (한 번만 실행)
    //    if (!isDie)
    //    {
    //        enemyCountData.enemyCount--;
    //        Debug.Log("Enemy Died, 남은 적 : " + enemyCountData.enemyCount);
    //        isDie = true;
    //    }

    //    // 죽음 애니메이션의 길이만큼 대기
    //    // float deathAnimationLength = GetAnimationClipLength("Die"); // 또는 "Death"
    //    yield return new WaitForSeconds(1.5f);

    //    // 오브젝트 비활성화 또는 파괴
    //    Destroy(gameObject);
    //}

    //#endregion

    //protected void ChangeState(State newState)
    //{
    //    // 죽은 경우 상태 전환하지 않음
    //    if (isDie) return;

    //    Debug.Log(transform.name + " 상태 변경: " + state + " → " + newState);
    //    state = newState;
    //}

    #region animationsettings
    protected void SetAnimatorState(State state)
    {
        if (anim != null)
        {
            if (state == State.HIT)
            {
                anim.Play("GetHit", 0, 0f); // 트리거를 사용해 애니메이션 강제 재생
            }
            else
            {
                anim.SetInteger("State", (int)state); // 다른 상태는 Integer로 처리
            }
        }
    }
    protected float GetAnimationClipLength(string clipname)
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == clipname)
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


    protected virtual void Update()
    {
        Debug.Log(name + " current state = " + state);
        Debug.Log(attackTimer + " attack Timer = " + attackTimer);
        if (state == State.IDLE) CheckPlayer();
        if (state == State.AIM || state == State.CHASE || state == State.ATTACK)
        {
            RotateTowardsTarget();
        }
        PlayAction(state);
    }

    private void PlayAction(State state)
    {
        if (stateActions.TryGetValue(state, out var action))
        {
            action?.Invoke();
        }
        else
        {
            Debug.LogWarning($"State {state}에 대한 액션이 정의되지 않았습니다.");
        }
    }

    private void CheckPlayer()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            if (state != State.ATTACK || state != State.HIT) ChangeState(State.CHASE);
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
    }
    protected virtual void UpdateIdle()
    {
    }

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
            if (this is RangedMonster)
                ChangeState(State.AIM);
            else
                ChangeState(State.ATTACK);
        }
    }

    protected virtual void UpdateAttack()
    {

        nmAgent.isStopped = true;
        nmAgent.speed = 0f;

        // 공격 타이머 진행
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown) 
        {
            // 공격 후 타겟이 범위를 벗어났다면 추적 상태로 전환
            if (Vector3.Distance(transform.position, target.position) > attackRange) 
                ChangeState(State.CHASE);

            // 공격 타이머 초기화
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
            // 오브젝트 파괴 또는 비활성화
            Destroy(gameObject);
        }
    }

    protected void UpdateAim()
    {
         nmAgent.isStopped = true;
         ChangeState(State.ATTACK);
    }

    protected void ChangeState(State newState)
    {
        if (state != newState || newState == State.HIT)
        {
            Debug.Log(transform.name + " 상태 변경: " + state + " → " + newState);
            SetAnimatorState(newState);
            state = newState;

            // 상태별 초기화
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
    }

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
            ChangeState(State.DIE);
            dieTimer = 0.0f;
        }
    }

}
