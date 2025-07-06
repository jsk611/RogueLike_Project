using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class UI_LvIndicator : MonoBehaviour
{ 
    private enum PerUpgradeType{
        weapon_Unlock,
        basic_atk,
        basic_hp,
        coin_AcqRate,
        heal_Rate,
        atk_UpgradeRate,
        util_UpgradeRate
    }

    //upgrade 레벨 당 강화율
    [SerializeField] PerUpgradeType upgradeType;
    [SerializeField] float upgradeRate = 10f;
    [SerializeField] int upgradeCost = 10;
    [SerializeField] int costIncreaseRate = 5;
    [SerializeField] TextMeshProUGUI LvText;

    //[SerializeField] float basicATK_UpgradeRate = 5f;
    //[SerializeField] float basicHP_UpgradeRate = 10f;
    //[SerializeField] float coinAcq_UpgradeRate = 10f;
    //[SerializeField] float heal_UpgradeRate = 0.1f;
    //[SerializeField] float atk_UpgradeRateUpgradeRate = 0.05f;
    //[SerializeField] float utilUpgradeRateUpgradeRate = 0.05f;


    private Dictionary<PerUpgradeType, Action> UpgradeAction;
    private Dictionary<PerUpgradeType, float> UpgradeRateSet;
    private PlayerStatus player;

    private void Start()
    {
        Initialization();
        DoLVSet();
        player = FindAnyObjectByType<PlayerStatus>();
    }

    #region LevelSet
    private void WeaponLockLvSet()
    {
        int lv = 0;
        for (int i = 0; i < 6; i++)
        {
            if (PermanentUpgradeManager.instance.weaponLockData.GetWeaponLock((WeaponType)i)) lv = i;
        }
        if (lv == 5) LvStringInit("MAX");
        else LvStringInit(lv.ToString());
    }
    private void BasicATKLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.Basic_ATK - 100f) / UpgradeRateSet[upgradeType]) + 1;
        LvStringInit(lv.ToString());
    }
    private void BasicHPLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.Basic_HP - 100f) / UpgradeRateSet[upgradeType]) + 1;
        LvStringInit(lv.ToString());
    }
    private void CoinAcqLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate - 1.0f) / UpgradeRateSet[upgradeType]) +1;
        LvStringInit(lv.ToString());
    }
    private void HealRateLVSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.MaintenanceHealRate - 1.0f) / UpgradeRateSet[upgradeType]) + 1;
        LvStringInit(lv.ToString());
    }
    private void AtkUpgradeRateLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate - 1.0f) / UpgradeRateSet[upgradeType]) + 1;
        LvStringInit(lv.ToString());
    }
    private void UtilUpgradeRateLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate - 1.0f) / UpgradeRateSet[upgradeType]) + 1;
        LvStringInit(lv.ToString());
    }
    private void LvStringInit(string lv)
    {
        string Lv = "Lv.";
        Lv += lv;
        LvText.text = Lv;
    }
    #endregion
    #region Upgrade
    private void WeaponLockUpgrade()
    {
        int lv = 0;
        for (int i = 0; i < 6; i++)
        {
            if (PermanentUpgradeManager.instance.weaponLockData.GetWeaponLock((WeaponType)i)) lv = i;
        }
        if(lv < 5) PermanentUpgradeManager.instance.weaponLockData.UnlockWeapon((WeaponType)(lv + 1));
        WeaponLockLvSet();
    }
    private void BasicATKUpgrade()
    {
        PermanentUpgradeManager.instance.upgradeData.Basic_ATK += UpgradeRateSet[upgradeType];
        BasicATKLvSet();
    }
    private void BasicHPUpgrade()
    {
        PermanentUpgradeManager.instance.upgradeData.Basic_HP += UpgradeRateSet[upgradeType];
        BasicHPLvSet();
    }
    private void CoinAcqUpgrade()
    {
        PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate += UpgradeRateSet[upgradeType];
        CoinAcqLvSet();
    }
    private void HealRateUpgrade()
    {
        PermanentUpgradeManager.instance.upgradeData.MaintenanceHealRate += UpgradeRateSet[upgradeType];
        HealRateLVSet();
    }
    private void AtkRateUpgrade()
    {
        PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate += UpgradeRateSet[upgradeType];
        AtkUpgradeRateLvSet();
    }
    private void UtilRateUpgrade()
    {
        PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate += UpgradeRateSet[upgradeType];
        UtilUpgradeRateLvSet();
    }
    #endregion
    public void DoPermanentUpgrade()
    {
        if (PermanentUpgradeManager.instance.upgradeData.CurrentDNA < upgradeCost)
        {
            Debug.Log("Not enough minerals");
            return;
        }
        player.DecreasePermanentCoin(upgradeCost);
        UpgradeAction[upgradeType].Invoke();
        upgradeCost += costIncreaseRate;
    }
    private void Initialization()
    {
        UpgradeAction = new Dictionary<PerUpgradeType, Action>{
            { PerUpgradeType.weapon_Unlock, WeaponLockUpgrade },
            { PerUpgradeType.basic_atk, BasicATKUpgrade },
            { PerUpgradeType.basic_hp, BasicHPUpgrade },
            { PerUpgradeType.coin_AcqRate,CoinAcqUpgrade },
            { PerUpgradeType.heal_Rate, HealRateUpgrade },
            { PerUpgradeType.atk_UpgradeRate,AtkRateUpgrade },
            { PerUpgradeType.util_UpgradeRate, UtilRateUpgrade }
        };
        UpgradeRateSet = new Dictionary<PerUpgradeType, float>();
        UpgradeRateSet[upgradeType] = upgradeRate;
    }
    private void DoLVSet()
    {
        switch(upgradeType)
        {
            case PerUpgradeType.weapon_Unlock:
                WeaponLockLvSet();
                break;
            case PerUpgradeType.basic_atk:
                BasicATKLvSet();
                break;
            case PerUpgradeType.basic_hp:
                BasicHPLvSet();
                break;
            case PerUpgradeType.coin_AcqRate:
                CoinAcqLvSet();
                break;
            case PerUpgradeType.heal_Rate:
                HealRateLVSet();
                break;
            case PerUpgradeType.atk_UpgradeRate:
                AtkUpgradeRateLvSet();
                break;
            case PerUpgradeType.util_UpgradeRate:
                UtilUpgradeRateLvSet();
                break;
        }
    }

}
