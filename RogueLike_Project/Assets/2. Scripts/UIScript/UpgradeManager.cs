using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InfimaGames.LowPolyShooterPack;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.GraphView;

public class UpgradeManager : MonoBehaviour
{
    //public static UpgradeManager instance;

    [SerializeField] GameObject upgradeUI;

    [SerializeField] GameObject[] commonButtons, rareButtons, epicButtons;
    private GameObject[] curUpgradeButtons = new GameObject[3]; // 수정된 변수명

    private CharacterBehaviour player;
    private PlayerStatus status;

    public bool UIenabled = false;

    private int repeatCommon = 0;
    private int repeatRare = 0;
    private int repeatEpic = 0;

    public enum CommonUpgrade
    {
        Default,
        Damage,
        AttackSpeed,
        ReloadSpeed,
        CriticalRate,
        CriticalDamage,
        MoveSpeed,
        Heath,
        CoinAcquisitonRate,
        PermanentCoinAcquisitionRate
    }
    public enum RareUpgrade
    {
        Default,
        ApplyBlaze,
        ApplyFreeze,
        ApplyPoisonous,
        ApplyShock,
    }
    public enum EpicUpgrade
    {
        Default,
        Pain_Stress,
        Berserk,
        K_Ampule_Activation,
        Cavity_System_Model,
        Havel_The_Rock
    }

    private void Awake()
    {
       // instance = this;
    }

    private void Start()
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = player.GetComponent<PlayerStatus>();
    }

    public void RepeatNumSet(int common,int rare,int epic)
    {
        repeatCommon = common;
        repeatRare = rare;
        repeatEpic = epic;
    }

    public void UpgradeDisplay(int upgradeLevel = 1)
    {
        upgradeUI.SetActive(true);

        // 중복 방지를 위한 리스트
        List<GameObject> selectedButtons = new List<GameObject>();
        GameObject[] selectList = commonButtons;
        if (upgradeLevel == 2) selectList = rareButtons;
        else if (upgradeLevel == 3) selectList = epicButtons;
        

        for (int i = 0; i < 3; i++)
        {
            GameObject selectedButton;
            selectedButton = selectList[Random.Range(0, selectList.Length)];

            // 중복 체크
            while (selectedButton != null && selectedButtons.Contains(selectedButton))
            {
                if (selectList.Length > 0)
                {
                    selectedButton = selectList[Random.Range(0, rareButtons.Length)];
                }
            }
            curUpgradeButtons[i] = selectedButton;
            selectedButtons.Add(selectedButton); // 선택된 버튼을 리스트에 추가
        }
        if(!UIenabled)
        {
            UIenabled = !UIenabled;
            player.SetCursorState(false);
        }
        // 기존 UI 요소 유지
        
        for (int i = 0; i < 3; i++)
        {
            if (curUpgradeButtons[i] != null)
            {
                curUpgradeButtons[i].SetActive(true);
                StartCoroutine(Typing(curUpgradeButtons[i]));
            }
            else
            {
                Debug.LogError($"Upgrade button at index {i} is null.");
            }
        }
    }

    public void CompleteCommonUpgrade(CommonUpgrade type,float degree)
    {
        switch(type)
        {
            case CommonUpgrade.AttackSpeed:
                status.IncreaseAttackSpeed(degree);
                break;
            case CommonUpgrade.ReloadSpeed:
                status.IncreaseReloadSpeed(degree);
                break;
            case CommonUpgrade.MoveSpeed:
                status.IncreaseMovementSpeed(degree);
                break;
            case CommonUpgrade.CriticalRate:
                status.IncreaseCriticalRate(degree);
                break;
            case CommonUpgrade.CriticalDamage:
                status.IncreaseCriticalDamage(degree);
                break;
            case CommonUpgrade.Damage:
                status.IncreaseAttackDamage(degree);
                break;
            case CommonUpgrade.Heath:
                status.IncreaseMaxHealth(degree);
                status.IncreaseHealth(degree);
                break;
            case CommonUpgrade.CoinAcquisitonRate:
                status.IncreaseAcquisitionRate(degree);
                break;
            case CommonUpgrade.PermanentCoinAcquisitionRate:
                status.IncreasePermanentAcquisitionRate(degree);
                break;
            default:
                break;
        }
        for(int i = 0; i < 3; i++)
        {
            curUpgradeButtons[i].SetActive(false);
        }
        upgradeUI.SetActive(false);

        if (repeatCommon > 0)
        {
            StopAllCoroutines();
            repeatCommon--;
            UpgradeDisplay();
        }
        else
        {
            if(repeatRare > 0 || repeatEpic > 0)
            {
                StopAllCoroutines();
                UpgradeDisplay(2);
                return;
            }
            UIenabled = !UIenabled;
            player.SetCursorState(true);
        }
    }
    public void CompleteRareUpgrade(RareUpgrade type, float degree)
    {
        WeaponBehaviour weapon = player.GetInventory().GetEquipped();
        //if (weapon.GetComponent<Blaze>() != null) { Destroy(weapon.GetComponent<Blaze>()); status.IncreaseCoin(375); }
        //if (weapon.GetComponent<Freeze>() != null) { Destroy(weapon.GetComponent<Freeze>()); status.IncreaseCoin(375); }
        //if (weapon.GetComponent<Poison>() != null) { Destroy(weapon.GetComponent<Poison>()); status.IncreaseCoin(375); }
        //if (weapon.GetComponent<Shock>() != null) { Destroy(weapon.GetComponent<Shock>()); status.IncreaseCoin(375); }
        if(weapon.GetComponent<WeaponCondition>() != null) { Destroy(weapon.GetComponent<WeaponCondition>()); status.IncreaseCoin(375); }
        

        switch (type)
        {
            case RareUpgrade.ApplyBlaze:
                weapon.AddComponent<Blaze>().StateInitializer(2,2,1);
                break;
            case RareUpgrade.ApplyFreeze:
                weapon.AddComponent<Freeze>().StateInitializer(2,2,1);
                break;
            case RareUpgrade.ApplyPoisonous:
                weapon.AddComponent<Poison>().StateInitializer(2, 2, 1);
                break;
            case RareUpgrade.ApplyShock:
                weapon.AddComponent<Shock>().StateInitializer(2, 2, 1);
                break;
            default:
                break;
        }
        
        for (int i = 0; i < 3; i++)
        {
            curUpgradeButtons[i].SetActive(false);
        }
        upgradeUI.SetActive(false);

        if (repeatRare > 0)
        {
            StopAllCoroutines();
            repeatRare--;
            UpgradeDisplay(2);
        }
        else
        {
            if(repeatEpic > 0)
            {
                StopAllCoroutines();
                UpgradeDisplay(3);
                return;
            }
            UIenabled = !UIenabled;
            player.SetCursorState(true);
        }
    }
    public void CompleteEpicUpgrade(EpicUpgrade type, float degree)
    {

    }

    IEnumerator Typing(GameObject curButton)
    {
        TMP_Text tx = curButton.transform.GetChild(1).GetComponent<TMP_Text>();
        string temptx = curButton.GetComponent<CompleteUpgrade>().baseText;
        tx.text = "";

        for(int i = 0; i <= temptx.Length; i++)
        {
            tx.text = temptx.Substring(0, i);
            yield return new WaitForSeconds(0.06f);
        }
    }
}
