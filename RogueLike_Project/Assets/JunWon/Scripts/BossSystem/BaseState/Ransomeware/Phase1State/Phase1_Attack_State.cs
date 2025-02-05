using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Attack_State : BossPhaseBase<Ransomware>
{
    private float timer = 0f;
    private float attackCoolTime = 2f;

    public Phase1_Attack_State(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
        owner.NmAgent.isStopped = true;
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
    }
}
