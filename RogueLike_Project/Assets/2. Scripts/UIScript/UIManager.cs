using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using System;
using DG.Tweening;
using InfimaGames.LowPolyShooterPack;

public class UIManager : MonoBehaviour
{
    static public UIManager instance = new UIManager();

    public GameObject[] weapon;


    public Text[] curAmmo, maxAmmo;
    Text curammo, maxammo;

    int dna, packet;

    [SerializeField] Image fade;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1.0f;
        fade.gameObject.SetActive(true);
        fade.DOFade(0, 1f);
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

    public float stopwatch = 0;
    public bool isStarted = false;
    [SerializeField] TMP_Text stopwatchText;
    private void Update()
    {
        if (isStarted)
        {
            stopwatch += Time.deltaTime;
            if((int)stopwatch % 60 / 10 == 0) stopwatchText.text = $"{(int)stopwatch / 60}:0{(int)stopwatch % 60}";
            else stopwatchText.text = $"{(int)stopwatch / 60}:{(int)stopwatch % 60}";
        }
        else stopwatchText.text = "0:00";
    }

    //Controlling bars value
    public Image[] Bar;
    [SerializeField] PlayerHPBar_New signalBar;
    [SerializeField] TMP_Text hpStatText;
    public void BarValueChange(int i, float maxValue, float curValue)
    {
        if (i == 0)
        {
            signalBar.ChangeBarValue(curValue, maxValue);
            hpStatText.text = $"<size=12>{(int)curValue}</size> | {(int)maxValue}";
        }
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
    #region MissionUI
    [SerializeField] GameObject missionUI;
    [SerializeField] TMP_Text MissionText;
    [SerializeField] TMP_Text ProgressText;
    [SerializeField] EnemyCountData enemyCountData;

    [SerializeField] GameObject[] bossHPBars;
    Stack<GameObject> barStk = new Stack<GameObject>();
    Dictionary<BossStatus, GameObject> boss_hpBar_Dict = new Dictionary<BossStatus, GameObject>();

    int maxEnemyCount;
    bool isKillingMission = false;
    bool isSurviveMission = false;
    bool isCaptureMission = false;
    bool isItemMission = false;
    public float time;
    public bool isAllBossKilled;
    public void KillingMissionStart()
    {
        isKillingMission = true;
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionStart");
        maxEnemyCount = enemyCountData.enemyCount;
        MissionText.text = "<b><color=orange>목표: </color></b> 적 처치하기";
        KillingMissionUpdate(true);
    }
    public void BossMissionStart()
    {
        isKillingMission = true;
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionStart");
        
        MissionText.text = "<b><color=orange>목표: </color></b> 모든 보스 처치하기";
        foreach (GameObject bar in bossHPBars)
        {
            bar.SetActive(false);
            barStk.Push(bar);
        }
        isAllBossKilled = false;
        ProgressText.text = "";
    }

    public void BossSummoned(BossStatus boss)
    {
        if(barStk.Count <= 0)
        {
            Debug.LogWarning("Serious Problem in Boss Stack");
            return;
        }
        GameObject bar = barStk.Pop();
        bar.SetActive(true);
        boss_hpBar_Dict.Add(boss, bar);
        BossHPBar bossHPBar = bar.GetComponent<BossHPBar>();
        bossHPBar.SetBoss(boss);
    }
    public void BossKilled(BossStatus boss)
    {
        boss_hpBar_Dict[boss].SetActive(false);
        barStk.Push(boss_hpBar_Dict[boss]);
        boss_hpBar_Dict.Remove(boss);

        //보스가 다 죽었는지 확인
        if (boss_hpBar_Dict.Count == 0)
        {
            isAllBossKilled = true;
        }
    }

    void KillingMissionUpdate(bool tmp)
    {
        if(isKillingMission)
        {
            ProgressText.text = (maxEnemyCount-enemyCountData.enemyCount).ToString() + "/" + maxEnemyCount.ToString();
            if (enemyCountData.enemyCount == 0) ProgressText.color = Color.green;
            else ProgressText.color = Color.white;
        }
        else if(isSurviveMission)
        {
            time -= 5f;
        }
    }
    public void SurviveMissionStart(float maxTime)
    {
        isSurviveMission = true;
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionStart");

        time = maxTime;
        MissionText.text = "<b><color=orange>목표: </color></b> 살아남기";
        StartCoroutine(SurviveMissionUpdate());
    }
    IEnumerator SurviveMissionUpdate()
    {
        ProgressText.color = Color.white;
        while(time >= 0)
        {
            time -= Time.deltaTime;
            ProgressText.text = (Mathf.Floor(time * 10f) / 10f).ToString();
            yield return null;
        }
        time = 0;
        ProgressText.text = (Mathf.Floor(time * 10f) / 10f).ToString();
        ProgressText.color = Color.green;
    }
    public void CaptureMissionStart()
    {
        isCaptureMission = true;
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionStart");
        MissionText.text = "<b><color=orange>목표: </color></b> 거점 점령";
    }
    public void CaptureMissionUpdate(float progress)
    {
        ProgressText.text = $"{(int)(progress*100)}%";
        if(progress >= 1) ProgressText.color = Color.green;
        else ProgressText.color = Color.white;
    }
    public int itemCount;
    public void ItemMissionStart(int cnt)
    {
        itemCount = cnt;    
        isItemMission = true;
        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionStart");
        MissionText.text = "<b><color=orange>목표: </color></b> 에너지 전달";
        ProgressText.color = Color.white;
        ProgressText.text = $"{itemCount}회 남음";
    }
    public void ItemMissionUpdate()
    {
        ProgressText.text = $"{--itemCount}회 남음";
        if (itemCount == 0) ProgressText.color = Color.green;
        else ProgressText.color = Color.white;
    }
    public void MissionComplete()
    {
        isKillingMission = false;
        isSurviveMission = false;

        Animator missionUIAnim = missionUI.GetComponent<Animator>();
        missionUIAnim.SetTrigger("MissionEnd");
    }
    #endregion

    #region ProgressUI
    [SerializeField] TMP_Text waveText;
    public void changeWaveText(string title)
    {
        StartCoroutine(ProgressUI.instance.ChangeWaveProgress(title));
        
    }

    #endregion

    [SerializeField] Image DyingBackground;
    [SerializeField] Image NoResp;
    [SerializeField] GameObject DyingParticle;
    public IEnumerator DieBuffering()
    {
        ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().SetCursorState(false);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        DyingBackground.DOFade(0.5f, 1.5f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(1f);
        NoResp.DOFade(0.5f, 1f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.4f);
        DyingParticle.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        PlayerPrefs.SetString("Time", $"{(int)stopwatch / 60}:{(int)stopwatch % 60}");
        PlayerPrefs.SetString("Stage", waveText.text);
        SceneManager.LoadScene("GameOverScene");
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
