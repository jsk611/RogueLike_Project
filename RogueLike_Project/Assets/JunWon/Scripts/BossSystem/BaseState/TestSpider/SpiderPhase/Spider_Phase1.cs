using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Phase1 : BossPhaseBase<SpiderPrime>
{
    Spider_Phase1(SpiderPrime owner) : base(owner) { }
    StateMachine<SpiderPrime> subFsm;

    float meleeAttackRange = 5f;
    float rangedAttackRange = 20f;
    // Start is called before the first frame update
    void Start()
    {
        subFsm = new StateMachine<SpiderPrime>(this);
        InitializeState();
        InitializeSubFSM();
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

        subFsm.AddTransition(new Transition<SpiderPrime>(
            huntState,
            meleeAttackState,
            () => Vector3.Distance(owner.Player.position, owner.transform.position) <= meleeAttackRange
        ));
        subFsm.AddTransition(new Transition<SpiderPrime>(
            huntState,
            shootAttackState,
            () => Vector3.Distance(owner.Player.position, owner.transform.position) <= rangedAttackRange
    //        && owner.AbilityManager.GetAbilityRemainingCooldown("Spider Range Attack")
        ));


        //subFsm.AddTransition(new Transition<SpiderPrime>(
        //    meleeAttackState,
        //    huntState,
        //   // owner.AbilityManager.
        //));
    }
    // Update is called once per frame
    void Update()
    {
        subFsm.Update();
    }
}
