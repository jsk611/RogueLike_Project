using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2_DataBlink_State : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase2_DataBlink_State(Ransomware owner) : base(owner)
    {
    }
    
}
