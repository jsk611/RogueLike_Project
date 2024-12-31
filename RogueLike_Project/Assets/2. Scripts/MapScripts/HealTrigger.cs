using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealTrigger : MonoBehaviour
{
    bool isHealed = false;
    // Start is called before the first frame update
    void Start()
    {
        isHealed = false;
    }

    private void OnEnable()
    {
        isHealed = false;
        GetComponentInChildren<PlatformIcon>().gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color += new Color(0, 0, 0, 0.125f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!isHealed && Input.GetKeyDown(KeyCode.F))
            {
                isHealed = true;
                PlayerStatus ps = other.gameObject.GetComponent<PlayerStatus>();
                ps.IncreaseHealth(ps.GetMaxHealth() * 0.3f);
                GetComponent<MeshRenderer>().material.color -= new Color(1, 1, 1, 0.125f);
                GetComponentInChildren<PlatformIcon>().gameObject.SetActive(false);
            }
        }
    }
    
}
