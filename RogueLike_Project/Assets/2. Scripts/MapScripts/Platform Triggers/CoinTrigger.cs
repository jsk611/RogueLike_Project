using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
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
            if (Input.GetKeyDown(KeyCode.F) && ps.GetCoin()>=coinAmount)
            {
                ps.IncreasePermanentCoin(1);
                ps.DecreaseCoin(coinAmount);
             //   GetComponent<MeshRenderer>().material.color -= new Color(1, 1, 1, 0.125f);
             //   GetComponentInChildren<PlatformIcon>().gameObject.SetActive(false);
            }
        }
    }

}
