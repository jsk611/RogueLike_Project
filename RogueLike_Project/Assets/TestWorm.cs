using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class TestWorm : MonoBehaviour
{
    public List<Transform> bodyList;
    public Transform target;
    public float speed;
    Vector3 velocity = Vector3.zero;

    float turnInterval = 1f;
    float turnTimer = 0f;
    // Start is called before the first frame update
    Transform head;
    void Start()
    {
        head = bodyList[0];
    }

    // Update is called once per frame
    void Update()
    {
        turnTimer += Time.deltaTime;
        if(turnTimer>=turnInterval)
        {
            turnTimer = 0f;
            target.position = head.position + new Vector3(Random.Range(-10,10), Random.Range(-10, 10), Random.Range(-10, 10));    
        }
        head.rotation = Quaternion.Lerp(head.rotation, Quaternion.LookRotation(target.position - head.position), Time.deltaTime * speed);
        head.position += head.forward * Time.deltaTime * speed;
        for (int i = 1; i < bodyList.Count; i++)
        {
            //float x = Mathf.Lerp(bodyList[i].position.x, bodyList[i - 1].position.x, 0.1f);
            //float y = Mathf.Lerp(bodyList[i].position.y, bodyList[i - 1].position.y, 0.1f);
            //float z = Mathf.Lerp(bodyList[i].position.z, bodyList[i - 1].position.z, 0.1f);
            //bodyList[i].position = new Vector3(x, y, z);
            bodyList[i].rotation = Quaternion.Lerp(bodyList[i].rotation, Quaternion.LookRotation(bodyList[i - 1].position - bodyList[i].position), Time.deltaTime * speed);
            bodyList[i].position = Vector3.Lerp(bodyList[i].position,bodyList[i-1].position, Time.deltaTime * speed*0.8f);
        }
    }
}
