using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 간단한 언어 선택 UI 컴포넌트
/// 한국어/일본어/영어 선택 기능
/// </summary>
public class SimpleLanguageSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button koreanButton;
    [SerializeField] private Button japaneseButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private TextMeshProUGUI currentLanguageText;
    
    [Header("Button Texts")]
    [SerializeField] private string koreanButtonText = "한국어";
    [SerializeField] private string japaneseButtonText = "日本語";
    [SerializeField] private string englishButtonText = "English";

    private void Start()
    {
        SetupButtons();
        UpdateCurrentLanguageDisplay();
    }

    private void SetupButtons()
    {
        // 버튼들이 할당되지 않았으면 자동으로 찾기
        if (koreanButton == null || japaneseButton == null || englishButton == null)
        {
            FindButtonsAutomatically();
        }

        // 버튼 이벤트 연결
        if (koreanButton != null)
        {
            koreanButton.onClick.AddListener(() => ChangeLanguage(LanguageManager.Language.Korean));
            SetButtonText(koreanButton, koreanButtonText);
        }

        if (japaneseButton != null)
        {
            japaneseButton.onClick.AddListener(() => ChangeLanguage(LanguageManager.Language.Japanese));
            SetButtonText(japaneseButton, japaneseButtonText);
        }

        if (englishButton != null)
        {
            englishButton.onClick.AddListener(() => ChangeLanguage(LanguageManager.Language.English));
            SetButtonText(englishButton, englishButtonText);
        }
    }

    private void FindButtonsAutomatically()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        
        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLower();
            
            if (buttonName.Contains("korean") || buttonName.Contains("한국"))
            {
                koreanButton = button;
            }
            else if (buttonName.Contains("japanese") || buttonName.Contains("일본") || buttonName.Contains("japan"))
            {
                japaneseButton = button;
            }
            else if (buttonName.Contains("english") || buttonName.Contains("영어"))
            {
                englishButton = button;
            }
        }
    }

    private void SetButtonText(Button button, string text)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    private void ChangeLanguage(LanguageManager.Language language)
    {
        LanguageManager.SetByLanguage(language);
        UpdateCurrentLanguageDisplay();
        UpdateButtonStates();
    }

    private void UpdateCurrentLanguageDisplay()
    {
        if (currentLanguageText != null)
        {
            LanguageManager.Language current = LanguageManager.GetCurrentLanguage();
            string[] languageNames = LanguageManager.GetAvailableLanguages();
            currentLanguageText.text = $"Current: {languageNames[(int)current]}";
        }
    }

    private void UpdateButtonStates()
    {
        LanguageManager.Language current = LanguageManager.GetCurrentLanguage();
        
        // 현재 선택된 언어 버튼을 다른 색상으로 표시
        SetButtonSelected(koreanButton, current == LanguageManager.Language.Korean);
        SetButtonSelected(japaneseButton, current == LanguageManager.Language.Japanese);
        SetButtonSelected(englishButton, current == LanguageManager.Language.English);
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        if (button == null) return;

        ColorBlock colors = button.colors;
        if (selected)
        {
            colors.normalColor = Color.green;
            colors.selectedColor = Color.green;
        }
        else
        {
            colors.normalColor = Color.white;
            colors.selectedColor = Color.gray;
        }
        button.colors = colors;
    }

    // 외부에서 호출 가능한 메서드들
    public void SetKorean()
    {
        ChangeLanguage(LanguageManager.Language.Korean);
    }

    public void SetJapanese()
    {
        ChangeLanguage(LanguageManager.Language.Japanese);
    }

    public void SetEnglish()
    {
        ChangeLanguage(LanguageManager.Language.English);
    }
}
