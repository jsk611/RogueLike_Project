using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;
using InfimaGames.LowPolyShooterPack;

public class UpgradeManager_New : MonoBehaviour
{
    [SerializeField] GameObject upgradeRootUI;
    [SerializeField] GameObject terminal1;
    [SerializeField] GameObject terminal2;

    [SerializeField] TMP_InputField upgradeInputField;
    [SerializeField] TMP_InputField decisionInputField;

    [SerializeField] GameObject[] UpgradeDecisionSet;
    [SerializeField] GameObject[] commonUpgradeSet;
    [SerializeField] GameObject[] weaponUpgradeSet;
    [SerializeField] GameObject[] specialUpgradeSet;

    [SerializeField] GameObject upgradeProcessing;
    [SerializeField] GameObject upgradeSuccess;

    [SerializeField] AudioClip upgradeSuccessSound;

    [Header("Upgrade Text UI")]
    [SerializeField] TextMeshProUGUI[] ATK_Texts;
    [SerializeField] TextMeshProUGUI[] UTL_Texts;
    [SerializeField] TextMeshProUGUI[] Coin_Texts;

    private IAudioManagerService audioManager;
    private InfimaGames.LowPolyShooterPack.AudioSettings audioSetting;

    public UpgradeTier upgradeTier;
    private float curUpgradeLevel;

    private GameObject[] UpgradeSet;
    public List<int> upgradeType;

    public bool upgrading = false;
    CharacterBehaviour player;
    PlayerStatus playerStatus;

    Dictionary<UpgradeTier, Action> upgradeActions;

    public bool Upgrading => upgrading;


    private UpgradeDecision decisionTypeInput;
    private CommonUpgrade commonTypeInput;
    private WeaponUpgrade weaponTypeInput;
    //SpecialUpgrade specialUpgrade;
    int upgradeResult = -1;
    private void Start()
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        playerStatus = player.gameObject.GetComponent<PlayerStatus>();

        audioManager = ServiceLocator.Current.Get<IAudioManagerService>();
        audioSetting = new InfimaGames.LowPolyShooterPack.AudioSettings(1.0f, 0.0f, true);

