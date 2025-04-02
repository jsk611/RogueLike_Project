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
    public string assaultAttack;
    public string rushAttack;
    // Start is called before the first frame update

    private void InitializeAbilities()
    {
        meleeAttack = "SpiderMeleeAttack";
        rangedAttack = "SpiderRangeAttack";
        assaultAttack = "SpiderAerialAttack";
        rushAttack = "SpiderRushAttack";

        owner.AbilityManager.SetAbilityActive(meleeAttack);
        owner.AbilityManager.SetAbilityActive(rangedAttack);
        owner.AbilityManager.SetAbilityActive(assaultAttack);

        owner.AbilityManager.SetAbilityActive(rushAttack);
        owner.AbilityManager.SetMaxCoolTime(rushAttack);
    }
    private void InitializeState()
    {

    }
    private void InitializeSubFSM()
    {
        var huntState = new Phase1_Hunt_State(owner);
        var meleeAttackState = new Phase1_Melee_State(owner);
        var shootAttackState = new Phase1_Shoot_State(owner);
        var aerialAttackState = new Phase1_AirAssault(owner);
        var rushAttackState = new Phase1_Rush_State(owner);

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
        //subFsm.AddTransition(new Transition<SpiderPrime>(
        //    huntState,
        //    aerialAttackState,
        //    () => Vector3.Distance(owner.Player.position, owner.transform.position) > rangedAttackRange
        //    && owner.AbilityManager.GetAbilityRemainingCooldown(assaultAttack) == 0
        //));
        subFsm.AddTransition(new Transition<SpiderPrime>(
            huntState,
            rushAttackState,
            () => 
             owner.AbilityManager.GetAbilityRemainingCooldown(rushAttack) == 0
        ));
            


        subFsm.AddTransition(new Transition<SpiderPrime>(
            shootAttackState,
            huntState,
            () => shootAttackState.IsAttackFinished
        ));
        subFsm.AddTransition(new Transition<SpiderPrime>(
            meleeAttackState,
            huntState,
            () => meleeAttackState.IsAttackFinished
        ));
        subFsm.AddTransition(new Transition<SpiderPrime>(
            aerialAttackState,
            huntState,
            () => aerialAttackState.isAttackFinished
        ));
        subFsm.AddTransition(new Transition<SpiderPrime>(
            rushAttackState,
            huntState,
            ()=>rushAttackState.isAttackFinished
        ));
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
