using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_Melee_State : State<SpiderPrime>
{
    public Phase1_Melee_State(SpiderPrime owner) : base(owner) { }
    // Start is called before the first frame update
    private bool attackFinished = false;
    public bool IsAttackFinished => attackFinished;

    private float attackTime = 0.5f;

    FootIK frontLeft, frontRight;
    public override void Enter()
    {
        frontLeft = owner.LegIKManager.frontLeft;
        frontRight = owner.LegIKManager.frontRight;

        frontLeft.moveLock = true;
        frontRight.moveLock = true;

        owner.StartCoroutine(LegAttack());
    }
    public override void Update()
    {

    }
    public override void Exit()
    {
        attackFinished = false;

        frontLeft.moveLock = false;
        frontRight.moveLock = false;
    }

    private IEnumerator LegAttack()
    {
        float time = 0f;
        float elapsedTime = time/attackTime;
        Vector3 target = owner.Player.position;
        frontLeft.transform.position =target + Vector3.up * 20;
        frontRight.transform.position = target + Vector3.up * 20;
        while (elapsedTime < 1)
        {
            elapsedTime = time / attackTime;
            frontLeft.transform.position = Vector3.Lerp(frontLeft.transform.position, target, elapsedTime);
            frontRight.transform.position = Vector3.Lerp( frontRight.transform.position, target,elapsedTime);
            time += Time.deltaTime;
            yield return null;
        }
        attackFinished = true;
    }
}
