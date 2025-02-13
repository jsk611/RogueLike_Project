using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LurkState_Troy : State<Troy>
{
    public LurkState_Troy(Troy owner) : base(owner) { }
    //select enemy in field
    //enforce it
    //being immune

    // Start is called before the first frame update
    public override void Enter()
    {
        owner.ISCAMOUFLAGED = true;
    }
    public override void Update()
    {
        
    }
    public override void Exit()
    {
        
    }
}
