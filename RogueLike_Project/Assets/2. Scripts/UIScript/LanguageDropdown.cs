using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    // 드롭다운 옵션 순서와 맞추세요: ENGLISH, KOREAN ...
    [SerializeField] private string[] codes = { "en", "ko" };

    private IEnumerator Start()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();

        // Localization 시스템 초기화 기다리기
        yield return LocalizationSettings.InitializationOperation;

        // 저장된 언어 불러와 초기 선택 동기화
        string saved = PlayerPrefs.GetString("lang",
            LocalizationSettings.SelectedLocale.Identifier.Code);

        int idx = IndexOfCode(saved);
        if (idx < 0) idx = 0;

        dropdown.SetValueWithoutNotify(idx);
        Apply(idx);                                  // ← 여기서 ‘호출’
        dropdown.onValueChanged.AddListener(Apply);  // 값 바뀔 때마다 ‘호출’
    }

    private int IndexOfCode(string code)
    {
        for (int i = 0; i < codes.Length; i++)
            if (code.StartsWith(codes[i])) return i; // en-US, ko-KR도 매칭
        return -1;
    }

    private void Apply(int index)
    {
        index = Mathf.Clamp(index, 0, codes.Length - 1);

        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code.StartsWith(codes[index]));
        if (locale == null) return;

        LocalizationSettings.SelectedLocale = locale; // 전역 언어 변경 → 전체 UI 자동 갱신
        PlayerPrefs.SetString("lang", codes[index]);  // 다음 실행 시 유지
    }
}
