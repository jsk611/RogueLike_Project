using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LurkState_Troy : State<Troy>
{
    public LurkState_Troy(Troy owner) : base(owner) { }
    private StatusBehaviour copyState;
    private MeshRenderer meshRenderer;

    private float lurkTimer;
    public override void Enter()
    {
        lurkTimer = 0f;
        owner.IdleToLurk();
        if(owner.COPYCHAIN <=0)
        {
            owner.IdleToLurk();
            return;
        }
        HideAndSeek(false);
        Vector3 dir = owner.Player.position - owner.transform.position;
        dir.y = 0;
        owner.NmAgent.isStopped = false;
        owner.NmAgent.SetDestination(owner.transform.position + dir*3);
        GameObject copy = GameObject.Instantiate(EnemySpawnLogic.instance.GetEnemyPrefab(EnemyType.Wormboss), owner.transform.position, owner.transform.rotation);
        copyState = copy.GetComponent<StatusBehaviour>();
        copy.GetComponent<Troy>().COPYCHAIN = 0;
     //   copy.GetComponent<Troy>().SetCopied(owner.BossStatus.GetHealth());
     //   Debug.Log(copyState.name);
        Debug.Log(copyState.GetHealth());

    }
    public override void Update()
    {
        lurkTimer += Time.deltaTime;
        if (copyState.GetHealth()<=0 || lurkTimer >= 4f)
        {
            owner.IdleToLurk();
        }
    }
    public override void Exit()
    {
        owner.NmAgent.ResetPath();
        HideAndSeek(true);
    }
    private void HideAndSeek(bool val)
    {
        owner.GetComponent<MeshRenderer>().enabled = val;
        owner.GetComponent<BoxCollider>().enabled = val;
        owner.transform.Find("EnemyIcon").gameObject.SetActive(val);
    }
}

