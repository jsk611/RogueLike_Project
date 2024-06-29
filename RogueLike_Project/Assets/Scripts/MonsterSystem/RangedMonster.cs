using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedMonster : MonoBehaviour
{
    [Header("RangedMonster Stats")]
    [SerializeField] int monsterID_ = 0;
    [SerializeField] float hp_ = 0;
    [SerializeField] float def = 0;


    [Header("Preset Fields")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject splashFx;
    [SerializeField] private NavMeshAgent nmAgent;


    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    [SerializeField] float fireRate = 2f;

    public Weapon gun;
    public Transform firePoint;

    public Transform target;

    private FieldOfView fov;

    private float searchTargetDelay = 0.2f;


    enum State
    {
        IDLE,
        CHASE,
        ATTACK,
        AIMING,
        SHOT,
        KILLED
    }

    State state; // setting situation

    void Start()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();

        fov = GetComponent<FieldOfView>();

        hp_ = 10;
        state = State.IDLE;
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (hp_ > 0)
        {
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
        }
    }

    IEnumerator IDLE()
    {
        // 현재 animator 상태정보 얻기
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);

        //// 애니메이션 이름이 IdleNormal 이 아니면 Play
        //if (curAnimStateInfo.IsName("IdleNormal") == false)
        //    anim.Play("IdleNormal", 0, 0);

        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return null;
    }

    IEnumerator CHASE()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);

        //if (curAnimStateInfo.IsName("WalkFWD") == false)
        //{
        //    anim.Play("WalkFWD", 0, 0);
        //    // SetDestination 을 위해 한 frame을 넘기기위한 코드
        //    yield return null;
        //}


        nmAgent.SetDestination(target.position);

        // 목표까지의 남은 거리가 멈추는 지점보다 작거나 같으면
        if (nmAgent.remainingDistance <= nmAgent.stoppingDistance)
        {
            // StateMachine 을 공격으로 변경
            ChangeState(State.AIMING);
        }

        // 목표 감지를 실패한 경우
        if (fov.visibleTargets.Count == 0 || target == null)
        {
            yield return new WaitForSeconds(searchTargetDelay);
            ChangeState(State.IDLE);
        }
        yield return null;
    }

    IEnumerator ATTACK()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        //anim.Play("Attack01", 0, 0);

        // 거리가 멀어지면
        if (nmAgent.remainingDistance > nmAgent.stoppingDistance)
        {
            // StateMachine을 추적으로 변경
            ChangeState(State.CHASE);
        }

        yield return null;
        // 공격 animation 의 두 배만큼 대기
        // 이 대기 시간을 이용해 공격 간격을 조절할 수 있음.
        //yield return new WaitForSeconds(curAnimStateInfo.length * 2f);
    }

    IEnumerator AIMING()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        //anim.Play("Attack01", 0, 0);

        // 거리가 멀어지면
        ChangeState(State.SHOT);
        yield return new WaitForSeconds(fireRate);

        // 공격 animation 의 두 배만큼 대기
        // 이 대기 시간을 이용해 공격 간격을 조절할 수 있음.
        //yield return new WaitForSeconds(curAnimStateInfo.length * 2f);
    }

    IEnumerator SHOT()
    {
        gun.Fire(transform.rotation);
        ChangeState(State.CHASE);
        yield return null;
    }

    IEnumerator KILLED()
    {
        yield return null;
    }

    void ChangeState(State newState)
    {
        state = newState;
    }

    void Update()
    {
        if (target == null) return;
        // target 이 null 이 아니면 target 을 계속 추적
        nmAgent.SetDestination(target.position);
    }
}
