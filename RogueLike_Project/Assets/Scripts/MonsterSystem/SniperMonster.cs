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

    public Weapon gun;
    public Transform firePoint;

    private FieldOfView fov;
    private float searchTargetDelay = 0.2f;


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
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
        }
    }

    private IEnumerator IDLE()
    {
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
            ChangeState(State.IDLE);
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

    private IEnumerator KILLED()
    {
        yield return null;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }


}
