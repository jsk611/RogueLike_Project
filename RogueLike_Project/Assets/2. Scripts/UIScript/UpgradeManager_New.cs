using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;
using InfimaGames.LowPolyShooterPack;
using static UpgradeManager;

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
    private IAudioManagerService audioManager;
    private InfimaGames.LowPolyShooterPack.AudioSettings audioSetting;

    public UpgradeTier upgradeTier;
    private float curUpgradeLevel;

    private GameObject[] UpgradeSet;
    public List<int> upgradeType;

    bool isWaitingInput = false;
    public bool upgrading = false;
    CharacterBehaviour player;
    PlayerStatus playerStatus;

    Dictionary<UpgradeTier, Action> upgradeActions;

    public bool Upgrading => upgrading;


    private UpgradeDecision decisionTypeInput;
    private CommonUpgrade commonTypeInput;
    private WeaponUpgrade weaponTypeInput;
    private WeaponUpgrade curSelectedType = WeaponUpgrade.Null;
    //SpecialUpgrade specialUpgrade;
    int upgradeResult = -1;
    private void Start()
    {
        upgradeInputField.onEndEdit.AddListener(OnInputEnd);
        decisionInputField.onEndEdit.AddListener(DecisionInputEnd);
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

        // 1. ���׷��̵� Ƽ��� ����ߵ� ����Ʈ��
        switch (upgradeTier)
        {
            case UpgradeTier.common: UpgradeSet = commonUpgradeSet; break;
            case UpgradeTier.weapon: UpgradeSet = weaponUpgradeSet; break;
            case UpgradeTier.special: UpgradeSet = specialUpgradeSet; break;
        }

        if (UpgradeSet == null || UpgradeSet.Length == 0)
        {
            Debug.LogError("UpgradeSet�� ��� �ֽ��ϴ�. Inspector���� Ȯ���ϼ���!");
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

        // 3. ��ǲ�ʵ� Ȱ��ȭ
        upgradeInputField.transform.parent.gameObject.SetActive(true);
        isWaitingInput = true;
    }

    void FilterUpgradeSet(GameObject[] UpgradeSet)
    {
        // 2. Ÿ�� ����Ʈ���� ������ ���׷��̵� ��� ����
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
        Debug.Log(curSelectedType);
        if (decisionTypeInput == UpgradeDecision.BASIC)
        {
            decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
            StartCoroutine(DecisionBasic());

        }
        else if (decisionTypeInput == UpgradeDecision.WEAPON)
        {
            decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
            curSelectedType = weaponTypeInput;
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
                        playerStatus?.IncreaseAttackDamage(10*ATKUpgradeRate);
                        break;
                    case ATKUGType.AttackSpeed:
                        playerStatus?.IncreaseAttackSpeed(1*ATKUpgradeRate);
                        break;
                    case ATKUGType.ReloadSpeed:
                        playerStatus?.IncreaseReloadSpeed(1*ATKUpgradeRate);
                        break;
                }
                break;
            case CommonUpgrade.UTIL:
                float UTLUpgradeRate = PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate;
                switch ((UTILUGType)upgradeResult)
                {
                    case UTILUGType.Heath:
                        playerStatus.IncreaseMaxHealth(10*UTLUpgradeRate);
                        playerStatus.IncreaseHealth(15*UTLUpgradeRate);
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
                        playerStatus.IncreaseCoin((int)(100*COINAcquistionRate));
                        break;
                    case COINUGType.PermanentCoinAcquisitionRate:
                        playerStatus.IncreasePermanentAcquisitionRate(1*COINAcquistionRate);
                        break;
                }
                break;
        }
    }
    void ApplyWeaponUpgrade()
    {
        switch ((WeaponUpgradeSet)upgradeResult)
        {
            case WeaponUpgradeSet.damage:
                WeaponConditionUpgrade(1, 0, 0, 0, 0);
                break;
            case WeaponUpgradeSet.interval:
                WeaponConditionUpgrade(0, 0, 0, 1, 0);
                break;
            case WeaponUpgradeSet.effect:
                WeaponConditionUpgrade(0, 0, 0, 0, 1);
                break;
            case WeaponUpgradeSet.probability:
                WeaponConditionUpgrade(0, 0, 1, 0, 0);
                break;
            case WeaponUpgradeSet.duration:
                WeaponConditionUpgrade(0, 1, 0, 0, 0);
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
                if (weaponBlaze == null) weapon.AddComponent<Blaze>().StateInitializer(1, 1, 1, 1, 1);
                else weaponBlaze.Upgrade(dmg,dur,prob,itv,eff);
                break;
            case WeaponUpgrade.Freeze:
                Freeze weaponFreeze = weapon.GetComponent<Freeze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponFreeze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponFreeze == null) weapon.AddComponent<Freeze>().StateInitializer(1, 1, 1, 1, 1);
                else weaponFreeze.Upgrade(dmg, dur, prob, itv, eff);
                break;
            case WeaponUpgrade.Shock:
                Shock weaponShock = weapon.GetComponent<Shock>();
                if (weapon.GetComponent<WeaponCondition>() != weaponShock) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponShock == null) weapon.AddComponent<Shock>().StateInitializer(1, 1, 1, 1, 1);
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
            progressBarText = "|" + new string('��', barCount) + new string('��', (20-barCount)) + "|";
            upgradeProcessingText[1].text = progressBarText;
            upgradeProcessingText[2].text = $"{progress} / 100%";

            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(0.2f);
        upgradeActions[upgradeTier].Invoke();
        audioManager.PlayOneShotDelayed(upgradeSuccessSound,audioSetting,0.0f);
        upgradeSuccess.SetActive(true);
        yield return new WaitForSeconds(3f);
        upgradeRootUI.SetActive(false);
        player.SetCursorState(true);
        upgrading = false;
        player.SetInteractingUI(upgrading);
    }

}



