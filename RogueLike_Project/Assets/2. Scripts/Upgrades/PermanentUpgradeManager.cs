using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using JetBrains.Annotations;
public class UpgradeData {
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
    private bool[] weaponLock;
    public bool fist;

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
        instance = this;

        weaponLockData = new WeaponLockData();
        upgradeData = new UpgradeData();
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
        if (PlayerPrefs.GetInt("isNewGame") == 1)
        {
            PlayerPrefs.SetInt("isNewGame", 0);
            WeaponLockData weaponLockData = new WeaponLockData();
            UpgradeData upgradeData = new UpgradeData();
            SettingData settingData = new SettingData();
        }
        else
        {
            string pathWeapon = Path.Combine(Application.dataPath, "WeaponData.json");
            string pathUpgrade = Path.Combine(Application.dataPath, "UpgradeData.json");

            string weaponLockJSON;
            string upgradeJSON;
            if (File.Exists(pathWeapon))
            {
                weaponLockJSON = File.ReadAllText(pathWeapon);
                weaponLockData = JsonUtility.FromJson<WeaponLockData>(weaponLockJSON);
            }
            if (File.Exists(pathUpgrade))
            {
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
