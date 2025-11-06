using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

[Serializable]
public class PlayerScoreData
{
    public string playerName;
    public int score;
    public int rank;
}

[Serializable]
public class DailyScoreboard
{
    public string date; // YYYY-MM-DD
    public List<PlayerScoreData> players = new List<PlayerScoreData>();
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public ScoreboardUI ui; // 씬마다 있는 UI가 연결될 수 있음
    private DailyScoreboard current;

    private const string SCOREBOARD_KEY_PREFIX = "scoreboard_";
    private const string PROGRESS_KEY_PREFIX = "progress_";
    private const int MAX_PLAYERS_PER_DAY = 1000; // 메모리 보호

    // 앱 시작 시 자동 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadTodayBoard();
    }

    /// <summary>
    /// 점수 제출 (같은 이름은 최고 점수만 유지)
    /// </summary>
    public void SubmitScore(string name, int score)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("[ScoreManager] 플레이어 이름이 비어있습니다.");
            return;
        }

        if (score < 0)
        {
            Debug.LogWarning($"[ScoreManager] 음수 점수는 0으로 처리됩니다: {score}");
            score = 0;
        }

        LoadTodayBoard(); // 자정 넘어간 뒤 호출 대비

        name = name.Trim();

        // 같은 이름은 최고 점수만 유지
        var exist = current.players.FirstOrDefault(p =>
            string.Equals(p.playerName, name, StringComparison.OrdinalIgnoreCase));

        if (exist != null)
        {
            if (score > exist.score)
            {
                Debug.Log($"[ScoreManager] {name} 점수 갱신: {exist.score} → {score}");
                exist.score = score;
            }
            else
            {
                Debug.Log($"[ScoreManager] {name} 기존 점수가 더 높음: {exist.score} >= {score}");
                return; // 더 낮은 점수는 무시
            }
        }
        else
        {
            if (current.players.Count >= MAX_PLAYERS_PER_DAY)
            {
                Debug.LogWarning($"[ScoreManager] 일일 최대 플레이어 수 도달: {MAX_PLAYERS_PER_DAY}");
                return;
            }
            Debug.Log($"[ScoreManager] 새 플레이어 등록: {name} - {score}점");
            current.players.Add(new PlayerScoreData { playerName = name, score = score });
        }

        // 정렬 및 순위 부여
        current.players = current.players.OrderByDescending(p => p.score).ToList();
        for (int i = 0; i < current.players.Count; i++)
        {
            current.players[i].rank = i + 1;
        }

        Save();

        // UI 업데이트 (있으면)
        if (ui != null)
        {
            ui.UpdateUI(current, name);
        }
    }

    /// <summary>
    /// 현재 순위표 가져오기
    /// </summary>
    public DailyScoreboard GetCurrentBoard()
    {
        if (current == null) LoadTodayBoard();
        return current;
    }

    /// <summary>
    /// 특정 플레이어의 순위 가져오기 (없으면 -1)
    /// </summary>
    public int GetPlayerRank(string name)
    {
        if (current == null || string.IsNullOrWhiteSpace(name)) return -1;
        var player = current.players.FirstOrDefault(p =>
            string.Equals(p.playerName, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return player?.rank ?? -1;
    }

    // ===== 파일 경로 유틸 =====
    private string GetDir()
    {
        return Path.Combine(Application.persistentDataPath, "scoreboards");
    }

    private string GetFilePath(string date)
    {
        try
        {
            string dir = GetDir();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, $"scoreboard_{date}.json");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreManager] 디렉토리 생성 실패: {e.Message}");
            return null;
        }
    }

    // ===== 로드/세이브 =====
    private void LoadTodayBoard()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: PlayerPrefs 사용
        string key = SCOREBOARD_KEY_PREFIX + today;
        string json = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(json))
        {
            current = new DailyScoreboard { date = today };
            Debug.Log($"[ScoreManager] 새 보드 생성: {today}");
        }
        else
        {
            try
            {
                current = JsonUtility.FromJson<DailyScoreboard>(json);
                Debug.Log($"[ScoreManager] 보드 로드 성공: {today} ({current.players.Count}명)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ScoreManager] JSON 파싱 실패: {e.Message}");
                current = new DailyScoreboard { date = today };
            }
        }
