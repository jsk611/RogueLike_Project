using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedMonster : MonsterBase
{
    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    [SerializeField] float fireRate = 2f;
    [SerializeField] float rotationSpeed = 2f;

    public EnemyWeapon gun;
    public Transform firePoint;

    private FieldOfView fov;

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>();
        hp = 10; // 기본 체력 설정
        state = State.IDLE;
        base.Start();
    }

    protected override IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            
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
            ChangeState(State.AIMING);
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

    IEnumerator ATTACK()
    {
        try
        {
            gun.Fire(transform.rotation);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Gun Fire Error: " + e.Message);
        }

        ChangeState(State.CHASE);
        yield return new WaitForSeconds(fireRate);
    }

    private void Update()
    {
        if (target != null && (state == State.AIMING || state == State.CHASE))
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
