using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LurkState_Troy : State<Troy>
{
    public LurkState_Troy(Troy owner) : base(owner) { }
    private GameObject copyTroy;
    private StatusBehaviour copyState;
    private MeshRenderer meshRenderer;

    private float lurkTimer;
    public override void Enter()
    {
        copyTroy = owner.TROYBOMB;
        copyState = copyTroy.GetComponent<StatusBehaviour>();
        lurkTimer = 0f;
        GameObject.Instantiate(copyTroy, owner.transform.position, owner.transform.rotation);
        owner.GetComponent<MeshRenderer>().enabled = false;

        Vector3 dir = owner.Player.position - owner.transform.position;
        dir.y = 0;
        owner.NmAgent.SetDestination(owner.transform.position + dir*3);

        owner.IdleToLurk();
    }
    public override void Update()
    {
        lurkTimer += Time.deltaTime;
        if (lurkTimer >= 4f)
        {
            owner.IdleToLurk();
        }
    }
    public override void Exit()
    {
        owner.NmAgent.ResetPath();
        owner.GetComponent<MeshRenderer>().enabled = true;
    }
}

