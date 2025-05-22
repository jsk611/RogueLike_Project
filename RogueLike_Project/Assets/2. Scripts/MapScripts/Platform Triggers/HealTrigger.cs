using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealTrigger : MonoBehaviour
{
    [SerializeField] TMP_Text helpUI;
    [SerializeField] int healCost = 200; 
    string uiText = "Recovery - 100DNA";
    bool isHealed = false;
    
    PlayerStatus ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
        isHealed = false;
    }

    private void OnEnable()
    {
        isHealed = false;
        GetComponentInChildren<PlatformIcon>(true).gameObject.SetActive(true);
        GetComponent<MeshRenderer>().material.color += new Color(0, 0, 0, 0.125f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            helpUI.text = uiText;
            helpUI.enabled = true;
            helpUI.color = Color.green;
            if (!isHealed && Input.GetKeyDown(KeyCode.F) && ps.GetCoin()>=200)
            {
                isHealed = true;
                ps.IncreaseHealth(ps.GetMaxHealth() * 0.25f);
                ps.DecreaseCoin(healCost);
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
