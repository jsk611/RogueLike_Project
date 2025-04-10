using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTrigger : MonoBehaviour
{
    [SerializeField] TMP_Text helpUI;
    string uiText = "Upgrade - ";

    UpgradeManager_New UpgradeManager;
    int canUpgrade;
    // Start is called before the first frame update
    void Start()
    {
        UpgradeManager = ServiceLocator.Current.Get<IGameModeService>().GetUpgradeManager();
    }

    private void OnEnable()
    {
        canUpgrade = 2;
        GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color += new Color(0, 0, 0, 0.125f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            helpUI.text = uiText + canUpgrade +" left";
            helpUI.enabled = true;
            helpUI.color = Color.cyan;
            if (canUpgrade > 0 && Input.GetKeyUp(KeyCode.F))
            {
                StartCoroutine(UpgradeManager.DecisionTreeDisplay(canUpgrade));
                canUpgrade--;
            }
            else if (canUpgrade <= 0)
            {
                GetComponent<MeshRenderer>().material.color -= new Color(1, 1, 1, 0.125f);
                GetComponentInChildren<PlatformIcon>().gameObject.SetActive(false);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            helpUI.enabled = false;
    }
}
