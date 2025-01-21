using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffField : MonoBehaviour
{
    public float duration = 5f;
    
    [SerializeField]
    private float healAmount = 5f; // 초당 회복량
    public float healInterval = 1f; // HP 회복 간격 (초)

    [SerializeField]
    private float healTimer = 0f;


    [SerializeField]
    private LayerMask targetLayer; // 몬스터 Layer

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerStay(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            
            if (monster != null)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healInterval)
                {
                    monster.IncreaseHealth(healAmount);
                    Debug.Log("Heal");
                    healTimer = 0f;
                }
            }
        }
    }

}
