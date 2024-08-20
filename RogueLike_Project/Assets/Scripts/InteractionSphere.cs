using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionSphere : MonoBehaviour
{
    // Start is called before the first frame update
    SphereCollider sphereCollider;
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Can Switch Wave");
            StartCoroutine("PressToSwitchWave");
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Cannot Switch Wave");
            StopCoroutine("PressToSwitchWave");
        }
    }
    //
    IEnumerator PressToSwitchWave()
    {
        while (true)
        {
            if(Input.GetKey(KeyCode.F))
            {
                Debug.Log("Switching!");
                Destroy(gameObject);
            }
            Debug.Log("Press to Switch Wave!");

            yield return null;
        }
    }
}
