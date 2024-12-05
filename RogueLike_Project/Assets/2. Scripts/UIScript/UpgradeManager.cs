using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InfimaGames.LowPolyShooterPack;

public class UpgradeManager : MonoBehaviour
{
    //public static UpgradeManager instance;

    [SerializeField] GameObject upgradeUI;

    [SerializeField] GameObject[] commonButtons, rareButtons, epicButtons;
    private GameObject[] curUpgradeButtons = new GameObject[3]; // 수정된 변수명

    private CharacterBehaviour player;
    private StatusBehaviour status;

    public bool UIenabled = false;

    private int repeatNum = 0;

    public enum Upgrade
    {
        Damage,
        AttackSpeed,
        ReloadSpeed,
        CriticalRate,
        CriticalDamage,
        MoveSpeed,
        Heath
    }

    private void Awake()
    {
       // instance = this;
    }

    private void Start()
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = player.GetComponent<StatusBehaviour>();
    }

    public void RepeatNumSet(int n)
    {
        repeatNum = n;
    }

    public void UpgradeDisplay()
    {
        upgradeUI.SetActive(true);

        // 중복 방지를 위한 리스트
        List<GameObject> selectedButtons = new List<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            GameObject selectedButton;
            selectedButton = commonButtons[Random.Range(0, commonButtons.Length)];

            // 중복 체크
            while (selectedButton != null && selectedButtons.Contains(selectedButton))
            {
                if (commonButtons.Length > 0)
                {
                    selectedButton = commonButtons[Random.Range(0, commonButtons.Length)];
                }
            }
            curUpgradeButtons[i] = selectedButton;
            selectedButtons.Add(selectedButton); // 선택된 버튼을 리스트에 추가
        }
        if(!UIenabled)
        {
            UIenabled = !UIenabled;
            player.SetCursorState(false);
        }
        // 기존 UI 요소 유지
        
        for (int i = 0; i < 3; i++)
        {
            if (curUpgradeButtons[i] != null)
            {
                curUpgradeButtons[i].SetActive(true);
                StartCoroutine(Typing(curUpgradeButtons[i]));
            }
            else
            {
                Debug.LogError($"Upgrade button at index {i} is null.");
            }
        }
    }

    public void CompleteUpgrade(Upgrade type,float degree)
    {
        switch(type)
        {
            case Upgrade.AttackSpeed:
                status.IncreaseAttackSpeed(degree);
                break;
            case Upgrade.ReloadSpeed:
                status.IncreaseReloadSpeed(degree);
                break;
            case Upgrade.MoveSpeed:
                status.IncreaseMovementSpeed(degree);
                break;
            case Upgrade.CriticalRate:
                status.IncreaseCriticalRate(degree);
                break;
            case Upgrade.CriticalDamage:
                status.IncreaseCriticalDamage(degree);
                break;
            case Upgrade.Damage:
                status.IncreaseAttackDamage(degree);
                break;
            case Upgrade.Heath:
                status.IncreaseMaxHealth(degree);
                status.IncreaseHealth(degree);
                break;
        }
        for(int i = 0; i < 3; i++)
        {
            curUpgradeButtons[i].SetActive(false);
        }
        upgradeUI.SetActive(false);

        if (repeatNum > 0)
        {
            StopAllCoroutines();
            repeatNum--;
            UpgradeDisplay();

        }
        else
        {
            UIenabled = !UIenabled;
            player.SetCursorState(true);
        }

    }

    IEnumerator Typing(GameObject curButton)
    {
        TMP_Text tx = curButton.transform.GetChild(1).GetComponent<TMP_Text>();
        string temptx = curButton.GetComponent<CompleteUpgrade>().baseText;
        tx.text = "";

        for(int i = 0; i <= temptx.Length; i++)
        {
            tx.text = temptx.Substring(0, i);
            yield return new WaitForSeconds(0.06f);
        }
    }
}
