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
        owner.IdleToLurk();
        if(owner.COPYCHAIN <=0)
        {
            owner.IdleToLurk();
            return;
        }


        GameObject copy = GameObject.Instantiate(owner.gameObject, owner.transform.position, owner.transform.rotation);
        owner.copyList.Add(copy);
        copyState = copy.GetComponent<StatusBehaviour>();
        copy.GetComponent<Troy>().COPYCHAIN = 0;
        copy.GetComponent<BossStatus>().SetHealth(owner.BossStatus.GetHealth());
        copy.GetComponent<Troy>().HPBar.SetRatio(owner.BossStatus.GetHealth(), owner.BossStatus.GetMaxHealth());

        owner.GetComponent<MeshRenderer>().enabled = false;
        owner.HPBar.GetComponent<Canvas>().enabled= false;
        owner.GetComponent<BoxCollider>().enabled = false;
        owner.transform.Find("EnemyIcon").gameObject.SetActive(false);
        

        Vector3 dir = owner.Player.position - owner.transform.position;
        dir.y = 0;
        owner.NmAgent.isStopped = false;
        owner.NmAgent.SetDestination(owner.transform.position + dir*3);


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
        owner.transform.Find("EnemyIcon").gameObject.SetActive(true);
    }
}

