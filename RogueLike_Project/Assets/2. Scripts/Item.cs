using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] int star = 1;

    bool _isChasing = false;
    public bool isChasing
    {
        get { return _isChasing; }
        set 
        { 
            _isChasing = value;
            rb.useGravity = !value;
            physicsCol.isTrigger = value;
        }
    }
    Transform playerPos;
    Rigidbody rb;
    public float velocity;
    [SerializeField] SphereCollider physicsCol;
    private void Start()
    {
        playerPos = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!isChasing)
        {
            float distance = Vector3.Distance(transform.position, playerPos.position);
            if (distance < 5f || distance > 300f)
            {
                isChasing = true;
            }
        }

        if (isChasing)
        {

            Vector3 dir = (playerPos.position - transform.position).normalized; 
            rb.velocity = dir * velocity;
            if(velocity < 24) velocity *= 1.05f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            WaveManager waveManager = FindObjectOfType<WaveManager>();
            waveManager.AddItem(star);

            Destroy(gameObject);
        }
    }
}
