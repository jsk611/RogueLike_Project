using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    // ��Ӵٿ� �ɼ� ������ ���߼���: ENGLISH, KOREAN ...
    [SerializeField] private string[] codes = { "en", "ko" };

    private IEnumerator Start()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();

        // Localization �ý��� �ʱ�ȭ ��ٸ���
        yield return LocalizationSettings.InitializationOperation;

        // ����� ��� �ҷ��� �ʱ� ���� ����ȭ
        string saved = PlayerPrefs.GetString("lang",
            LocalizationSettings.SelectedLocale.Identifier.Code);

        int idx = IndexOfCode(saved);
        if (idx < 0) idx = 0;

        dropdown.SetValueWithoutNotify(idx);
        Apply(idx);                                  // �� ���⼭ ��ȣ�⡯
        dropdown.onValueChanged.AddListener(Apply);  // �� �ٲ� ������ ��ȣ�⡯
    }

    private int IndexOfCode(string code)
    {
        for (int i = 0; i < codes.Length; i++)
            if (code.StartsWith(codes[i])) return i; // en-US, ko-KR�� ��Ī
        return -1;
    }

    private void Apply(int index)
    {
        index = Mathf.Clamp(index, 0, codes.Length - 1);

        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code.StartsWith(codes[index]));
        if (locale == null) return;

        LocalizationSettings.SelectedLocale = locale; // ���� ��� ���� �� ��ü UI �ڵ� ����
        PlayerPrefs.SetString("lang", codes[index]);  // ���� ���� �� ����
    }
}
