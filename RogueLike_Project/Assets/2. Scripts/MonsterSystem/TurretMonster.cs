using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class TurretMonster : RangedMonster
{
    protected override void UpdateChase()
    {
        if (target == null) {
            ChangeState(State.IDLE);
            return;
        }

        if (Vector3.Distance(target.position, transform.position) <= attackRange)
            ChangeState(State.ATTACK);
    }

    
    protected override void UpdateHit()
    {
        if (anim.GetFloat("Activated") == 1)
            ChangeState(State.ATTACK);
        else if (anim.GetFloat("Activated") == 0)
            ChangeState(State.CHASE);
    }
    public void SetActivated(int value) 
    {
        if (value == 1) anim.SetFloat("Activated", 1);
        else anim.SetFloat("Activated", 0);
    }
}