#else
        // 일반 플랫폼: 파일 시스템 사용
        string path = GetFilePath(today);
        if (path == null)
        {
            current = new DailyScoreboard { date = today };
            return;
        }

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                current = JsonUtility.FromJson<DailyScoreboard>(json);
                Debug.Log($"[ScoreManager] 보드 로드 성공: {today} ({current.players.Count}명)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ScoreManager] 파일 읽기 실패: {e.Message}");
                current = new DailyScoreboard { date = today };
            }
        }
        else
        {
            current = new DailyScoreboard { date = today };
            Debug.Log($"[ScoreManager] 새 보드 생성: {today}");
        }
#endif
    }

    private void Save()
    {
        if (current == null)
        {
            Debug.LogError("[ScoreManager] 저장할 데이터가 없습니다.");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(current, true);

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: PlayerPrefs 사용
            string key = SCOREBOARD_KEY_PREFIX + current.date;
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            Debug.Log($"[ScoreManager] PlayerPrefs 저장 완료: {key}");
#else
            // 일반 플랫폼: 파일 시스템 사용
            string path = GetFilePath(current.date);
            if (path == null) return;

            File.WriteAllText(path, json);
            Debug.Log($"[ScoreManager] 파일 저장 완료: {path}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreManager] 저장 실패: {e.Message}");
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && current != null)
        {
            Save();
            Debug.Log("[ScoreManager] 일시정지 시 저장 완료");
        }
    }

    private void OnApplicationQuit()
    {
        if (current != null)
        {
            Save();
            Debug.Log("[ScoreManager] 종료 시 저장 완료");
        }
    }

    #region SessionProgress
    /// <summary>
    /// 세션별 진행 상황 (게임 중 점수 임시 저장)
    /// </summary>
    [Serializable]
    private class SessionProgress
    {
        public string date;
        public string playerName;
        public int score;
    }

    private string GetProgressKey(string name)
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        // 특수문자 제거 (안전한 키 생성)
        string safeName = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return $"{PROGRESS_KEY_PREFIX}{today}_{safeName}";
    }

    private string GetProgressFilePath(string name)
    {
        try
        {
            string dir = GetDir();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string key = GetProgressKey(name);
            return Path.Combine(dir, $"{key}.json");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreManager] 진행상황 경로 생성 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 세션 진행 상황 저장 (게임 중 임시 저장용)
    /// </summary>
    public void SaveSessionProgress(string name, int score)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("[ScoreManager] 진행상황 저장: 이름이 비어있습니다.");
            return;
        }

        var prog = new SessionProgress
        {
            date = DateTime.Now.ToString("yyyy-MM-dd"),
            playerName = name.Trim(),
            score = Mathf.Max(0, score)
        };

        try
        {
            string json = JsonUtility.ToJson(prog, true);

#if UNITY_WEBGL && !UNITY_EDITOR
            string key = GetProgressKey(name);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
#else
            string path = GetProgressFilePath(name);
            if (path != null)
            {
                File.WriteAllText(path, json);
            }
#endif
            Debug.Log($"[ScoreManager] 진행상황 저장: {name} - {score}점");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreManager] 진행상황 저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 세션 진행 상황 로드 (오늘 날짜만 유효, 아니면 0 반환)
    /// </summary>
    public int LoadSessionProgressOrZero(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return 0;

        try
        {
            string json = "";

#if UNITY_WEBGL && !UNITY_EDITOR
            string key = GetProgressKey(name);
            json = PlayerPrefs.GetString(key, "");
#else
            string path = GetProgressFilePath(name);
            if (path != null && File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
#endif

            if (string.IsNullOrEmpty(json)) return 0;

            var prog = JsonUtility.FromJson<SessionProgress>(json);

            // 날짜 바뀌면 무시 (일일 보드 기준)
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (prog.date != today)
            {
                Debug.Log($"[ScoreManager] 진행상황 만료됨: {prog.date} (오늘: {today})");
                ClearSessionProgress(name); // 오래된 데이터 삭제
                return 0;
            }

            Debug.Log($"[ScoreManager] 진행상황 로드: {name} - {prog.score}점");
            return Mathf.Max(0, prog.score);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreManager] 진행상황 로드 실패: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 세션 진행 상황 삭제
    /// </summary>
    public void ClearSessionProgress(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string key = GetProgressKey(name);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
#else
            string path = GetProgressFilePath(name);
            if (path != null && File.Exists(path))
            {
                File.Delete(path);
            }
#endif
            Debug.Log($"[ScoreManager] 진행상황 삭제: {name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreManager] 진행상황 삭제 실패: {e.Message}");
        }
    }
    #endregion
}