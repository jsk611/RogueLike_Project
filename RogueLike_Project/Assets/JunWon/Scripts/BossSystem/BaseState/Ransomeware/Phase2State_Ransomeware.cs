using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2State_Ransomeware : State<Ransomware>
{
    public Phase2State_Ransomeware(Ransomware owner) : base(owner)
    { }

    private StateMachine<Ransomware> subFsm;

    private void InitializeAbility()
    {
        owner.AbilityManager.SetAbilityActive("DataBlink");
        owner.AbilityManager.SetMaxCoolTime("DataBlink");

    }
    private void InitializeStats()
    {
    }
    private void InitializeSubFSM()
    {
        var idleState = new Phase1_Idle_State(owner);
        var chaseState = new Phase1_Chase_State(owner);
        var blinkState = new Phase2_DataBlink_State(owner);

        subFsm = new StateMachine<Ransomware>(idleState);


        subFsm.AddTransition(new Transition<Ransomware>(
           idleState,
           chaseState,
           () => true
       ));



        // Blink State Transition

        subFsm.AddTransition(new Transition<Ransomware>(
           chaseState,
           blinkState,
           () => owner.AbilityManager.GetAbilityRemainingCooldown("DataBlink") == 0
       ));

        subFsm.AddTransition(new Transition<Ransomware>(
            blinkState,
            chaseState,
            () => blinkState.IsAnimationFinished()
        ));


    }



    public override void Enter()
    {
        Debug.Log("랜섬웨어 보스 페이즈2 시작");
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase2"), 1);
        InitializeStats();
        InitializeAbility();
        InitializeSubFSM();

    }

    public override void Update()
    {
        subFsm.Update();
    }
}
