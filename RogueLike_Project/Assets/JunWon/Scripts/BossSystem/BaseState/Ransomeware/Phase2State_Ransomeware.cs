using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2State_Ransomeware : State<Ransomware>
{
    public Phase2State_Ransomeware(Ransomware owner) : base(owner)
    { }

    private StateMachine<Ransomware> subFsm;

    private void InitializeSubFSM()
    {
        var idleState = new Phase1_Idle_State(owner);

        subFsm = new StateMachine<Ransomware>(idleState);
    
    }



    public override void Enter()
    {
        Debug.Log("랜섬웨어 보스 페이즈1 시작");
        InitializeSubFSM();
    }

    public override void Update()
    {
        subFsm.Update();
    }
}
