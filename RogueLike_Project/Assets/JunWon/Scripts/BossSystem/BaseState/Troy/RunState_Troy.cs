using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RunState_Troy : State<Troy>
{
    float timer = 0f;
    bool canHit;

    Vector3 dir;
    Vector3 dest;
    PlayerStatus playerStatus;
    PlayerControl playerControl;
    float originSpeed;

    public RunState_Troy(Troy owner) : base(owner)
    {
    }
    public override void Enter()
    {
        if (!playerStatus)
        {
            originSpeed = owner.BossStatus.GetMovementSpeed();
            playerControl = owner.Player.GetComponent<PlayerControl>();
            playerStatus = owner.Player.GetComponent<PlayerStatus>();
        }

        dir = (owner.Player.position - owner.transform.position).normalized;
        dest = owner.Player.position + dir * 10f;
        canHit = true;
        timer = 0f;
        owner.BossStatus.SetMovementSpeed(originSpeed * 3);
        owner.NmAgent.SetDestination(dest);
    }
    public override void Update()
    {
       
        if(canHit && Vector3.Distance(owner.Player.position,owner.transform.position) <= 6)
        {
            playerStatus.DecreaseHealth(owner.BossStatus.GetAttackDamage());
            owner.CoroutineRunner(playerControl.AirBorne((owner.Player.position - owner.transform.position).normalized, 10, 4));
            canHit = false;
        }
        if (Vector3.Distance(dest, owner.transform.position) < 4f || timer > 4)
        {
            owner.BossStatus.SetMovementSpeed(originSpeed);
            if (owner.lurkPhase) owner.CoroutineRunner(BreakToLurk());
            else owner.ChangeState(Troy.AnimatorState.Idle);
        }
        timer += Time.deltaTime;
    }
    public override void Exit()
    {
        if (!owner.isBoss) owner.TakeDamage(9999, false);
    }

    IEnumerator BreakToLurk()
    {
        owner.Animator.Play("Stunned");
        yield return new WaitForSeconds(3.5f);
        if (owner.lurkPhase) owner.ChangeState(Troy.AnimatorState.Lurk);
        else owner.ChangeState(Troy.AnimatorState.Idle);
    }
}

