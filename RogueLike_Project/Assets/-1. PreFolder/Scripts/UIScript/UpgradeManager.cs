using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public GameObject[] commonButtons, rareButtons, epicButtons;
    public Canvas uiCanvas; // UI를 표시할 Canvas
    private GameObject[] curUpgradeButtons = new GameObject[3]; // 수정된 변수명

    public void UpgradeDisplay()
    {
        // 버튼 배열의 길이 확인
        if (commonButtons.Length == 0 || rareButtons.Length == 0 || epicButtons.Length == 0)
        {
            Debug.LogError("Button arrays must contain at least one button of each type.");
            return;
        }

        // 중복 방지를 위한 리스트
        List<GameObject> selectedButtons = new List<GameObject>();
        int rarepoint = 94, epicpoint = 99;

        for (int i = 0; i < 3; i++)
        {
            GameObject selectedButton = null;
            int point = Random.Range(0, 100);

            if (point >= epicpoint)
            {
                if (epicButtons.Length > 0)
                {
                    selectedButton = epicButtons[Random.Range(0, epicButtons.Length)];
                }
                epicpoint = 100; // 이후 버튼이 epic이 되지 않도록 설정
            }
            else if (point < epicpoint && point >= rarepoint)
            {
                if (rareButtons.Length > 0)
                {
                    selectedButton = rareButtons[Random.Range(0, rareButtons.Length)];
                }
                rarepoint = 100; // 이후 버튼이 rare이 되지 않도록 설정
            }
            else
            {
                if (commonButtons.Length > 0)
                {
                    selectedButton = commonButtons[Random.Range(0, commonButtons.Length)];
                }
            }

            // 중복 체크
            while (selectedButton != null && selectedButtons.Contains(selectedButton))
            {
                if (point >= epicpoint)
                {
                    if (epicButtons.Length > 0)
                    {
                        selectedButton = epicButtons[Random.Range(0, epicButtons.Length)];
                    }
                }
                else if (point < epicpoint && point >= rarepoint)
                {
                    if (rareButtons.Length > 0)
                    {
                        selectedButton = rareButtons[Random.Range(0, rareButtons.Length)];
                    }
                }
                else
                {
                    if (commonButtons.Length > 0)
                    {
                        selectedButton = commonButtons[Random.Range(0, commonButtons.Length)];
                    }
                }
            }

            curUpgradeButtons[i] = selectedButton;
            selectedButtons.Add(selectedButton); // 선택된 버튼을 리스트에 추가
        }

        // 기존 UI 요소 유지

        for (int i = 0; i < 3; i++)
        {
            if (curUpgradeButtons[i] != null)
            {
                // Instantiate할 때 Canvas의 자식으로 설정
                GameObject buttonInstance = Instantiate(curUpgradeButtons[i], uiCanvas.transform);

                // RectTransform 설정
                RectTransform rectTransform = buttonInstance.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, 50 - 75 * i);
            }
            else
            {
                Debug.LogError($"Upgrade button at index {i} is null.");
            }
        }
    }

    private void Start()
    {
        UpgradeDisplay();
    }
}
