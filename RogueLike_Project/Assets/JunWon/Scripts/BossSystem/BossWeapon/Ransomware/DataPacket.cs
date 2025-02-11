using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPacket : MProjectile
{
    private PostProcessingManager postProcessing;
    void Start()
    {
        postProcessing = FindObjectOfType<PostProcessingManager>();
        Debug.Log("PostProcessing reference: " + (postProcessing != null ? "Found" : "Not Found"));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit detected");
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.ConditionOverload(StatusBehaviour.Condition.Frozen, 0, 0.5f);
                playerStatus.DecreaseHealth(damage);
            }
            Destroy(gameObject);
        }
    }

}
