using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_Melee_State : State<SpiderPrime>
{
    public Phase1_Melee_State(SpiderPrime owner) : base(owner) { }
    // Start is called before the first frame update
    private bool attackFinished = false;
    public bool IsAttackFinished => attackFinished;
    public override void Enter()
    {

    }
    public override void Update()
    {
        if (!attackFinished)
        {
            if (Vector3.Distance(owner.Player.position, owner.transform.position) <= 10f) owner.Player.GetComponent<PlayerStatus>().DecreaseHealth(owner.BossStatus.GetAttackDamage());
            attackFinished = true;
        }
    }
    public override void Exit()
    {
        attackFinished = false;
    }
}
