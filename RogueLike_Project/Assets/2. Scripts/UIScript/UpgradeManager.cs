using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InfimaGames.LowPolyShooterPack;
using Unity.VisualScripting;


public class UpgradeManager : MonoBehaviour
{
    //public static UpgradeManager instance;

    [SerializeField] GameObject upgradeUI;

    [SerializeField] VarSetDisplayManager varSetDisplayManager;


    [SerializeField] GameObject commonButtons, rareButtons, epicButtons;
    private Button[] curUpgradeButtons = new Button[3]; // 수정된 변수명

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
        MoveSpeed,
        Heath,
        CoinAcquisitonRate,
        PermanentCoinAcquisitionRate
    }
    public enum WeaponUpgrade
    {
        Default,
        ApplyBlaze,
        ApplyFreeze,
        ApplyPoisonous,
        ApplyShock,
    }
    public struct RareUpgradeSet
    {
        public float damage;
        public float duration;
        public float probability;
        public float interval;
        public float effect;
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
        //List<GameObject> selectedButtons = new List<GameObject>();
        GameObject upgradeType = commonButtons;
        if (upgradeLevel == 2) upgradeType = rareButtons;
        else if (upgradeLevel == 3) upgradeType = epicButtons;
       
        curUpgradeButtons = upgradeType.GetComponentsInChildren<Button>(true);
        if (upgradeLevel == 2) curUpgradeButtons = ButtonRandomSelect(curUpgradeButtons,3);
        if(!UIenabled)
        {
            UIenabled = !UIenabled;
            player.SetCursorState(false);
        }
        // 기존 UI 요소 유지
        for (int i = 0; i < curUpgradeButtons.Length; i++)
        {
            if (curUpgradeButtons[i] != null)
            {
                curUpgradeButtons[i].gameObject.SetActive(true);
                StartCoroutine(Typing(curUpgradeButtons[i].gameObject));
            }
            else
            {
                Debug.LogError($"Upgrade button at index {i} is null.");
            }
        }
    }
    Button[] ButtonRandomSelect(Button[] buttonGroups,int selectN)
    {
        Button[] selectedButtons = new Button[selectN];
        List<Button> buttons = new List<Button>(buttonGroups);  
        for (int i = 0;i<selectN;i++)
        {
            int randomIndex = Random.Range(0,buttons.Count);
            selectedButtons[i] = buttons[randomIndex];
            buttons.RemoveAt(randomIndex);
        }
        return selectedButtons;
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
        for(int i = 0; i < curUpgradeButtons.Length; i++)
        {
            curUpgradeButtons[i].gameObject.SetActive(false);
        }
        varSetDisplayManager.ResetVarSet();
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
    public void CompleteRareUpgrade(WeaponUpgrade type,RareUpgradeSet upgradeSet)
    {
        WeaponConditionUpgrade(type, upgradeSet);
        for (int i = 0; i < curUpgradeButtons.Length; i++)
        {
            curUpgradeButtons[i].gameObject.SetActive(false);
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
    private void WeaponConditionUpgrade(WeaponUpgrade type, RareUpgradeSet upgradeSet)
    {
        WeaponBehaviour weapon = player.GetInventory().GetEquipped();
        switch (type)
        {
            case WeaponUpgrade.ApplyBlaze:
                Blaze weaponBlaze = weapon.GetComponent<Blaze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponBlaze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponBlaze == null) weapon.AddComponent<Blaze>().StateInitializer(20, 1, 25, 1, 1);
            //    else weaponBlaze.Upgrade(upgradeSet);
                    break;
            case WeaponUpgrade.ApplyFreeze:
                Freeze weaponFreeze = weapon.GetComponent<Freeze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponFreeze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponFreeze == null) weapon.AddComponent<Freeze>().StateInitializer(20, 1, 25, 1, 1);
                //         else weaponFreeze.Upgrade(upgradeSet);
                break;
            case WeaponUpgrade.ApplyShock:
                Shock weaponShock = weapon.GetComponent<Shock>();
                if (weapon.GetComponent<WeaponCondition>() != weaponShock) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponShock == null) weapon.AddComponent<Shock>().StateInitializer(20, 1, 25, 1, 1);
                //     else weaponShock.Upgrade(upgradeSet);
                break;
            default:
                break;
        }
    }
    public void CompleteEpicUpgrade(EpicUpgrade type, float degree)
    {

    }

    public void VarSetDisplay(int type)
    {
        Debug.Log("asdfasdf");
        varSetDisplayManager.ShowVarSet(type);
    }
    IEnumerator Typing(GameObject curButton)
    {
        TMP_Text tx = curButton.transform.GetChild(1).GetComponent<TMP_Text>();
        string temptx = tx.text;
        tx.text = "";

        for(int i = 0; i <= temptx.Length; i++)
        {
            tx.text = temptx.Substring(0, i);
            yield return new WaitForSeconds(0.06f);
        }
    }
}
