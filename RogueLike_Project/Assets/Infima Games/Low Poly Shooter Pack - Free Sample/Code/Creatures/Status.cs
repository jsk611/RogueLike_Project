using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    [Tooltip("Creature Health")]
    [SerializeField]
    private float Health;

    [Tooltip("Creature Defence")]
    [SerializeField]
    private float Defence;



    // Start is called before the first frame update
    public void DecreaseHealth(float damage)
    {
        Health -= damage - Defence;
    }
}
