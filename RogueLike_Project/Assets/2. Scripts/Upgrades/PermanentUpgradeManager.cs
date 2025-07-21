using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using JetBrains.Annotations;
public class UpgradeData {
    public int CurrentDNA = 1000;
    public float Basic_ATK = 100.0f;
    public float Basic_HP = 100.0f;
    public float ATKUpgradeRate = 1.0f;
    public float UTLUpgradeRate = 1.0f;
    public float CoinAcquisitionRate = 1.0f;
    public float MaintenanceHealRate = 1.0f;
}    
public enum WeaponType
{
    fist,
    pistol,
    rifle,
    sniper,
    shotgun,
    grenade
}
public class WeaponLockData
{
    [SerializeField] private bool[] weaponLock;

    public bool GetWeaponLock(WeaponType index)
    {
        return weaponLock[(int)index];
    }
    public void UnlockWeapon(WeaponType index)
    {
        weaponLock[(int)index] = true;
    }
    public WeaponLockData()
    {
        weaponLock = new bool[6];
        for (int i = 0;i<6;i++) weaponLock[i] = false;
        weaponLock[(int)WeaponType.fist] = true;
        weaponLock[(int)WeaponType.pistol] = true;
    }
}
public class SettingData
{
    public Vector2 Screen;
    public Vector2 Resolution;

    public float MainSound;
    public float BGM;
    public float SoundEffect;

    public string Language;
}
public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager instance;
    public WeaponLockData weaponLockData;
    public UpgradeData upgradeData;
    public SettingData settingData;

 
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        upgradeData = new UpgradeData();
        weaponLockData = new WeaponLockData();
        settingData = new SettingData();
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
    public void LoadData()
    {
        if (PlayerPrefs.GetInt("isNewGame") == 1)
        {
            PlayerPrefs.SetInt("isNewGame", 0);
            weaponLockData = new WeaponLockData();
            upgradeData = new UpgradeData();
            settingData = new SettingData();
        }
        else
        {
            string pathWeapon = Path.Combine(Application.dataPath, "WeaponData.json");
            string pathUpgrade = Path.Combine(Application.dataPath, "UpgradeData.json");

            string weaponLockJSON;
            string upgradeJSON;
            if (File.Exists(pathWeapon))
            {
                Debug.Log("Weapon File Exist");
                weaponLockJSON = File.ReadAllText(pathWeapon);
                weaponLockData = JsonUtility.FromJson<WeaponLockData>(weaponLockJSON);
            }
            if (File.Exists(pathUpgrade))
            {
                Debug.Log("Upgrade File Exist");
                upgradeJSON = File.ReadAllText(pathUpgrade);
                upgradeData = JsonUtility.FromJson<UpgradeData>(upgradeJSON);
            }
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
