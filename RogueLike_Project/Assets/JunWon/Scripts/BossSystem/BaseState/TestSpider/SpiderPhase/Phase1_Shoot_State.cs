using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_Shoot_State : State<SpiderPrime>
{
    public Phase1_Shoot_State(SpiderPrime owner) : base(owner) {}
    private string rangedAttack = "SpiderRangeAttack";
    private bool attackFinished = false;
    public bool IsAttackFinished => attackFinished;
    public override void Enter()
    {

    }
    public override void Update()
    {
        if (!attackFinished)
        {
            owner.HeadWeapon.Fire();
            attackFinished = true;  
        }
    }
    public override void Exit()
    {
        attackFinished = false;
        owner.AbilityManager.SetMaxCoolTime(rangedAttack);
    }
    
}
