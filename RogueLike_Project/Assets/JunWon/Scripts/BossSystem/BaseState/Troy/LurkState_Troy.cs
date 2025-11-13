using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Tutorials.Core.Editor;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public class LurkState_Troy : State<Troy>
{
    public LurkState_Troy(Troy owner) : base(owner) { }

    float timer = 0;
    float originSpeed;
    Vector3[] dir = new Vector3[3];
    Transform player;


    public override void Enter()
    {
        originSpeed = owner.BossStatus.GetMovementSpeed();
        owner.NmAgent.isStopped = false;
        timer = 0f;
        if (!owner.isBoss) return;
        player = owner.Player;
        dir[0] = player.right;
        dir[1] = -player.right;
        dir[2] = -player.forward;
        for(int i = 0;i<3;i++)
        {
            Troy alter = GameObject.Instantiate(owner.gameObject).GetComponent<Troy>();
            alter.GetComponent<BossBase>().isBoss = false;
            alter.NmAgent.SetDestination(player.position + dir[i] * 20);
            alter.BossStatus.SetMovementSpeed(originSpeed * 5);
            alter.HideAndSeek(false);
        }
        owner.NmAgent.SetDestination(player.position + player.forward * 50);
        owner.HideAndSeek(false);
    }
    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 5f)
        {
            owner.NmAgent.isStopped = false;
            owner.ChangeState(Troy.AnimatorState.Rush);
        }
        else if(timer >= 3f)
        {
            owner.NmAgent.isStopped = true;
            owner.BossStatus.SetMovementSpeed(originSpeed);
            owner.Animator.Play("Standby");
            owner.transform.rotation = Quaternion.LookRotation(owner.Player.position - owner.transform.position);
            owner.HideAndSeek(true);
        }
    }
  

    public override void Exit()
    {

    }

 

}

