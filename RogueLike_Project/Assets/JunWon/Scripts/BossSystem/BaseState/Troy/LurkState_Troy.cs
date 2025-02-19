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
        lurkTimer = 0f;


        GameObject copy = GameObject.Instantiate(copyTroy, owner.transform.position, owner.transform.rotation);
        copyState = copy.GetComponent<StatusBehaviour>();
        copy.GetComponent<Troy>().COPYCHAIN = owner.COPYCHAIN - 1;

        owner.GetComponent<MeshRenderer>().enabled = false;
        owner.HPBar.GetComponent<Canvas>().enabled= false;
        owner.GetComponent<BoxCollider>().enabled = false;


        Vector3 dir = owner.Player.position - owner.transform.position;
        dir.y = 0;
        owner.NmAgent.isStopped = false;
        owner.NmAgent.SetDestination(owner.transform.position + dir*3);

        owner.IdleToLurk();

    }
    public override void Update()
    {
        lurkTimer += Time.deltaTime;
        if (copyTroy == null || copyState.GetHealth()<=0 || lurkTimer >= 4f)
        {
            owner.IdleToLurk();
        }
    }
    public override void Exit()
    {
        owner.NmAgent.ResetPath();
        owner.GetComponent<MeshRenderer>().enabled = true;
        owner.HPBar.GetComponent<Canvas>().enabled = true;
        owner.GetComponent<BoxCollider>().enabled = true;
    }
}

