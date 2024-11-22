using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFaceToPlayer : MonoBehaviour
{
    Transform playerTransform;
    [SerializeField] bool isDamageUI;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        StartCoroutine(InitialCoroutine());
    }
    IEnumerator InitialCoroutine()
    {
        yield return null;
        if (isDamageUI)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            float angleY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + 180f;
            transform.rotation = Quaternion.Euler(0, angleY, 0);
            transform.position -= transform.forward * 1f + transform.forward * Random.Range(0, 0.2f);
            transform.position += transform.right * Random.Range(-0.5f, 0.5f);
            transform.position += transform.up * Random.Range(-0.5f, 0.5f);


        }
    } 

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;

        float angleY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + 180f; 
        transform.rotation = Quaternion.Euler(0, angleY, 0); 

    }
}
