using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTrigger : MonoBehaviour
{
    UpgradeManager UpgradeManager;
    bool isUpgraded;
    // Start is called before the first frame update
    void Start()
    {
        UpgradeManager = ServiceLocator.Current.Get<IGameModeService>().GetUpgradeManager();
    }

    private void OnEnable()
    {
        isUpgraded = false;
        GetComponentInChildren<PlatformIcon>().gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color += new Color(0, 0, 0, 0.125f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!isUpgraded && Input.GetKeyDown(KeyCode.F))
            {
                isUpgraded = true;
                UpgradeManager.RepeatNumSet(0);
                UpgradeManager.UpgradeDisplay();
                GetComponent<MeshRenderer>().material.color -= new Color(1, 1, 1, 0.125f);
                GetComponentInChildren<PlatformIcon>().gameObject.SetActive(false);
            }
        }
    }
}
