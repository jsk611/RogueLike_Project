using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ScrollView에 순위 데이터를 표시하는 간단한 스크립트
/// </summary>
public class LeaderboardDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform content;           // ScrollView의 Content
    [SerializeField] private GameObject rankEntryPrefab;  // RankEntry 프리팹

    void Start()
    {
        // 테스트 데이터로 순위표 표시
        ShowTestData();
    }

    /// <summary>
    /// 테스트용 하드코딩 데이터
    /// </summary>
    void ShowTestData()
    {
        // 테스트 데이터 (나중에 ScoreManager로 교체)
        var testData = new List<(int rank, string name, int score)>
        {
            (1, "TheEctoplasm", 701),
            (2, "Magnos", 655),
            (3, "祝卡宇航中考顺利", 609),
            (4, "莱虚昆-鸡你太美", 605),
            (5, "羊驼斯坦森", 535),
            (6, "nomoretitanic", 530),
            (7, "Arthur Pendragon", 520),
            (8, "鱼人永不败", 501),
            (9, "PrinceCosmo", 493),
            (10, "[XX] Inquisitor", 484)
        };

        // 각 데이터를 RankEntry로 생성
        foreach (var data in testData)
        {
            CreateRankEntry(data.rank, data.name, data.score);
        }
    }

    /// <summary>
    /// RankEntry 프리팹을 복사해서 Content에 추가
    /// </summary>
    void CreateRankEntry(int rank, string playerName, int score)
    {
        // 1. 프리팹 복사 (Instantiate)
        GameObject entry = Instantiate(rankEntryPrefab, content);

        // 2. 자식 Text 찾기
        Transform rankText = entry.transform.Find("RankText");
        Transform nameText = entry.transform.Find("NameText");
        Transform scoreText = entry.transform.Find("ScoreText");

        // 3. 텍스트 내용 변경
        if (rankText != null)
        {
            Text tmp = rankText.GetComponent<Text>();
            tmp.text = $"#{rank}";
        }

        if (nameText != null)
        {
            Text tmp = nameText.GetComponent<Text>();
            tmp.text = playerName;
        }

        if (scoreText != null)
        {
            Text tmp = scoreText.GetComponent<Text>();
            tmp.text = score.ToString("N0"); // 천단위 콤마
        }
    }
}