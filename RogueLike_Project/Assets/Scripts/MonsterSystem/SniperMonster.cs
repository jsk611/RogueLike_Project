using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SniperMonster : MonoBehaviour
{
    [SerializeField]
    MonsterData monsterData;
    public MonsterData MonsterData { set { monsterData = value; } }


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
    [SerializeField] float rotationSpeed = 2f;

    public Weapon gun;
    public Transform firePoint;

    public Transform target;

    private FieldOfView fov;
    private Coroutine stateMachineCoroutine;

    private float searchTargetDelay = 0.2f;


    enum State
    {
        IDLE,
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
        stateMachineCoroutine = StartCoroutine(StateMachine());
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
            ChangeState(State.ATTACK);
        }
        else
        {
            target = null;
        }

        yield return null;
    }


    IEnumerator ATTACK()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        //anim.Play("Attack01", 0, 0);

        // 거리가 멀어지면
        
            // StateMachine을 추적으로 변경
        if (target != null) ChangeState(State.AIMING);
        else ChangeState(State.IDLE);


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
        try
        {
            // gun.Fire(transform.rotation);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gun Fire Error: " + e.Message);
        }

        ChangeState(State.ATTACK);
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
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        // target 이 null 이 아니면 target 을 계속 추적
    }


    public void TakeDamage(int damage, Transform playerTransform)
    {
        hp_ -= damage;

        if (hp_ > 0)
        {
            ChangeState(State.IDLE);
            target = playerTransform;
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
        // 적이 사망하면 수행할 동작 (예: 애니메이션 재생, 오브젝트 비활성화 등)
        Destroy(gameObject);
    }
}
