using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blaze : WeaponCondition
{


    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            if (collision.gameObject.GetComponent<StatusBehaviour>() == null) return;
            collision.gameObject.GetComponent<StatusBehaviour>().ConditionOverload(StatusBehaviour.Condition.Blazed,damage,duration,probability,interval,effect);
        }
    }
}
