using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{

    Transform originalPosition;
    // Start is called before the first frame update
    void Awake()
    {
        originalPosition = transform;
    }

    // Update is called once per frame
    void Update()
    {
        float x = Random.Range(-1, 2) * 0.1f;
        float y = Random.Range(-1, 2) * 0.1f;

        transform.position = new Vector3(originalPosition.position.x + x, originalPosition.position.y + y, originalPosition.position.z);
    }
}
