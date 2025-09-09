using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 시작 시 시스템 언어를 자동으로 감지하여 설정하는 컴포넌트
/// </summary>
public class LanguageAutoDetector : MonoBehaviour
{
    [Header("Auto Detection Settings")]
    [SerializeField] private bool enableAutoDetection = true; // 자동 감지 활성화
    [SerializeField] private bool onlyOnFirstRun = true; // 첫 실행에만 자동 감지
    [SerializeField] private LanguageManager.Language fallbackLanguage = LanguageManager.Language.English; // 기본 언어
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        if (enableAutoDetection)
        {
            StartCoroutine(AutoDetectLanguage());
        }
    }

    private IEnumerator AutoDetectLanguage()
    {
        // Localization 시스템이 초기화될 때까지 대기
        yield return new WaitForSeconds(0.5f);

        // 첫 실행에만 자동 감지하는 옵션이 활성화된 경우
        if (onlyOnFirstRun && PlayerPrefs.HasKey("lang"))
        {
            if (showDebugLogs)
            {
                Debug.Log("Language already set, skipping auto detection");
            }
            yield break;
        }

        // 시스템 언어 감지
        SystemLanguage systemLang = Application.systemLanguage;
        LanguageManager.Language detectedLanguage = DetectLanguageFromSystem(systemLang);

        // 감지된 언어로 설정
        LanguageManager.SetByLanguage(detectedLanguage);

        if (showDebugLogs)
        {
            Debug.Log($"Auto-detected language: {systemLang} → {detectedLanguage}");
        }
    }

    private LanguageManager.Language DetectLanguageFromSystem(SystemLanguage systemLang)
    {
        switch (systemLang)
        {
            case SystemLanguage.Korean:
                return LanguageManager.Language.Korean;
            
            case SystemLanguage.Japanese:
                return LanguageManager.Language.Japanese;
            
            case SystemLanguage.English:
            default:
                return fallbackLanguage;
        }
    }

    /// <summary>
    /// 수동으로 언어 자동 감지 실행
    /// </summary>
    [ContextMenu("Detect Language Now")]
    public void DetectLanguageNow()
    {
        SystemLanguage systemLang = Application.systemLanguage;
        LanguageManager.Language detectedLanguage = DetectLanguageFromSystem(systemLang);
        LanguageManager.SetByLanguage(detectedLanguage);
        
        Debug.Log($"Manual language detection: {systemLang} → {detectedLanguage}");
    }

    /// <summary>
    /// 언어 설정 초기화 (개발용)
    /// </summary>
    [ContextMenu("Reset Language Settings")]
    public void ResetLanguageSettings()
    {
        PlayerPrefs.DeleteKey("lang");
        LanguageManager.SetByLanguage(fallbackLanguage);
        
        Debug.Log("Language settings reset to default");
    }
}
