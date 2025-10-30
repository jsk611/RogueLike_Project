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
    [SerializeField] private int showCount = 10;  // ���� N��

    private readonly List<RankEntryUI> pool = new();

    /// <summary>
    /// ���ϸ� ����� (����) �� �г����� �޾� UI ����
    /// </summary>
    public void UpdateUI(DailyScoreboard board, string selfName = null, string customTitle = null)
    {
        if (board == null) return;

        // ���
        if (titleText) titleText.text = string.IsNullOrEmpty(customTitle)
            ? "CLASSIFICA??O DO DESAFIO DI?RIO"
            : customTitle;

        if (dateText)
        {
            // ���� date�� "yyyy-MM-dd" ��� ����
            if (DateTime.TryParse(board.date, out var d))
                dateText.text = d.ToString("dd/MM/yyyy");
            else
                dateText.text = board.date;
        }

        // Ǯ Ȯ��
        EnsurePool(Mathf.Max(showCount, board.players.Count));

        // ��Ȱ��ȭ����
        foreach (var e in pool) e.gameObject.SetActive(false);

        // ���� showCount�� �Ѹ���
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

        // �� ��ũ�� ���̴� �������� ��ũ��(����Ʈ ���� ���� ����)
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
        // �� ������ ��ٷ� ���̾ƿ� �ݿ�
        yield return null;

        if (!scrollRect || !scrollRect.content) yield break;

        // 0(�� ��)~1(�� �Ʒ�) ��ֶ������ ���
        float t = (float)index / Mathf.Max(1, visibleCount - 1);
        // ��ܿ� �� �� ���̵��� ��¦ ����
        t = Mathf.Clamp01(t);
        scrollRect.normalizedPosition = new Vector2(0, 1f - t);
    }
}
