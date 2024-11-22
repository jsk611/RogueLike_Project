using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteUpgrade : MonoBehaviour
{
    UpgradeManager upgradeManager = new UpgradeManager();

    public void UpgradeDone()
    {
        upgradeManager.CompleteUpgrade(); 
    }
}