        upgradeActions = new Dictionary<UpgradeTier, Action>
        {
            {UpgradeTier.common,ApplyCommonUpgrade },
            {UpgradeTier.weapon,ApplyWeaponUpgrade },
        };

    }

    public IEnumerator DecisionTreeDisplay(int level)
    {
        player.CancelAiming();
        ExternSoundManager.instance.PauseBGM();
        UpgradeValueEdit();
        decisionInputField.transform.gameObject.SetActive(false);
        terminal1.SetActive(true);
        terminal2.SetActive(false);
        decisionInputField.text = "";
        decisionInputField.onEndEdit.AddListener(DecisionInputEnd);
        upgrading = true;
        player.SetInteractingUI(upgrading);
        curUpgradeLevel = level;
        foreach (GameObject types in UpgradeDecisionSet) types.SetActive(false);

        yield return new WaitForEndOfFrame();

        for (int i = 0; i <= level; i++) UpgradeDecisionSet[i].SetActive(true);
        upgradeRootUI.SetActive(true);
        player.SetCursorState(false);
        decisionInputField.transform.gameObject.SetActive(true);
    }
    public void BasicUpgradeCall()
    {
        player.CancelAiming();
        UpgradeValueEdit();
        ExternSoundManager.instance.PauseBGM();
        terminal1.SetActive(false);
        curUpgradeLevel = 1;
        decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
        StartCoroutine(UpgradeDisplay(UpgradeTier.common));
    }
    public IEnumerator UpgradeDisplay(UpgradeTier tier)
    {
        terminal2.SetActive(true);
        decisionInputField.transform.gameObject.SetActive(false);

        upgrading = true;
        player.SetInteractingUI(upgrading);
        upgradeInputField.text = "";
        upgradeInputField.onEndEdit.AddListener(OnInputEnd);
        upgradeTier = tier;

        yield return new WaitForEndOfFrame();
        upgradeRootUI.SetActive(true);
        player.SetCursorState(false);

        upgradeProcessing.SetActive(false);
        upgradeSuccess.SetActive(false);

        if(UpgradeSet != null)
        {
            foreach (GameObject upgrade in UpgradeSet)
            {
                upgrade.SetActive(false);
            }
        }
        upgradeType = new List<int>();
        yield return new WaitForSeconds(0.2f);

        // 1. 업그레이드 티어별로 띄워야될 리스트업
        switch (upgradeTier)
        {
            case UpgradeTier.common: UpgradeSet = commonUpgradeSet; break;
            case UpgradeTier.weapon: UpgradeSet = weaponUpgradeSet; break;
            case UpgradeTier.special: UpgradeSet = specialUpgradeSet; break;
        }

        if (UpgradeSet == null || UpgradeSet.Length == 0)
        {
            Debug.LogError("UpgradeSet이 비어 있습니다. Inspector에서 확인하세요!");
            yield break;
        }
        foreach (GameObject upgrade in UpgradeSet)
        {
            upgrade.SetActive(true);
            List<Transform> directChildren = new List<Transform>();

            foreach (Transform child in upgrade.transform)
            {
                directChildren.Add(child);
                child.gameObject.SetActive(false);
            }

            if (directChildren.Count > 0)
            {
                int randIdx = Random.Range(0, directChildren.Count);
                directChildren[randIdx].gameObject.SetActive(true);
                upgradeType.Add(randIdx);
            }
        }

        // 3. 인풋필드 활성화
        upgradeInputField.transform.parent.gameObject.SetActive(true);
    }

    void FilterUpgradeSet(GameObject[] UpgradeSet)
    {
        // 2. 타입 리스트별로 결정할 업그레이드 요소 결정
        foreach (GameObject upgrade in UpgradeSet)
        {
            upgrade.SetActive(true);
            List<Transform> directChildren = new List<Transform>();

            foreach (Transform child in upgrade.transform)
            {
                directChildren.Add(child);
                child.gameObject.SetActive(false);
            }

            if (directChildren.Count > 0)
            {
                int randIdx = Random.Range(0, directChildren.Count);
                directChildren[randIdx].gameObject.SetActive(true);
                upgradeType.Add(randIdx);
            }
        }
    }

    void DecisionInputEnd(string input)
    {
        if (Enum.TryParse(input, true, out decisionTypeInput))
            DecisionTree();
    }
    void OnInputEnd(string input)
    {
        if(curUpgradeLevel >=1 && upgradeTier == UpgradeTier.common && Enum.TryParse(input, true, out commonTypeInput))
        {
            upgradeResult = upgradeType[(int)commonTypeInput];
            upgradeInputField.onEndEdit.RemoveListener(OnInputEnd);
        }
        else if(curUpgradeLevel >=2 && upgradeTier == UpgradeTier.weapon && Enum.TryParse(input,true,out weaponTypeInput))
        {
            upgradeResult = upgradeType[(int)weaponTypeInput];
            upgradeInputField.onEndEdit.RemoveListener(OnInputEnd);
        }
        //else if (upgradeTier == UpgradeTier.special && Enum.TryParse(input, out int result))
        //{
        //    upgradeResult = upgradeType[(int)result];
        //}
        else
        {
            Debug.Log("Wrong Input");
            return;
        }
        StartCoroutine(EndUpgrade());
    }
    void DecisionTree()
    {
        if (decisionTypeInput == UpgradeDecision.BASIC)
        {
            decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
            StartCoroutine(DecisionBasic());
        }
        else if (decisionTypeInput == UpgradeDecision.WEAPON)
        {
            decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
            StartCoroutine(UpgradeDisplay(UpgradeTier.weapon));
        }
        else if (decisionTypeInput == UpgradeDecision.EXIT)
        {
            upgradeRootUI.SetActive(false);
            player.SetCursorState(true);
            upgrading = false;
            player.SetCursorState(upgrading);
        }
    }
    IEnumerator DecisionBasic()
    {
        StartCoroutine(UpgradeDisplay(UpgradeTier.common));
        yield return new WaitUntil(() => !upgrading);
        StartCoroutine(UpgradeDisplay(UpgradeTier.common));
    }
    void ApplyCommonUpgrade()
    {
        switch (commonTypeInput)
        {
            case CommonUpgrade.ATK:
                float ATKUpgradeRate = PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate;
                switch ((ATKUGType)upgradeResult)
                {
                    case ATKUGType.Damage:
                        playerStatus?.IncreaseAttackDamage(15*ATKUpgradeRate);
                        break;
                    case ATKUGType.AttackSpeed:
                        playerStatus?.IncreaseAttackSpeed(6*ATKUpgradeRate);
                        break;
                    case ATKUGType.ReloadSpeed:
                        playerStatus?.IncreaseReloadSpeed(7*ATKUpgradeRate);
                        break;
                }
                break;
            case CommonUpgrade.UTIL:
                float UTLUpgradeRate = PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate;
                switch ((UTILUGType)upgradeResult)
                {
                    case UTILUGType.Heath:
                        playerStatus.IncreaseMaxHealth(15*UTLUpgradeRate);
                        playerStatus.IncreaseHealth(20*UTLUpgradeRate);
                        break;
                    case UTILUGType.MoveSpeed:
                        playerStatus.IncreaseMovementSpeed(1*UTLUpgradeRate);
                        break;
                }
                break;
            case CommonUpgrade.COIN:
                float COINAcquistionRate = PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate;
                switch ((COINUGType)upgradeResult)
                {
                    case COINUGType.CoinAcquisitonRate:
                        playerStatus.IncreaseCoin(100);
                        break;
                    case COINUGType.PermanentCoinAcquisitionRate:
                        playerStatus.IncreaseCoin(100);
                        break;
                }
                break;
        }
    }
    void ApplyWeaponUpgrade()
    {
        switch ((WeaponUpgradeSet)upgradeResult)
        {
            case WeaponUpgradeSet.probability:
                WeaponConditionUpgrade(0, 0, 15, 0, 0);
                break;
            case WeaponUpgradeSet.damage:
                WeaponConditionUpgrade(20, 0, 0, 0, 0);
                break;
            case WeaponUpgradeSet.duration:
                WeaponConditionUpgrade(0, 1, 0, 0, 0);
                break;
            case WeaponUpgradeSet.interval:
                WeaponConditionUpgrade(0, 0, 0, 1, 0);
                break;
            case WeaponUpgradeSet.effect:
                WeaponConditionUpgrade(0, 0, 0, 0, 1);
                break;
        }
    }
    private void WeaponConditionUpgrade(float dmg, float dur, float prob, float itv, float eff)
    {
        WeaponBehaviour weapon = player.GetInventory().GetEquipped();
        switch (weaponTypeInput)
        {
            case WeaponUpgrade.Blaze:
                Blaze weaponBlaze = weapon.GetComponent<Blaze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponBlaze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponBlaze == null) weapon.AddComponent<Blaze>().StateInitializer(20, 1, 25, 1, 1);
                else weaponBlaze.Upgrade(dmg,dur,prob,itv,eff);
                break;
            case WeaponUpgrade.Freeze:
                Freeze weaponFreeze = weapon.GetComponent<Freeze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponFreeze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponFreeze == null) weapon.AddComponent<Freeze>().StateInitializer(20, 1, 25, 1, 1);
                else weaponFreeze.Upgrade(dmg, dur, prob, itv, eff);
                break;
            case WeaponUpgrade.Shock:
                Shock weaponShock = weapon.GetComponent<Shock>();
                if (weapon.GetComponent<WeaponCondition>() != weaponShock) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponShock == null) weapon.AddComponent<Shock>().StateInitializer(20, 1, 25, 1, 1);
                else weaponShock.Upgrade(dmg, dur, prob, itv, eff);
                break;
            default:
                break;
        }
    }

    IEnumerator EndUpgrade()
    {
        yield return new WaitForEndOfFrame();
        upgradeProcessing.SetActive(true);
        TMP_Text[] upgradeProcessingText = upgradeProcessing.GetComponentsInChildren<TMP_Text>();
        int progress = 0;
        string progressBarText = "|";
        while(progress < 100)
        {
            progress += Random.Range(0, 16);
            if(progress > 100) progress = 100;

            int barCount = (int)(progress / 100f * 20);
            progressBarText = "|" + new string('■', barCount) + new string('□', (20-barCount)) + "|";
            upgradeProcessingText[1].text = progressBarText;
            upgradeProcessingText[2].text = $"{progress} / 100%";

            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(0.2f);
        upgradeActions[upgradeTier].Invoke();
        audioManager.PlayOneShotDelayed(upgradeSuccessSound,audioSetting,0.0f);
        upgradeSuccess.SetActive(true);
        yield return new WaitForSeconds(3f);
        ExternSoundManager.instance.ResumeBGM();
        upgradeRootUI.SetActive(false);
        player.SetCursorState(true);
        upgrading = false;
        player.SetInteractingUI(upgrading);
    }

    private void UpgradeValueEdit()
    {
        UpgradeData temp = PermanentUpgradeManager.instance.upgradeData;
        {
            ATK_Texts[0].text = string.Format("Damage + {0:F2}",15 * temp.ATKUpgradeRate);
            ATK_Texts[1].text = string.Format("AttackSpeed + {0:F2}%", 6 * temp.ATKUpgradeRate);
            ATK_Texts[2].text = string.Format("ReloadingSpeed + {0:F2}%", 7 * temp.ATKUpgradeRate);
        }
        {
            UTL_Texts[0].text = string.Format("MoveSpeed + {0:F2}", 1 * temp.UTLUpgradeRate);
            UTL_Texts[1].text = string.Format("Health + {0:D}",(int)(15 * temp.UTLUpgradeRate));
        }
        {
            Coin_Texts[0].text = string.Format("Coin + {0:D}", (int)(100 * temp.CoinAcquisitionRate));
            Coin_Texts[1].text = Coin_Texts[0].text;
        }
    }
}



