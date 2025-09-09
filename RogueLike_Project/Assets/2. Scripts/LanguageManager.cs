// 드롭다운 스크립트에서 호출
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class LanguageManager
{
    public static void SetByCode(string code) // "en", "ko", "ja"...
    {
        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code.StartsWith(code));
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale; // 전역 적용
            PlayerPrefs.SetString("lang", code);          // 재실행 시 유지
        }
    }
}
