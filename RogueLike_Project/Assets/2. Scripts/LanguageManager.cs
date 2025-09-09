// ��Ӵٿ� ��ũ��Ʈ���� ȣ��
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class LanguageManager
{
    // 지원 언어 목록
    public enum Language
    {
        English,
        Korean, 
        Japanese
    }
    
    // 언어 코드 매핑
    private static readonly string[] languageCodes = { "en", "ko", "ja" };
    private static readonly string[] languageNames = { "English", "한국어", "日本語" };
    
    public static void SetByCode(string code) // "en", "ko", "ja"...
    {
        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code.StartsWith(code));
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale; // 언어 설정
            PlayerPrefs.SetString("lang", code);          // 저장해 둘 설정
            Debug.Log($"Language changed to: {GetLanguageName(code)}");
        }
        else
        {
            Debug.LogWarning($"Language code '{code}' not found!");
        }
    }
    
    public static void SetByLanguage(Language language)
    {
        int index = (int)language;
        if (index >= 0 && index < languageCodes.Length)
        {
            SetByCode(languageCodes[index]);
        }
    }
    
    public static Language GetCurrentLanguage()
    {
        string currentCode = PlayerPrefs.GetString("lang", "en");
        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (currentCode.StartsWith(languageCodes[i]))
            {
                return (Language)i;
            }
        }
        return Language.English; // 기본값
    }
    
    public static string GetCurrentLanguageCode()
    {
        return PlayerPrefs.GetString("lang", "en");
    }
    
    public static string GetLanguageName(string code)
    {
        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (code.StartsWith(languageCodes[i]))
            {
                return languageNames[i];
            }
        }
        return "Unknown";
    }
    
    public static string[] GetAvailableLanguages()
    {
        return languageNames;
    }
}
