using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InfimaGames.LowPolyShooterPack;

public class UpgradeManager : MonoBehaviour
{
    static public UpgradeManager instance = new UpgradeManager();
    public GameObject upgradeUI;

    public GameObject[] commonButtons, rareButtons, epicButtons;
    public Canvas uiCanvas; // UI를 표시할 Canvas
    private GameObject[] curUpgradeButtons = new GameObject[3]; // 수정된 변수명
    [SerializeField] private GameObject[] buttonInstance = new GameObject[3]; 


    private CharacterBehaviour player;
    private bool UIenabled = false;

    public int repeatNum = 2;

    public void UpgradeDisplay()
    {
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
                // Instantiate할 때 Canvas의 자식으로 설정
                buttonInstance[i] = Instantiate(curUpgradeButtons[i], uiCanvas.transform);
                StartCoroutine(Typing(buttonInstance[i]));

                // RectTransform 설정
                RectTransform rectTransform = buttonInstance[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, 85 - 100 * i);
            }
            else
            {
                Debug.LogError($"Upgrade button at index {i} is null.");
            }
        }
    }

    public void CompleteUpgrade()
    {
        if(repeatNum > 0)
        {
            repeatNum--;
            for (int i = 0; i < 3; i++)
                Destroy(buttonInstance[i]);
            UpgradeDisplay();
        }
        else
        {
            upgradeUI.SetActive(false);
        }
    }

    private void Start()
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        UpgradeDisplay();
    }

    IEnumerator Typing(GameObject curButton)
    {
        TMP_Text tx = curButton.transform.GetChild(0).GetComponent<TMP_Text>();
        string temptx = tx.text;
        tx.text = "";

        for(int i = 0; i <= temptx.Length; i++)
        {
            tx.text = temptx.Substring(0, i);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
