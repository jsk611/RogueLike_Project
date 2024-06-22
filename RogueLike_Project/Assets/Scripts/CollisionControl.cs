using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionControl : MonoBehaviour
{
    // Start is called before the first frame update
    BoxCollider HitBox;
    void Start()
    {
        HitBox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "wall") Debug.Log("my head!");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "wall") Debug.Log("my head!");
    }
}
