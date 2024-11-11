using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("settings")]
    [SerializeField] float firerate = 2f;
    [SerializeField] float rotationSpeed = 2f;

    public EnemyWeapon gun;
    public Transform firePoint;

    protected override void Start() { base.Start(); }

    #region StateMachine
    protected override IEnumerator IDLE() { return base.IDLE(); }
    protected override IEnumerator CHASE() { return base.CHASE(); }
    protected override IEnumerator ATTACK() { 
        yield return base.ATTACK();

        try
        {
            gun.Fire(transform.rotation);
        }
        catch (System.Exception e)
        {
            Debug.LogError("gun fire error: " + e.Message);
        }

        ChangeState(State.ATTACK);
        yield return null;


    }
    protected override IEnumerator HIT() { return base.HIT(); }
    protected override IEnumerator AIM() { return base.AIM(); }

    protected override IEnumerator DIE() { return base.DIE(); }
    #endregion


    private void Update()
    {
        if (target != null && (state == State.AIM || state == State.CHASE))
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
