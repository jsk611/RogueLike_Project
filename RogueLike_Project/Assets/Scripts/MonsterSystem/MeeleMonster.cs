using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeeleMonster : MonsterBase
{
    [Header("Settings")]
    public float attackRange = 2f; // 공격 거리
    public float attackCooldown = 1.5f; // 공격 간격
    public int damage = 10; // 공격력

    [SerializeField] float closeRange = 5f; // 매우 가까운 범위
    private FieldOfView fov;

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        base.Start();
    }

    protected override IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
        }
    }

    IEnumerator IDLE()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else if (target != null && Vector3.Distance(transform.position, target.position) <= closeRange)
        {
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
        nmAgent.SetDestination(target.position);

        if (nmAgent.remainingDistance <= nmAgent.stoppingDistance)
        {
            ChangeState(State.ATTACK);
        }

        yield return null;
    }

    IEnumerator ATTACK()
    {
        if (nmAgent.remainingDistance > nmAgent.stoppingDistance)
        {
            ChangeState(State.CHASE);
        }

        yield return null;
    }
}
