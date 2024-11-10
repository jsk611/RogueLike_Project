using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trousseau : MonoBehaviour
{
    // Start is called before the first frame update

    const string creature = "Creature";
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(creature))
        {
            collision.gameObject.SetActive(false);
        }
    }
}
