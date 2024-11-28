using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shock : MonoBehaviour
{
    private float effect = 0;
    public float duration;
    public float interval;
    public float shockTime;
    // Start is called before the first frame update


    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
     
            collision.gameObject.GetComponent<StatusBehaviour>().ConditionOverload(StatusBehaviour.Condition.Shocked, effect, duration, interval,shockTime);
        }
    }
}

