using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using System;

public class UIManager : MonoBehaviour
{
    static public UIManager instance = new UIManager();

    public GameObject[] weapon;


    public Text[] curAmmo, maxAmmo;
    Text curammo, maxammo;


    int dna, packet;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        EventManager.Instance.MonsterKilledEvent += KillingMissionUpdate;
        packet = PlayerPrefs.GetInt("packet", 0);
        BarValueChange(0, 100, 100);
        BarValueChange(1, 100, 100);
        DNAReset(0);
        PacketReset(packet);
        Swapping(0);
    }

    //Controlling bars value
    public Image[] Bar;
    public void BarValueChange(int i, float maxValue, float curValue)
    {
        Bar[i].fillAmount = curValue / maxValue;
    }

    //Swapping Weapons
    public Image[] UIWeaponImages;
    Image curImage;
    public Sprite[] weaponImages;

    public void Swapping(int index)
    {
        
        curammo = curAmmo[index];
        maxammo = maxAmmo[index];
        curImage = UIWeaponImages[index];

        for (int i = 0; i < 2; i++)
        {
            if (index == i)
            {
                weapon[i].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            }
            else
            {
                weapon[i].transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            }
        }
    }
    public void WeaponImageSwap(GameObject weapon)
    {
        string[] weapons = { "Melee_Weapon 1", "SniperRifle", "Pistol", "Shotgun", "AssaultRifle", "SCi-fi_Grenade 1" };

        for(int i=0; i<6; i++)
        {
            if (weapon.ToString() == weapons[i] + " (UnityEngine.GameObject)") 
            {
                curImage.sprite = weaponImages[i];
                break;
            }
        }
    }

    public void AmmoTextReset(bool knifeActive, int cur, int max)
    {
        if (!knifeActive)
        {
            curammo.text = cur.ToString();
            maxammo.text = max.ToString();
        }
        else
        {
            curammo.text = "-";
            maxammo.text = "-";
        }    

    }


    public TMP_Text packetText;
    public TMP_Text dnaText;
    public void DNAReset(int curDNA)
    {
        if (curDNA == 0)
            dnaText.text = "0";
        else
            dnaText.text = GetThousandCommaText(curDNA).ToString();
    }
    public void PacketReset(int curpacket)
    {
        PlayerPrefs.SetInt("packet", curpacket);

        if (curpacket == 0)
            packetText.text = "0";
        else
            packetText.text = GetThousandCommaText(curpacket).ToString();
    }
    public void dnaIncrease(int amount)
    {
        dna += amount;
        DNAReset(dna);
    }
    public void packetIncrease(int amount)
    {
        packet += amount;
        PacketReset(packet);
    }
    public string GetThousandCommaText(int data)
    {
        return string.Format("{0:#,###}", data);
    }

    /*
    public Image deactivateImage;
    public IEnumerator OnCooltime(float cool)
    {
        float curcool = cool;
        while (curcool > 0)
        {
            curcool -= Time.deltaTime;
            deactivateImage.fillAmount = (curcool / cool);
            yield return null;
        }
    }
    */

    [SerializeField] GameObject missionUI;
    [SerializeField] TMP_Text ProgressText;
    [SerializeField] EnemyCountData enemyCountData;
    int maxEnemyCount;
    public void KillingMissionStart()
    {
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionStart");
        maxEnemyCount = enemyCountData.enemyCount;
        KillingMissionUpdate(true);
    }
    void KillingMissionUpdate(bool tmp)
    {
        ProgressText.text = (maxEnemyCount-enemyCountData.enemyCount).ToString() + "/" + maxEnemyCount.ToString();
        if (enemyCountData.enemyCount == 0) ProgressText.color = Color.green;
        else ProgressText.color = Color.white;
    }
    public void MissionComplete()
    {
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionEnd");
    }
    public GameObject PauseUI;
    public void PauseGame()
    {
        PauseUI.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene("IngameScene");
    }
}
