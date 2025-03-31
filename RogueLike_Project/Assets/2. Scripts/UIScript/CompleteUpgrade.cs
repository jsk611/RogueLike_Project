using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompleteUpgrade : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    
    [SerializeField] UpgradeManager.CommonUpgrade commontype;
    [SerializeField] float degree;

    [SerializeField] UpgradeManager.WeaponUpgrade raretype;
    [SerializeField] float damage;
    [SerializeField] float duration;
    [SerializeField] float probability;
    [SerializeField] float interval;
    [SerializeField] float effect;
    UpgradeManager.RareUpgradeSet upgradeSet;

    [SerializeField] UpgradeManager.EpicUpgrade epictype;
    
    public string baseText;

    public void CommonUpgradeDone()
    {
        upgradeManager.CompleteCommonUpgrade(commontype,degree);//CompleteUpgrade(UpgradeManager.Upgrade.AttackSpeed,2); 
    }
    public void RareUpgradeDone()
    {
        upgradeSet.damage = damage;
        upgradeSet.duration = duration;
        upgradeSet.probability = probability;
        upgradeSet.interval = interval;
        upgradeSet.effect = effect;

        upgradeManager.CompleteRareUpgrade(raretype, upgradeSet);
    }
    public void EpicUpgradeDone()
    {
        upgradeManager.CompleteEpicUpgrade(epictype,degree);
    }
}
