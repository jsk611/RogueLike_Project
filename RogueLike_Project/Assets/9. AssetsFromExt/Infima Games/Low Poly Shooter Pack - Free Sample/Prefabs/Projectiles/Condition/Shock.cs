using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shock : WeaponCondition
{

    public float shockTime;

    public void StateInitializer(float dmg, float dur, float prob, float itv, float eff, float shkTime)
    {
        base.StateInitializer(dmg, dur, prob, itv, eff);
        shockTime = shkTime;
    }
    public void Succession(Shock bulletCondition)
    {
        bulletCondition.StateInitializer(damage, duration, probability,interval,effect, shockTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            collision.gameObject.GetComponent<StatusBehaviour>().ConditionOverload(StatusBehaviour.Condition.Shocked, effect, duration, interval,shockTime);
        }
    }
}

