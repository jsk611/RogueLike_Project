using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Tutorials.Core.Editor;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamouflageState_Troy : State<Troy>
{
    public CamouflageState_Troy(Troy owner) : base(owner) { }
    //select enemy in field
    //enforce it
    //being immune
    EnemySpawnLogic enemyManager;
    StatusBehaviour CamouflageObject;
    PlayerStatus playerStatus;
    PlayerControl playerControl;

  
    // Start is called before the first frame update
    public override void Enter()
    {
        if (playerStatus == null)
        {
            playerStatus = owner.Player.GetComponent<PlayerStatus>();
            playerControl = owner.Player.GetComponent<PlayerControl>();
        }
        owner.HideAndSeek(false);
        owner.NmAgent.isStopped = true;

        
        for(int i = 0;i<owner.SUMMONAMOUNT;i++) owner.SummonMinion();
        
        CamouflageObject = owner.SUMMONEDMONSTERS[Random.Range(0,owner.SUMMONEDMONSTERS.Count)];
    }
    public override void Update()
    {
        if (CamouflageObject == null)
        {   
            owner.TakeDamage(owner.BossStatus.GetHealth() * 0.2f);
            owner.ChangeState(Troy.AnimatorState.Stunned);
        }

    }
    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.HideAndSeek(true);
        owner.NmAgent.SetDestination(owner.transform.position);

        owner.SUMMONEDMONSTERS.RemoveAll(x => x == null);
        foreach (StatusBehaviour minion in owner.SUMMONEDMONSTERS)
        {
            minion.GetComponent<MonsterBase>()?.TakeDamage(9999, false);
        }
    }

}