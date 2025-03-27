using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Phase1 : BossPhaseBase<SpiderPrime>
{
    public Spider_Phase1(SpiderPrime owner) : base(owner) { }
    StateMachine<SpiderPrime> subFsm;

    float meleeAttackRange = 5f;
    float rangedAttackRange = 20f;

    public string meleeAttack;
    public string rangedAttack;
    // Start is called before the first frame update

    private void InitializeAbilities()
    {
        meleeAttack = "SpiderMeleeAttack";
        rangedAttack = "SpiderRangeAttack";

        owner.AbilityManager.SetAbilityActive(meleeAttack);
        owner.AbilityManager.SetAbilityActive(rangedAttack);
    }
    private void InitializeState()
    {

    }
    private void InitializeSubFSM()
    {
        var huntState = new Phase1_Hunt_State(owner);
        var meleeAttackState = new Phase1_Melee_State(owner);
        var shootAttackState = new Phase1_Shoot_State(owner);
        //var aerial assault = new Phase2_Aerial_State(owner);

        subFsm = new StateMachine<SpiderPrime>(huntState);

        subFsm.AddTransition(new Transition<SpiderPrime>(
            huntState,
            meleeAttackState,
            () => Vector3.Distance(owner.Player.position, owner.transform.position) <= meleeAttackRange
            && owner.AbilityManager.GetAbilityRemainingCooldown(meleeAttack)==0)
        );
        subFsm.AddTransition(new Transition<SpiderPrime>(
            huntState,
            shootAttackState,
            () => Vector3.Distance(owner.Player.position, owner.transform.position) <= rangedAttackRange
            && owner.AbilityManager.GetAbilityRemainingCooldown(rangedAttack)==0
        ));

        subFsm.AddTransition(new Transition<SpiderPrime>(
            shootAttackState,
            huntState,
            () => shootAttackState.IsAttackFinished));
        subFsm.AddTransition(new Transition<SpiderPrime>(
            meleeAttackState,
            huntState,
            () => meleeAttackState.IsAttackFinished));


        //subFsm.AddTransition(new Transition<SpiderPrime>(
        //    meleeAttackState,
        //    huntState,
        //   // owner.AbilityManager.
        //));
    }
    // Update is called once per frame
    public override void Enter()
    {
        Debug.Log("spider phase1 start");
        
        InitializeState();
        InitializeAbilities();
        InitializeSubFSM();
    }
    public override void Update()
    {
        subFsm.Update();
        Debug.Log(subFsm.CurrentState);
    }
    public override void Exit()
    {
        subFsm.CurrentState?.Exit();
        owner.AbilityManager.SetAbilityInactive(meleeAttack);
        owner.AbilityManager.SetAbilityInactive(rangedAttack);
    }
}
