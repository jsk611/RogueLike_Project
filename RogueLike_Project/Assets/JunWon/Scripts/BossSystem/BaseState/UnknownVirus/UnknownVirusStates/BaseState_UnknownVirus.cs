using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Base State
// Base state for all Unknown Virus boss states
public abstract class BaseState_UnknownVirus : State<UnknownVirusBoss>
{
    protected bool isAttackFinished = false;

    public BaseState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public virtual void SetAttackFinished(bool value)
    {
        isAttackFinished = value;
    }
}
#endregion