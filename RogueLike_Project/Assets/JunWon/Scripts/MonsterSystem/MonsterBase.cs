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
    [SerializeField] private Transform body;        // 캐릭터 몸체 (XZ 회전)
    [SerializeField] private Transform head;        // 머리 또는 상체 (상하좌우 회전)
    [SerializeField] private float maxVerticalAngle = 60f; // 머리가 위/아래로 회전 가능한 최대 각도
    protected float rotateSpeed = 2.0f; // 회전 속도



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
    protected float attackTimer = 0.5f; // 공격 타이머
    protected float hp = 0; // 기본 체력
    protected float dmg = 0; // 기본 데미지
    protected float chaseSpeed; // 추적 속도


    [Header("Delay(CoolTime)")]
    private float lastTransitionTime = 0f;
    private float transitionCooldown = 0.3f;



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
        COOLDOWN,
    }


    protected State state;
    protected Coroutine stateMachineCoroutine;
    private Dictionary<State, Action> stateActions;
    private Dictionary<State, float> stateDurations;


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
        };

        stateDurations = new Dictionary<State, float>
        {
            { State.IDLE, 0.3f },
            { State.CHASE, 0f }, // 타이머가 필요 없으면 0으로 설정
            { State.ATTACK, 1.0f }, // 애니메이션 길이에 맞게 설정
            { State.HIT, 0.8f }, // Hit 애니메이션 길이
            { State.DIE, 5.0f }, // 죽음 애니메이션 길이
        };

        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();


        hp = monsterStatus.GetHealth(); // 기본 체력
        dmg = monsterStatus.GetAttackDamage(); // 기본 공격력
        chaseSpeed = monsterStatus.GetMovementSpeed(); // 기본 이동 속도
        // attackRange = monsterStatus.GetAttackRange(); // 기본 공격 범위


        state = State.IDLE;
    } // 기본 몬스터 세팅

  

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
        if (state == State.IDLE) CheckPlayer();
        if (state == State.CHASE || state == State.ATTACK)
        {
            RotateTowardsTarget();
        }
        PlayAction(state);
    }

    protected virtual void LateUpdate()
    {
        if (state == State.CHASE || state == State.ATTACK)
        {
            RotateTowardsTarget();
        }
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


    // 항상 진행중인 기능
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
        if (target == null) return;

        // 타겟 방향 계산
        Vector3 direction = (target.position - transform.position).normalized;

        // 방향 벡터를 기준으로 회전 계산
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // 부드럽게 회전
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
            {
                ChangeState(State.CHASE);
                return;
            }

            // 공격 타이머 초기화
            attackTimer = 0f;
            ChangeState(State.ATTACK);
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

    protected void ChangeState(State newState)
    {

        if (Time.time - lastTransitionTime < transitionCooldown)
            return;

        lastTransitionTime = Time.time;

        if (state != newState || newState == State.HIT || newState == State.ATTACK)
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
