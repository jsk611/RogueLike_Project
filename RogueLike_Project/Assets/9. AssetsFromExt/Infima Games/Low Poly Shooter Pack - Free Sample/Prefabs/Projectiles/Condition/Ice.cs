using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : WeaponCondition
{


    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            collision.gameObject.GetComponent<StatusBehaviour>().ConditionOverload(StatusBehaviour.Condition.Iced, effect, duration, interval);
        }
    }
}
