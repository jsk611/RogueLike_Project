using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Tazer : MonoBehaviour
{
    BoxCollider bulletCollider;
    // Start is called before the first frame updatey

    const string wall = "Wall";
    const string enemy = "Creature";

    Collider[] hits;

    LayerMask hitMask;
    void Awake()
    {
        bulletCollider = GetComponent<BoxCollider>();
        hitMask = LayerMask.GetMask(wall,enemy);
    }

    // Update is called once per frame


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall")|| collision.gameObject.layer == LayerMask.NameToLayer("Creature")) 
        {
 
                hits = Physics.OverlapSphere(transform.position, 30.0f, hitMask, QueryTriggerInteraction.Collide);
            StartCoroutine(Test());
             //   StartCoroutine(Oscillation(hits[0].transform,3.0f));
                
                //foreach (Collider hit in hits)
                //{
                //    StartCoroutine(Oscillation(hit.transform,0.3f));
                //}
            
        }
    }
    

    IEnumerator Oscillation(Transform body,float duration)
    {
        float shockedTime = 0.0f;
        Vector3 originalPosition = body.position;


        while (shockedTime < 3)    
        {
             shockedTime += Time.deltaTime;

            Debug.Log(body.transform.name);

            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);

            body.position += new Vector3(x,  y, 0);

           

            yield return null ;
        }
    }

    IEnumerator Test()
    {
        float test = 0;
        while (test < 3)
        {
            test += Time.deltaTime;
            Debug.Log("SDFf");
            yield return null;
        }
    }

}
