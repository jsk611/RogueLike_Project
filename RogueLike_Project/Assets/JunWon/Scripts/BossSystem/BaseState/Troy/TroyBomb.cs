using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class TroyBomb : MonoBehaviour
{
    // Start is called before the first frame update
    private float explodeTimer = 0f;
    private float explodeInterval;

    public float ExplosionTime {  get=>explodeInterval; set => explodeInterval = value; }
    // Update is called once per frame
    void Update()
    {
        explodeTimer += Time.deltaTime;
        //material color change();
        if(explodeTimer >= explodeInterval )
        {
            //explode with damage;
        }
    }
}
