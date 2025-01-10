using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InfimaGames.LowPolyShooterPack;
using Unity.VisualScripting;

public class UpgradeManager : MonoBehaviour
{
    //public static UpgradeManager instance;

    [SerializeField] GameObject upgradeUI;

    [SerializeField] GameObject[] commonButtons, rareButtons, epicButtons;
    private GameObject[] curUpgradeButtons = new GameObject[3]; // 수정된 변수명

    private CharacterBehaviour player;
    private PlayerStatus status;

    public bool UIenabled = false;

    private int repeatNum = 0;

    public enum CommonUpgrade
    {
        Default,
        Damage,
        AttackSpeed,
        ReloadSpeed,
        CriticalRate,
        CriticalDamage,
        MoveSpeed,
        Heath,
        CoinAcquisitonRate,
        PermanentCoinAcquisitionRate
    }
    public enum RareUpgrade
    {
        Default,
        ApplyBlaze,
        ApplyFreeze,
        ApplyPoisonous,
        ApplyShock,
    }
    public enum EpicUpgrade
    {
        Default,
        Berserk,
        Cavity_System_Model,
        
        K_Ampule_Activation,

    }

    private void Awake()
    {
       // instance = this;
    }

    private void Start()
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = player.GetComponent<PlayerStatus>();
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
            selectedButton = rareButtons[Random.Range(0, rareButtons.Length)];

            // 중복 체크
            while (selectedButton != null && selectedButtons.Contains(selectedButton))
            {
                if (rareButtons.Length > 0)
                {
                    selectedButton = rareButtons[Random.Range(0, rareButtons.Length)];
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

    public void CompleteCommonUpgrade(CommonUpgrade type,float degree)
    {
        switch(type)
        {
            case CommonUpgrade.AttackSpeed:
                status.IncreaseAttackSpeed(degree);
                break;
            case CommonUpgrade.ReloadSpeed:
                status.IncreaseReloadSpeed(degree);
                break;
            case CommonUpgrade.MoveSpeed:
                status.IncreaseMovementSpeed(degree);
                break;
            case CommonUpgrade.CriticalRate:
                status.IncreaseCriticalRate(degree);
                break;
            case CommonUpgrade.CriticalDamage:
                status.IncreaseCriticalDamage(degree);
                break;
            case CommonUpgrade.Damage:
                status.IncreaseAttackDamage(degree);
                break;
            case CommonUpgrade.Heath:
                status.IncreaseMaxHealth(degree);
                status.IncreaseHealth(degree);
                break;
            case CommonUpgrade.CoinAcquisitonRate:
                status.IncreaseAcquisitionRate(degree);
                break;
            case CommonUpgrade.PermanentCoinAcquisitionRate:
                status.IncreasePermanentAcquisitionRate(degree);
                break;
            default:
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
    public void CompleteRareUpgrade(RareUpgrade type, float degree)
    {
        switch (type)
        {
            case RareUpgrade.ApplyBlaze:
           //     player.GetInventory().GetEquipped(). AddComponent<Blaze>();   탄환에 적용해야 함
                break;
            case RareUpgrade.ApplyFreeze:
               
                break;
            case RareUpgrade.ApplyPoisonous:

                break;
            case RareUpgrade.ApplyShock:
    
            default:
                break;
        }
        for (int i = 0; i < 3; i++)
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
    public void CompleteEpicUpgrade(EpicUpgrade type, float degree)
    {

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
