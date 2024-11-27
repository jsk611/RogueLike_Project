using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : MonoBehaviour
{
    private float effect;
    public float duration;
    private float interval;
    // Start is called before the first frame update


    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
        {
            collision.gameObject.GetComponent<StatusBehaviour>().ConditionOverload(StatusBehaviour.Condition.Frozen, effect, duration, interval);
        }
    }
}
