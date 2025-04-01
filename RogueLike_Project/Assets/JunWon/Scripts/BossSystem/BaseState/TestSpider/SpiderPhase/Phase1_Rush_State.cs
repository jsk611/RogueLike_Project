using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_Rush_State : State<SpiderPrime>
{
    public Phase1_Rush_State(SpiderPrime owner) : base(owner) { }

    private string rushAttack = "SpiderRushAttack";
    Vector3 rushPoint;
    Vector3 rushDirection;

    float rushSpeed;
    float rushTIme=0f;
    float rushDuration=1.5f;

    private bool attackFinished = false;



    public bool isAttackFinished => attackFinished;
    // Start is called before the first frame update
    public override void Enter()
    {
        rushDirection = (owner.Player.position - owner.transform.position).normalized;
        rushPoint = owner.Player.position+rushDirection*3f;

        rushSpeed = owner.BossStatus.GetMovementSpeed();
        owner.BossStatus.SetMovementSpeed(rushSpeed * 3);

        rushTIme = 0f;
    }
    public override void Update()
    {
        owner.NmAgent.speed = owner.BossStatus.GetMovementSpeed();
        owner.NmAgent.SetDestination(rushPoint);
        if (Vector3.Distance(rushPoint,owner.transform.position) <= 1f || rushTIme/rushDuration >= 1f)
        {
            owner.BossStatus.SetMovementSpeed(rushSpeed);
            owner.NmAgent.speed = owner.BossStatus.GetMovementSpeed();
            attackFinished = true;
        }
        rushTIme += Time.deltaTime;
    }
    public override void Exit() 
    {
        attackFinished = false;
        owner.NmAgent.SetDestination(owner.transform.position);
        owner.AbilityManager.SetMaxCoolTime(rushAttack);
    }
}
