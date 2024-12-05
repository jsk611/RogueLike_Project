using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompleteUpgrade : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    
    public UpgradeManager.Upgrade type;
    public float degree;
    public string baseText;

    public void UpgradeDone()
    {
        upgradeManager.CompleteUpgrade(type,degree);//CompleteUpgrade(UpgradeManager.Upgrade.AttackSpeed,2); 
    }
}
