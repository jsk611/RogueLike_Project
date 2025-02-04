using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatedState_Ransomeware : State<Ransomware>
{
    public DefeatedState_Ransomeware(Ransomware owner) : base(owner) {
    }

    public override void Enter()
    {
        Debug.Log("[IntroState_Ransomeware] Enter");
        owner.GetComponent<Animator>().SetTrigger("Dead");
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        Debug.Log("[IntroState_Ransomeware] Exit");
    }


}
