using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 설정 메뉴에 통합되는 언어 설정 패널
/// </summary>
public class LanguageSettingsPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button applyButton;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("Settings")]
    [SerializeField] private bool autoApply = true; // 드롭다운 변경 시 즉시 적용
    [SerializeField] private bool showStatusMessage = true; // 상태 메시지 표시
    [SerializeField] private float statusMessageDuration = 2f; // 상태 메시지 표시 시간

    private LanguageManager.Language selectedLanguage;
    private Coroutine statusMessageCoroutine;

    private void Start()
    {
        InitializeDropdown();
        SetupEventListeners();
        LoadCurrentLanguage();
    }

    private void InitializeDropdown()
    {
        if (languageDropdown == null)
        {
            languageDropdown = GetComponentInChildren<TMP_Dropdown>();
        }

        if (languageDropdown != null)
        {
            // 드롭다운 옵션 설정
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string>(LanguageManager.GetAvailableLanguages()));
        }
    }

    private void SetupEventListeners()
    {
        if (languageDropdown != null)
        {
            if (autoApply)
            {
                languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
            }
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(ApplyLanguageChange);
            applyButton.gameObject.SetActive(!autoApply); // 자동 적용이면 버튼 숨기기
        }
    }

    private void LoadCurrentLanguage()
    {
        LanguageManager.Language currentLanguage = LanguageManager.GetCurrentLanguage();
        selectedLanguage = currentLanguage;

        if (languageDropdown != null)
        {
            languageDropdown.SetValueWithoutNotify((int)currentLanguage);
        }

        UpdateStatusText($"Current language: {LanguageManager.GetAvailableLanguages()[(int)currentLanguage]}");
    }

    private void OnLanguageDropdownChanged(int index)
    {
        selectedLanguage = (LanguageManager.Language)index;
        
        if (autoApply)
        {
            ApplyLanguageChange();
        }
        else
        {
            UpdateStatusText("Press Apply to change language");
        }
    }

    private void ApplyLanguageChange()
    {
        LanguageManager.SetByLanguage(selectedLanguage);
        
        string languageName = LanguageManager.GetAvailableLanguages()[(int)selectedLanguage];
        UpdateStatusText($"Language changed to {languageName}");
        
        Debug.Log($"Language applied: {languageName}");
    }

    private void UpdateStatusText(string message)
    {
        if (!showStatusMessage || statusText == null) return;

        statusText.text = message;
        statusText.gameObject.SetActive(true);

        // 일정 시간 후 메시지 숨기기
        if (statusMessageCoroutine != null)
        {
            StopCoroutine(statusMessageCoroutine);
        }
        statusMessageCoroutine = StartCoroutine(HideStatusMessageAfterDelay());
    }

    private IEnumerator HideStatusMessageAfterDelay()
    {
        yield return new WaitForSeconds(statusMessageDuration);
        
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }

    // 외부에서 호출 가능한 유틸리티 메서드들
    public void RefreshLanguageOptions()
    {
        InitializeDropdown();
        LoadCurrentLanguage();
    }

    public void SetAutoApply(bool enabled)
    {
        autoApply = enabled;
        
        if (applyButton != null)
        {
            applyButton.gameObject.SetActive(!autoApply);
        }
    }

    private void OnDestroy()
    {
        if (statusMessageCoroutine != null)
        {
            StopCoroutine(statusMessageCoroutine);
        }
    }
}
