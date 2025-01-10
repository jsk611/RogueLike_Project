using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompleteUpgrade : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    
    public UpgradeManager.CommonUpgrade commontype;
    public UpgradeManager.RareUpgrade raretype;
    public UpgradeManager.EpicUpgrade epictype;

    public float degree;
    public string baseText;

    public void CommonUpgradeDone()
    {
        upgradeManager.CompleteCommonUpgrade(commontype,degree);//CompleteUpgrade(UpgradeManager.Upgrade.AttackSpeed,2); 
    }
    public void RareUpgradeDone()
    {
        upgradeManager.CompleteRareUpgrade(raretype, degree);
    }
    public void EpicUpgradeDone()
    {
        upgradeManager.CompleteEpicUpgrade(epictype,degree);
    }
}
