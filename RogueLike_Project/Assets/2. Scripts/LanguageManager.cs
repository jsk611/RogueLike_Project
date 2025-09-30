// ��Ӵٿ� ��ũ��Ʈ���� ȣ��
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
            LocalizationSettings.SelectedLocale = locale; // ���� ����
            PlayerPrefs.SetString("lang", code);          // ����� �� ����
        }
    }
}
