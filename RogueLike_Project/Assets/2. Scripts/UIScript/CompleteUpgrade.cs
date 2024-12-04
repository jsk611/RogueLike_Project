using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteUpgrade : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    
    public UpgradeManager.Upgrade type;
    public float degree;

    public void UpgradeDone()
    {
        upgradeManager.CompleteUpgrade(type,degree);//CompleteUpgrade(UpgradeManager.Upgrade.AttackSpeed,2); 
    }
}
