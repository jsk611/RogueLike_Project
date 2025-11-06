using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 게임 로직과 점수 시스템을 연결하는 중앙 컨트롤러
/// 몬스터 처치, 웨이브 클리어 등의 이벤트를 처리
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Score System")]
    [SerializeField] private ScoreRules scoreRules;  // 점수 계산 규칙

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;  // 현재 점수 표시

    [Header("Settings")]
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private float autoSaveInterval = 10f;  // 10초마다 자동 저장

    // 현재 게임 상태
    private string playerName;
    private int currentScore;
    private int monstersKilled;
    private int wavesCleared;
    private float gameStartTime;
    private float autoSaveTimer;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 플레이어 이름 불러오기 (MainMenu에서 설정됨)
        playerName = PlayerPrefs.GetString("PlayerName", "Player");

        // ScoreRules 자동 찾기 (할당 안 되어있으면)
        if (scoreRules == null)
        {
            scoreRules = GetComponent<ScoreRules>();
            if (scoreRules == null)
            {
                Debug.LogError("[GameController] ScoreRules를 찾을 수 없습니다!");
            }
        }

        // 게임 시작 시간 기록
        gameStartTime = Time.time;

        // 이전 세션 진행상황 복구 (선택)
        currentScore = ScoreManager.Instance.LoadSessionProgressOrZero(playerName);

        // UI 업데이트
        UpdateScoreDisplay();

        Debug.Log($"[GameController] 게임 시작 - {playerName} (복구 점수: {currentScore})");
    }

    void Update()
    {
        // 자동 저장 (일정 시간마다)
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveProgress();
            autoSaveTimer = 0f;
        }
    }

    /// <summary>
    /// 몬스터 처치 시 호출 ?
    /// 다른 스크립트에서 호출: GameController.Instance.OnMonsterKilled();
    /// </summary>
    public void OnMonsterKilled()
    {
        if (scoreRules == null)
        {
            Debug.LogWarning("[GameController] ScoreRules가 없어서 점수 계산 불가!");
            return;
        }

        // ScoreRules에서 점수 계산
        int points = scoreRules.CalculateKillScore();

        // 점수 추가
        AddScore(points);

        // 통계 업데이트
        monstersKilled++;

        Debug.Log($"[GameController] 몬스터 처치! +{points}점 (총: {currentScore}점, 처치: {monstersKilled}마리)");
    }

    /// <summary>
    /// 웨이브 클리어 시 호출 ?
    /// 다른 스크립트에서 호출: GameController.Instance.OnWaveCleared();
    /// </summary>
    public void OnWaveCleared()
    {
        if (scoreRules == null)
        {
            Debug.LogWarning("[GameController] ScoreRules가 없어서 점수 계산 불가!");
            return;
        }

        // 생존 시간 계산
        float survivalTime = Time.time - gameStartTime;

        // ScoreRules에서 보너스 점수 계산
        int timeBonus = scoreRules.CalculateTimeBonus(survivalTime);

        int totalBonus = timeBonus;

        // 점수 추가
        AddScore(totalBonus);

        // 통계 업데이트
        wavesCleared++;

        Debug.Log($"[GameController] 웨이브 클리어! +{totalBonus}점 (시간: {timeBonus}");
    }

    /// <summary>
    /// 점수 추가 (내부 메서드)
    /// </summary>
    /// 
    private void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();

    }
    private void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();

        // 즉시 저장 (중요한 이벤트)
        SaveProgress();
    }

    /// <summary>
    /// 화면에 점수 표시
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
    }

    /// <summary>
    /// 진행상황 저장 (ScoreManager 사용)
    /// </summary>
    private void SaveProgress()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveSessionProgress(playerName, currentScore);
        }
    }

    /// <summary>
    /// 게임 오버 시 호출 ?
    /// 플레이어 사망 시 호출: GameController.Instance.OnGameOver();
    /// </summary>
    public void OnGameOver()
    {
        Debug.Log($"[GameController] 게임 오버 - 최종 점수: {currentScore}");

        // 최종 점수 제출
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SubmitScore(playerName, currentScore);

            // 진행상황 삭제 (제출 완료)
            ScoreManager.Instance.ClearSessionProgress(playerName);
        }

        // 게임 오버 씬으로 이동
        SceneManager.LoadScene(gameOverSceneName);
    }

    /// <summary>
    /// 플레이어 체력 퍼센트 가져오기 (예시)
    /// 실제 Player 스크립트에 맞게 수정 필요
    /// </summary>
    private float GetPlayerHealthPercent()
    {
        // 예시: Player 스크립트에서 체력 가져오기
        // Player player = FindObjectOfType<Player>();
        // if (player != null)
        // {
        //     return player.GetHealthPercent();
        // }

        // 임시: 80% 반환
        return 0.8f;
    }

    /// <summary>
    /// 현재 점수 반환 (외부 접근용)
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// 통계 정보 반환
    /// </summary>
    public (int score, int kills, int waves) GetGameStats()
    {
        return (currentScore, monstersKilled, wavesCleared);
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveProgress();
        }
    }

    void OnApplicationQuit()
    {
        SaveProgress();
    }
}