using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SniperMonster : MonsterBase
{

    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float rotationSpeed = 2f;

    public EnemyWeapon gun;
    public Transform firePoint;

    private FieldOfView fov;
    private float searchTargetDelay = 0.2f;

    private Quaternion initialWatchDirection;

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>();
        initialWatchDirection = transform.rotation; // 몬스터의 초기 방향을 저장
        hp = 10; // 기본 체력 설정
        state = State.IDLE;
        base.Start();

        StartCoroutine(SearchForTarget());
    }

    protected override IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
        }
    }

    private IEnumerator IDLE()
    {
        // 타겟이 있는지 확인
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

    private IEnumerator ATTACK()
    {
        if (target != null)
        {
            ChangeState(State.AIMING);
        }
        else
        {
            ChangeState(State.SEARCH); // 타겟을 잃었을 때 SEARCH 상태로 전환
        }

        yield return null;
    }

    private IEnumerator AIMING()
    {
        ChangeState(State.SHOT);
        yield return new WaitForSeconds(fireRate);
    }

    private IEnumerator SHOT()
    {
        try
        {
            gun.Fire(transform.rotation);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gun Fire Error: " + e.Message);
        }

        ChangeState(State.ATTACK);
        yield return null;
    }

    private IEnumerator SEARCH()
    {
        // SEARCH 상태에서는 일정 시간 동안 타겟을 찾으려고 시도
        float searchDuration = 2f; // 탐색 지속 시간
        float elapsedTime = 0f;

        while (elapsedTime < searchDuration)
        {
            // 시야 내에 타겟이 있는지 확인
            if (fov.visibleTargets.Count > 0)
            {
                target = fov.visibleTargets[0];
                ChangeState(State.ATTACK);
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 타겟을 찾지 못했다면 IDLE 상태로 전환
        ChangeState(State.IDLE);
    }

    private IEnumerator KILLED()
    {
        yield return null;
    }

    private void Update()
    {
        if (target != null)
        {
            // 타겟을 향해 회전
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // 만약 타겟이 시야에서 벗어났다면
            if (!fov.visibleTargets.Contains(target))
            {
                target = null;
                ChangeState(State.SEARCH); // SEARCH 상태로 전환
            }
        }
        else if (state == State.SEARCH || state == State.IDLE)
        {
            // SEARCH 또는 IDLE 상태일 때 초기 감시 방향으로 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, initialWatchDirection, Time.deltaTime * rotationSpeed);
        }
    }

    private IEnumerator SearchForTarget()
    {
        while (hp > 0)
        {
            // 일정 시간마다 시야를 스캔
            yield return new WaitForSeconds(searchTargetDelay);

            if (fov.visibleTargets.Count > 0 && state != State.ATTACK && state != State.AIMING && state != State.SHOT)
            {
                target = fov.visibleTargets[0];
                ChangeState(State.ATTACK);
            }
            else if (fov.visibleTargets.Count == 0 && target != null)
            {
                // 타겟을 잃었을 때, SEARCH 상태로 전환
                target = null;
                ChangeState(State.SEARCH);
            }
        }
    }
}
