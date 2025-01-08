using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
    bool gotCoin = false;
    public int coinAmount = 100;
    // Start is called before the first frame update
    void Start()
    {
        gotCoin = false;
    }

    private void OnEnable()
    {
        gotCoin = false;
        GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color += new Color(0, 0, 0, 0.125f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!gotCoin && Input.GetKeyDown(KeyCode.F))
            {
                gotCoin = true;
                ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>().IncreasePermanentCoin(coinAmount);
                GetComponent<MeshRenderer>().material.color -= new Color(1, 1, 1, 0.125f);
                GetComponentInChildren<PlatformIcon>().gameObject.SetActive(false);
            }
        }
    }

}
