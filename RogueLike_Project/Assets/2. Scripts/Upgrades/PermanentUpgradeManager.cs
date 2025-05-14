using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
    public class UpgradeData {
        public float ATKUpgradeRate = 1.0f;
        public float UTLUpgradeRate = 1.0f;
        public float CoinAcquisitionRate = 1.0f;
        public float MaintenanceHealRate = 1.0f;
    }    
    public class WeaponLockData
    {
        public bool fist = true;
        public bool pistol = true;
        public bool rifle = false;
        public bool sniper = false;
        public bool shotgun = false;
        public bool grenade = false;
    }
public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager instance;
    public WeaponLockData weaponLockData;
    public UpgradeData upgradeData;

    public void Awake()
    {
        DontDestroyOnLoad(this);
        instance = this;

        weaponLockData = new WeaponLockData();
        upgradeData = new UpgradeData();
        SaveData();
        LoadData();
    }
    public void SaveData()
    {
        string weaponLockJSON = JsonUtility.ToJson(weaponLockData);
        string upgradeJSON = JsonUtility.ToJson(upgradeData);
        string pathWeapon =  Path.Combine(Application.dataPath, "WeaponData.json");
        string pathUpgrade = Path.Combine(Application.dataPath, "UpgradeData.json");
        File.WriteAllText(pathWeapon, weaponLockJSON);
        File.WriteAllText (pathUpgrade, upgradeJSON);
        Debug.Log("File Saved");
    }
    private void LoadData()
    {
        string pathWeapon = Path.Combine(Application.dataPath, "WeaponData.json");
        string pathUpgrade = Path.Combine(Application.dataPath, "UpgradeData.json");

        string weaponLockJSON;
        string upgradeJSON;
        if (File.Exists(pathWeapon))
        {
            weaponLockJSON = File.ReadAllText(pathWeapon);
            weaponLockData = JsonUtility.FromJson<WeaponLockData>(weaponLockJSON);

            Debug.Log(weaponLockData.fist);
            Debug.Log(weaponLockData.rifle);
        }
        if (File.Exists(pathUpgrade))
        {
            upgradeJSON = File.ReadAllText(pathUpgrade);
            upgradeData = JsonUtility.FromJson<UpgradeData>(upgradeJSON);
        }
        
    }
    public WeaponLockData GetWeaponLockData()
    {
        return weaponLockData;
    }
    public UpgradeData GetCharacterData()
    {
        return upgradeData;
    }
}
