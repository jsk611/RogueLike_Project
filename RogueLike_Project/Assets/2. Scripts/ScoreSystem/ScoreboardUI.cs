using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_Text titleText;  // ex) "CLASSIFICA??O DO DESAFIO DI?RIO"
    [SerializeField] private TMP_Text dateText;   // ex) "18/07/2020"

    [Header("List")]
    [SerializeField] private Transform listRoot;  // Content(Vertical Layout)
    [SerializeField] private RankEntryUI entryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int showCount = 10;  // 상위 N명

    private readonly List<RankEntryUI> pool = new();

    /// <summary>
    /// 데일리 보드와 (선택) 내 닉네임을 받아 UI 갱신
    /// </summary>
    public void UpdateUI(DailyScoreboard board, string selfName = null, string customTitle = null)
    {
        if (board == null) return;

        // 헤더
        if (titleText) titleText.text = string.IsNullOrEmpty(customTitle)
            ? "CLASSIFICA??O DO DESAFIO DI?RIO"
            : customTitle;

        if (dateText)
        {
            // 보드 date가 "yyyy-MM-dd" 라고 가정
            if (DateTime.TryParse(board.date, out var d))
                dateText.text = d.ToString("dd/MM/yyyy");
            else
                dateText.text = board.date;
        }

        // 풀 확보
        EnsurePool(Mathf.Max(showCount, board.players.Count));

        // 비활성화부터
        foreach (var e in pool) e.gameObject.SetActive(false);

        // 상위 showCount만 뿌리기
        int count = Mathf.Min(showCount, board.players.Count);
        int selfIndex = -1;

        for (int i = 0; i < count; i++)
        {
            var p = board.players[i];
            var e = pool[i];
            e.gameObject.SetActive(true);

            bool isSelf = (!string.IsNullOrEmpty(selfName) &&
                           string.Equals(p.playerName, selfName, StringComparison.OrdinalIgnoreCase));

            if (isSelf) selfIndex = i;

            bool useAltRow = (i % 2 == 1);
            e.Bind(p.rank, p.playerName, p.score, isSelf, useAltRow);
        }

        // 내 랭크가 보이는 영역으로 스크롤(리스트 내에 있을 때만)
        if (selfIndex >= 0 && scrollRect && count > 0)
            StartCoroutine(ScrollToIndex(selfIndex, count));
    }

    private void EnsurePool(int need)
    {
        while (pool.Count < need)
        {
            var inst = Instantiate(entryPrefab, listRoot);
            inst.gameObject.name = $"RankEntry_{pool.Count + 1}";
            pool.Add(inst);
        }
    }

    private System.Collections.IEnumerator ScrollToIndex(int index, int visibleCount)
    {
        // 한 프레임 기다려 레이아웃 반영
        yield return null;

        if (!scrollRect || !scrollRect.content) yield break;

        // 0(맨 위)~1(맨 아래) 노멀라이즈드 계산
        float t = (float)index / Mathf.Max(1, visibleCount - 1);
        // 상단에 좀 더 보이도록 살짝 여유
        t = Mathf.Clamp01(t);
        scrollRect.normalizedPosition = new Vector2(0, 1f - t);
    }
}
