using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shock : WeaponCondition
{

    public float shockTime;

    public void StateInitializer(float eff, float dur, float itv, float shkTime)
    {
        base.StateInitializer(eff, dur, itv);
        shockTime = shkTime;
    }
    public void Succession(Shock bulletCondition)
    {
        bulletCondition.StateInitializer(effect, duration, interval, shockTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            collision.gameObject.GetComponent<StatusBehaviour>().ConditionOverload(StatusBehaviour.Condition.Shocked, effect, duration, interval,shockTime);
        }
    }
}

