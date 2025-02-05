using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Idle_State : BossPhaseBase<Ransomware>
{
    private float idleTimer = 0f;
    private float idleDuration = 2f;

    public Phase1_Idle_State(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
        idleTimer = 0f;
        owner.GetComponent<Animator>()?.SetTrigger("Phase1Idle");
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;
        // Idle 동작 구현
    }
}
