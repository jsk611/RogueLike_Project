using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
    [SerializeField] TMP_Text helpUI;
    string uiText = "Get DNA with ";
    bool isInteracting = false;

    public int coinAmount = 100;
    PlayerStatus ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();

    }

    private void OnEnable()
    {
        GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color += new Color(0, 0, 0, 0.125f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            helpUI.text = uiText + coinAmount + " coins";
            helpUI.enabled = true;
            helpUI.color = Color.yellow;
            if (Input.GetKeyDown(KeyCode.F) && ps.GetCoin()>=coinAmount && !isInteracting)
            {
                isInteracting = true;
                ps.IncreasePermanentCoin(1);
                ps.DecreaseCoin(coinAmount);
                isInteracting=false;
             //   GetComponent<MeshRenderer>().material.color -= new Color(1, 1, 1, 0.125f);
             //   GetComponentInChildren<PlatformIcon>().gameObject.SetActive(false);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) helpUI.enabled = false;
    }
}
