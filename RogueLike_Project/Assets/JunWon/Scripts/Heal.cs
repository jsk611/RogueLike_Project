using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    [SerializeField] Vector3 defaultSize;
    [SerializeField] private LayerMask targetLayer; // Inspector에서 설정
    void Update()
    {
        transform.localScale = new Vector3(
                defaultSize.x / transform.parent.localScale.x,
                defaultSize.y / transform.parent.localScale.y,
                defaultSize.z / transform.parent.localScale.z
            );
        transform.position = new Vector3(transform.position.x, transform.parent.position.y + transform.parent.localScale.y / 2 - 0.95f, transform.position.z);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            other.gameObject.GetComponent<MonsterStatus>().IncreaseHealth(Time.deltaTime * 25);
        }
    }
}
