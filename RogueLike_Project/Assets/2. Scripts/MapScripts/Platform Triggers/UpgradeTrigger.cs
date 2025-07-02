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
    bool isUpgrading;
    Color originColor;
    // Start is called before the first frame update
    void Start()
    {
        UpgradeManager = ServiceLocator.Current.Get<IGameModeService>().GetUpgradeManager();
        originColor = GetComponent<MeshRenderer>().material.color;
     
    }

    private void OnEnable()
    {
        canUpgrade = 2;
        isUpgrading = false;
        GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color = originColor;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            helpUI.text = uiText + canUpgrade +" left";
            helpUI.enabled = true;
            helpUI.color = Color.cyan;
            if (canUpgrade >0 && Input.GetKeyUp(KeyCode.F) && !UpgradeManager.Upgrading)
            {
                StartCoroutine(UpgradeManager.DecisionTreeDisplay(canUpgrade));
                canUpgrade= 0;
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
