using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashState_Troy : State<Troy>
{
    public CrashState_Troy(Troy owner) : base(owner) { }

    float deathTimer = 10f;
    float originSpeed;
    PlayerStatus playerStatus;

    bool standby;
    List<GameObject> stamp;

    public override void Enter()
    {
        if (!owner.isBoss) return;
        owner.ChangeState(Troy.AnimatorState.Crash);
        owner.crashPhase = true;
        originSpeed = owner.BossStatus.GetMovementSpeed();
        stamp = new List<GameObject>();
        standby = true;
        playerStatus = owner.Player.GetComponent<PlayerStatus>();
        owner.NmAgent.isStopped = true;
        owner.crash = false;
        owner.HideHP(false);
        owner.CoroutineRunner(FrenzySummoning());

    }

    public override void Update()
    {
        if (!owner.isBoss) return;
        deathTimer -= Time.deltaTime;
        if (deathTimer <= -2f)
        {
            owner.NmAgent.isStopped = false;
            owner.BossStatus.SetAttackDamage(9999999999);
            owner.NmAgent.SetDestination(owner.Player.position);
            owner.ChangeState(Troy.AnimatorState.Rush);
        }
        else if(deathTimer <= 0)
        {
            if(standby) owner.Animator.Play("Standby");
            standby = false;
            return;
        }
        if (!standby && Vector3.Distance(owner.Player.position, owner.transform.position) <= 5)
        {
            GameObject.Instantiate(owner.BOMBEFFECT,owner.transform.position,Quaternion.Euler(Vector3.left*90));
            playerStatus.DecreaseHealth(owner.BossStatus.GetAttackDamage());
        }

    }

    public override void Exit()
    {
        owner.HideAndSeek(true);
        owner.crashPhase = false;
        foreach(GameObject stp in stamp)
        {
            stp.SetActive(false);
            GameObject.Destroy(stp);
        }
    }

    IEnumerator FrenzySummoning()
    {
        for(int i = 0;i<15;i++)
        {
            Vector3 randPos = new Vector3(Random.Range(-4, 4f), 0, Random.Range(-4, 4f));
            Troy copy = GameObject.Instantiate(owner.gameObject, owner.transform.position + randPos, Quaternion.LookRotation(randPos)).GetComponent<Troy>();
            copy.isBoss = false;
            copy.NmAgent.isStopped = true;
            copy.BossStatus.SetHealth(200);
            copy.HideHP(false);
            copy.MakeDoll();
           
            stamp.Add(copy.gameObject);
            yield return new WaitForSeconds(Random.Range(0,0.4f));
        }
    }
}
