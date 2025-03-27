using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class UpgradeManager_New : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject[] commonUpgradeSet;
    [SerializeField] GameObject[] weaponUpgradeSet;
    [SerializeField] GameObject[] specialUpgradeSet;

    public UpgradeTier upgradeTier;
    GameObject[] UpgradeSet;
    public List<int> upgradeType;

    bool isWaitingInput = false;
    
    private void Start()
    {
        StartCoroutine(UpgradeDisplay());

        upgradeTier = UpgradeTier.common;

        inputField.onEndEdit.AddListener(OnInputEnd);
    }
    IEnumerator UpgradeDisplay()
    {
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
            List<Transform> directChildren = new List<Transform>();

            foreach (Transform child in upgrade.transform)
            {
                directChildren.Add(child);
            }

            if (directChildren.Count > 0)
            {
                int randIdx = Random.Range(0, directChildren.Count);
                directChildren[randIdx].gameObject.SetActive(true);
                upgradeType.Add(randIdx);
            }
        }

        // 3. 인풋필드 활성화
        inputField.transform.parent.gameObject.SetActive(true);
        isWaitingInput = true;
    }
    
    void OnInputEnd(string input)
    {
        if(upgradeTier == UpgradeTier.common && Enum.TryParse(input, out CommonUpgrade result))
        {
            
        }
        
    }
    IEnumerator EndUpgrade()
    {
        yield return new WaitForEndOfFrame();
    }

}
