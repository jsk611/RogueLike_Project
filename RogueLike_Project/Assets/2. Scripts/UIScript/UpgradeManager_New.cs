using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager_New : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject[] commonUpgradeSet;
    [SerializeField] GameObject[] weaponUpgradeSet;
    [SerializeField] GameObject[] specialUpgradeSet;

    public UpgradeTier upgradeTier;
    
    private void Start()
    {
        StartCoroutine(UpgradeDisplay());
    }
    IEnumerator UpgradeDisplay()
    {
        yield return new WaitForSeconds(0.2f);
        //1. 업그레이드 티어별 띄워야될 리스트업
        //2. 타입 리스트별로 결정할 업그레이드 요소 결정
        //3. 인풋필드 활성화
        //4. 엔터키 입력시 업글 진행
    }
}
