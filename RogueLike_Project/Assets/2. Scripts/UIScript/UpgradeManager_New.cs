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

    public UpgradeTier upgradeTier;
    private float curUpgradeLevel;

    GameObject[] UpgradeSet;
    public List<int> upgradeType;

    bool isWaitingInput = false;
    bool upgrading = false;
    CharacterBehaviour player;
    PlayerStatus playerStatus;

    Dictionary<UpgradeTier, Action> upgradeActions;


    public bool Upgrading => upgrading;

    private void Start()
    {
     //   StartCoroutine(UpgradeDisplay());

        upgradeInputField.onEndEdit.AddListener(OnInputEnd);
        decisionInputField.onEndEdit.AddListener(DecisionInputEnd);
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        playerStatus = player.gameObject.GetComponent<PlayerStatus>();

        upgradeActions = new Dictionary<UpgradeTier, Action>
        {
            {UpgradeTier.common,ApplyCommonUpgrade },
            {UpgradeTier.weapon,ApplyWeaponUpgrade },
        };
    }
    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Escape) && isWaitingInput)
    //    {
    //        isWaitingInput = false;
    //        OnInputEnd(inputField.text);
    //    }
    //}
    public IEnumerator DecisionTreeDisplay(int level)
    {
        decisionInputField.transform.gameObject.SetActive(false);
        terminal1.SetActive(true);
        terminal2.SetActive(false);
        decisionInputField.text = "";
        decisionInputField.onEndEdit.AddListener(DecisionInputEnd);
        upgrading = true;
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

        // 3. 인풋필드 활성화
        upgradeInputField.transform.parent.gameObject.SetActive(true);
        isWaitingInput = true;
    }

    UpgradeDecision decisionTypeInput;
    CommonUpgrade commonTypeInput;
    WeaponUpgrade weaponTypeInput;
    //SpecialUpgrade specialUpgrade;
    int upgradeResult = -1;

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
        if (curUpgradeLevel >=1 && decisionTypeInput == UpgradeDecision.BASIC)
        {
            decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
            StartCoroutine(UpgradeDisplay(UpgradeTier.common));
        }
        else if (curUpgradeLevel >=2 && decisionTypeInput == UpgradeDecision.WEAPON)
        {
            decisionInputField.onEndEdit.RemoveListener(DecisionInputEnd);
            StartCoroutine(UpgradeDisplay(UpgradeTier.weapon));
        }
        else if (decisionTypeInput == UpgradeDecision.EXIT)
        {
            upgradeRootUI.SetActive(false);
            player.SetCursorState(true);
            upgrading = false;
        }
       
    }
    void ApplyCommonUpgrade()
    {
        switch (commonTypeInput)
        {
            case CommonUpgrade.ATK:
                switch ((ATKUGType)upgradeResult)
                {
                    case ATKUGType.Damage:
                        //업글 적용
                        Debug.Log("Damage up");
                        playerStatus?.IncreaseAttackDamage(10);
                        break;
                    case ATKUGType.AttackSpeed:
                        //업글 적용
                        Debug.Log("AttackSpeed");
                        playerStatus?.IncreaseAttackSpeed(1);
                        break;
                    case ATKUGType.ReloadSpeed:
                        //업글 적용
                        Debug.Log("ReloadSpeed");
                        playerStatus?.IncreaseReloadSpeed(1);
                        break;
                }
                break;
            case CommonUpgrade.UTIL:
                switch ((UTILUGType)upgradeResult)
                {
                    case UTILUGType.Heath:
                        //업글 적용
                        Debug.Log("Heath");
                        playerStatus.IncreaseMaxHealth(10);
                        playerStatus.IncreaseHealth(10);
                        break;
                    case UTILUGType.MoveSpeed:
                        //업글 적용
                        Debug.Log("MoveSpeed");
                        playerStatus.IncreaseMovementSpeed(1);
                        break;
                    
                }
                break;
            case CommonUpgrade.COIN:
                switch ((COINUGType)upgradeResult)
                {
                    case COINUGType.CoinAcquisitonRate:
                        //업글 적용
                        Debug.Log("CoinAcquisitonRate");
                        playerStatus.IncreaseCoin(1);
                        break;
                    case COINUGType.PermanentCoinAcquisitionRate:
                        //업글 적용
                        Debug.Log("PermanentCoinAcquisitionRate");
                        playerStatus.IncreasePermanentAcquisitionRate(1);
                        break;
                }
                break;
        }
    }
    void ApplyWeaponUpgrade()
    {
        switch(weaponTypeInput)
        {
            case WeaponUpgrade.Blaze:
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
                break;
            case WeaponUpgrade.Freeze:
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
                break;
            case WeaponUpgrade.Shock:
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
    //void ApplyWeaponUpgrade2()
    //{
    //    switch()
    //    {
    //        case RareUpgradeSet.damage:
    //        case RareUpgradeSet.probability:
    //        case RareUpgradeSet.duration:
    //        case RareUpgradeSet.interval:
    //        case RareUpgradeSet.effect:
    //    }
    //}

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
        upgradeSuccess.SetActive(true);
        yield return new WaitForSeconds(3f);
        upgradeRootUI.SetActive(false);

        player.SetCursorState(true);
        upgrading = false;
    }

}



