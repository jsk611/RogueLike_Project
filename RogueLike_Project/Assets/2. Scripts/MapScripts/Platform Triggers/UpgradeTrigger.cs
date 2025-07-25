using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTrigger : MonoBehaviour
{
    [SerializeField] TMP_Text helpUI;
    [SerializeField] Color originColor;
    [SerializeField] int UpgradeCost = 500;

    UpgradeManager_New UpgradeManager;
    PlayerStatus player;
    int canUpgrade;
    // Start is called before the first frame update
    void Start()
    {
        UpgradeManager = ServiceLocator.Current.Get<IGameModeService>().GetUpgradeManager();
        originColor = GetComponent<MeshRenderer>().material.color;
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
    }

    private void OnEnable()
    {
        canUpgrade = 2;
        GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.SetColor("_Tint", originColor);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            helpUI.text = "Upgradable (500 DNA)";
            helpUI.enabled = true;
            helpUI.color = Color.cyan;
            if (canUpgrade >0 && player.GetCoin() >= UpgradeCost && Input.GetKeyUp(KeyCode.F) && !UpgradeManager.Upgrading)
            {
                player.DecreaseCoin(UpgradeCost);
                StartCoroutine(UpgradeManager.DecisionTreeDisplay(2));
                canUpgrade= 0;
            }
            else if (canUpgrade <= 0)
            {
                helpUI.text = "NULL";
                GetComponent<MeshRenderer>().material.SetColor("_Tint", originColor - new Color(1, 1, 1, 0));
                GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(false);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            helpUI.enabled = false;
    }
}
