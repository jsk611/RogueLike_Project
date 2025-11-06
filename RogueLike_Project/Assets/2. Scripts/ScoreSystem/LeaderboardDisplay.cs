using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ScrollView에 순위 데이터를 표시하는 스크립트
/// ScoreManager에서 데이터를 가져옴
/// </summary>
public class LeaderboardDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform content;           // ScrollView의 Content
    [SerializeField] private GameObject rankEntryPrefab;  // RankEntry 프리팹
    
    [Header("Settings")]
    [SerializeField] private bool useTestData = false;     // true = 테스트 데이터, false = ScoreManager
    [SerializeField] private int maxDisplayCount = 10;    // 최대 표시 개수
    
    void Start()
    {
        RefreshLeaderboard();
    }
    
    /// <summary>
    /// 순위표 새로고침
    /// </summary>
    public void RefreshLeaderboard()
    {
        // 기존 항목 삭제
        ClearLeaderboard();
        
        if (useTestData)
        {
            // 테스트 데이터 사용
            ShowTestData();
        }
        else
        {
            // ScoreManager에서 실제 데이터 가져오기
            ShowRealData();
        }
    }
    
    /// <summary>
    /// ScoreManager에서 실제 데이터 가져오기 ⭐
    /// </summary>
    void ShowRealData()
    {
        // ScoreManager가 없으면 경고
        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning("[LeaderboardDisplay] ScoreManager가 없습니다!");
            return;
        }
        
        // 현재 순위표 데이터 가져오기
        DailyScoreboard board = ScoreManager.Instance.GetCurrentBoard();
        
        // 데이터 확인
        if (board == null || board.players == null || board.players.Count == 0)
        {
            Debug.Log("[LeaderboardDisplay] 순위 데이터가 없습니다.");
            ShowEmptyMessage();
            return;
        }
        
        // 표시할 개수 결정
        int displayCount = Mathf.Min(board.players.Count, maxDisplayCount);
        
        Debug.Log($"[LeaderboardDisplay] 순위표 표시: {displayCount}명");
        
        // 순위표 생성
        for (int i = 0; i < displayCount; i++)
        {
            PlayerScoreData player = board.players[i];
            CreateRankEntry(player.rank, player.playerName, player.score);
        }
    }
    
    /// <summary>
    /// 테스트용 하드코딩 데이터
    /// </summary>
    void ShowTestData()
    {
        Debug.Log("[LeaderboardDisplay] 테스트 데이터 표시");
        
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
        // 프리팹 복사
        GameObject entry = Instantiate(rankEntryPrefab, content);
        
        // 자식 Text 찾기
        Transform rankText = entry.transform.Find("RankText");
        Transform nameText = entry.transform.Find("NameText");
        Transform scoreText = entry.transform.Find("ScoreText");
        
        // 텍스트 내용 변경
        if (rankText != null)
        {
            TextMeshProUGUI tmp = rankText.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = $"#{rank}";
            }
        }
        else
        {
            Debug.LogWarning($"[LeaderboardDisplay] RankText를 찾을 수 없습니다. 프리팹 확인 필요!");
        }
        
        if (nameText != null)
        {
            TextMeshProUGUI tmp = nameText.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = playerName;
            }
        }
        else
        {
            Debug.LogWarning($"[LeaderboardDisplay] NameText를 찾을 수 없습니다. 프리팹 확인 필요!");
        }
        
        if (scoreText != null)
        {
            TextMeshProUGUI tmp = scoreText.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = score.ToString("N0"); // 천단위 콤마
            }
        }
        else
        {
            Debug.LogWarning($"[LeaderboardDisplay] ScoreText를 찾을 수 없습니다. 프리팹 확인 필요!");
        }
    }
    
    /// <summary>
    /// 기존 순위표 삭제
    /// </summary>
    void ClearLeaderboard()
    {
        if (content == null) return;
        
        // Content의 모든 자식 삭제
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// 빈 순위표 메시지 표시 (선택)
    /// </summary>
    void ShowEmptyMessage()
    {
        // 간단한 메시지 생성 (선택 사항)
        GameObject entry = Instantiate(rankEntryPrefab, content);
        
        Transform nameText = entry.transform.Find("NameText");
        if (nameText != null)
        {
            TextMeshProUGUI tmp = nameText.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "아직 기록이 없습니다";
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }
    }
}