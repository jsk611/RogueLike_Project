using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState_Troy : State<Troy>
{
    float dirCoolTime = 1f;
    float dirTime = 0f;

    Vector3 dir = Vector3.zero;
    Vector3 dest = Vector3.zero;

    float runTimer = 0f;
    float camouflageTimer = 0f;
    float lurkTimer = 0f;

    public IdleState_Troy(Troy owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.NmAgent.isStopped = false;
        owner.ChangeState(Troy.AnimatorState.Walk);

        runTimer = 0f;
    }
    public override void Update()
    {
        runTimer += Time.deltaTime;
        camouflageTimer += Time.deltaTime;
        if(!owner.lurkPhase) lurkTimer += Time.deltaTime;
        Walking();

        if (!owner.isBoss) owner.ChangeState(Troy.AnimatorState.Lurk);

        if (camouflageTimer >= owner.CAMOUFLAGEINTERVAL)
        {
            camouflageTimer = 0f;
            owner.ChangeState(Troy.AnimatorState.Camouflage);
        }
        if (runTimer >= owner.RUNINTERVAL)
        {
            runTimer = 0f;
            owner.ChangeState(Troy.AnimatorState.Rush);
        }
        if (lurkTimer >= owner.LURKINTERVAL)
        {
            lurkTimer = 0f;
            owner.lurkPhase = true;
            owner.ChangeState(Troy.AnimatorState.Lurk);
        }
    }
    public override void Exit()
    {
    }

    private void Walking()
    {
        if (dirTime > dirCoolTime || Vector3.Distance(owner.transform.position,dest) < 4f)
        {
            dirTime = 0f;
            dir = new Vector3(Random.Range(-15, 16), 0, Random.Range(-15,16));
            dest = owner.transform.position + dir*2;
            owner.NmAgent.SetDestination(dest);
        }
        dirTime += Time.deltaTime;
    }
}

