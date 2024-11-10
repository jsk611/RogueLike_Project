using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpgradeTrigger : MonoBehaviour
{
    [SerializeField] GameObject upgradeUI;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                upgradeUI.SetActive(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (upgradeUI.activeSelf)
            {
                upgradeUI.SetActive(false);
            }
        }
    }

}
