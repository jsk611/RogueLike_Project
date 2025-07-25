using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

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
    [SerializeField] int maxLevel = 10;
    [SerializeField] float upgradeRate = 10f;
    [SerializeField] int upgradeCost = 10;
    [SerializeField] int costIncreaseRate = 5;
    [SerializeField] TextMeshProUGUI LvText;
    [SerializeField] TextMeshProUGUI CostText;
    [SerializeField] Slider slider;

    //[SerializeField] float basicATK_UpgradeRate = 5f;
    //[SerializeField] float basicHP_UpgradeRate = 10f;
    //[SerializeField] float coinAcq_UpgradeRate = 10f;
    //[SerializeField] float heal_UpgradeRate = 0.1f;
    //[SerializeField] float atk_UpgradeRateUpgradeRate = 0.05f;
    //[SerializeField] float utilUpgradeRateUpgradeRate = 0.05f;


    private Dictionary<PerUpgradeType, Action> UpgradeAction;
    private Dictionary<PerUpgradeType, float> UpgradeRateSet;
    private PlayerStatus player;
    public int curLevel = 1;

    private void Start()
    {
        Initialization();
        player = FindAnyObjectByType<PlayerStatus>();
    }

    #region LevelSet
    private int WeaponLockLvSet()
    {
        int lv = 0;
        for (int i = 0; i < 6; i++)
        {
            if (PermanentUpgradeManager.instance.weaponLockData.GetWeaponLock((WeaponType)i)) lv = i;
        }
        return lv;
    }
    private int BasicATKLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.Basic_ATK - 100f) / UpgradeRateSet[upgradeType]) + 1;
        return lv;
    }
    private int BasicHPLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.Basic_HP - 100f) / UpgradeRateSet[upgradeType]) + 1;
        return lv;
    }
    private int CoinAcqLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate - 1.0f) / UpgradeRateSet[upgradeType]) +1;
        return lv;
    }
    private int HealRateLVSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.MaintenanceHealRate - 1.0f) / UpgradeRateSet[upgradeType]) + 1;
        return lv;
    }
    private int AtkUpgradeRateLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate - 1.0f) / UpgradeRateSet[upgradeType]) + 1;
        return lv;
    }
    private int UtilUpgradeRateLvSet()
    {
        int lv = (int)((PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate - 1.0f) / UpgradeRateSet[upgradeType]) + 1;
        return lv;
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
        if(lv < maxLevel) PermanentUpgradeManager.instance.weaponLockData.UnlockWeapon((WeaponType)(lv + 1));
        WeaponLockLvSet();
        foreach(ItemWeapon item in FindObjectsOfType<ItemWeapon>(true))
        {
            item.CheckUnlocked();
        }
    }
    private void BasicATKUpgrade()
    {
        if(curLevel < maxLevel) PermanentUpgradeManager.instance.upgradeData.Basic_ATK += UpgradeRateSet[upgradeType];
        player.SetAttackDamage(PermanentUpgradeManager.instance.upgradeData.Basic_ATK);
    }
    private void BasicHPUpgrade()
    {
        if(curLevel < maxLevel) PermanentUpgradeManager.instance.upgradeData.Basic_HP += UpgradeRateSet[upgradeType];
        player.SetMaxHealth(PermanentUpgradeManager.instance.upgradeData.Basic_HP);
        player.SetHealth(PermanentUpgradeManager.instance.upgradeData.Basic_HP);
    }
    private void CoinAcqUpgrade()
    {
        if (curLevel < maxLevel) PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate += UpgradeRateSet[upgradeType];
    }
    private void HealRateUpgrade()
    {
        if (curLevel < maxLevel) PermanentUpgradeManager.instance.upgradeData.MaintenanceHealRate += UpgradeRateSet[upgradeType];
    }
    private void AtkRateUpgrade()
    {
        if (curLevel < maxLevel) PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate += UpgradeRateSet[upgradeType];
    }
    private void UtilRateUpgrade()
    {
        if(curLevel < maxLevel) PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate += UpgradeRateSet[upgradeType];
    }
    #endregion
    public void DoPermanentUpgrade()
    {
        if (PermanentUpgradeManager.instance.upgradeData.CurrentDNA < upgradeCost)
        {
            Debug.Log("Not enough minerals");
            return;
        }
        if (curLevel >= maxLevel)
        {
            Debug.Log("Max Level");
            return;
        }
        player.DecreasePermanentCoin(upgradeCost);
        UpgradeAction[upgradeType].Invoke();
        upgradeCost += costIncreaseRate;
        curLevel++;
        slider.value = curLevel;
        CostText.text = "Cost : " + upgradeCost.ToString();
        if (curLevel == maxLevel) LvStringInit("MAX");
        else LvStringInit(curLevel.ToString());
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
        curLevel = DoLVSet();
        LvStringInit(curLevel.ToString());
        slider.value = curLevel;
        CostText.text = "Cost : " + upgradeCost.ToString();
    }
    private int DoLVSet()
    {

        switch(upgradeType)
        {
            case PerUpgradeType.weapon_Unlock: return WeaponLockLvSet();
            case PerUpgradeType.basic_atk: return BasicATKLvSet();
            case PerUpgradeType.basic_hp: return BasicHPLvSet();
            case PerUpgradeType.coin_AcqRate: return CoinAcqLvSet();
            case PerUpgradeType.heal_Rate: return HealRateLVSet();
            case PerUpgradeType.atk_UpgradeRate: return AtkUpgradeRateLvSet();
            case PerUpgradeType.util_UpgradeRate: return UtilUpgradeRateLvSet();
            default: return 1;
        }
    }

}
