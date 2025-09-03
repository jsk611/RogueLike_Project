using UnityEngine;

/// <summary>
/// 적극성 시스템들을 초기화하고 관리하는 매니저 클래스
/// 게임 시작 시 필요한 시스템들을 자동으로 생성하고 설정합니다.
/// </summary>
public class AggressionSystemManager : MonoBehaviour
{
    [Header("System Settings")]
    [SerializeField] private bool autoInitialize = true; // 자동 초기화
    [SerializeField] private bool enableDebugUI = true; // 디버그 UI 활성화
    
    [Header("System References")]
    [SerializeField] private GameObject playerBehaviorAnalyzerPrefab;
    [SerializeField] private GameObject dynamicDifficultyAdjusterPrefab;
    
    // 시스템 인스턴스들
    private PlayerBehaviorAnalyzer behaviorAnalyzer;
    private DynamicDifficultyAdjuster difficultyAdjuster;
    
    // 싱글톤 패턴
    public static AggressionSystemManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (autoInitialize)
            {
                InitializeSystems();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 모든 적극성 시스템 초기화
    /// </summary>
    public void InitializeSystems()
    {
        Debug.Log("Initializing Aggression Systems...");
        
        // PlayerBehaviorAnalyzer 초기화
        InitializePlayerBehaviorAnalyzer();
        
        // DynamicDifficultyAdjuster 초기화
        InitializeDynamicDifficultyAdjuster();
        
        // 기존 몬스터들에 적극성 시스템 추가
        AddAggressionSystemToExistingMonsters();
        
        Debug.Log("Aggression Systems initialized successfully!");
    }

    /// <summary>
    /// PlayerBehaviorAnalyzer 초기화
    /// </summary>
    private void InitializePlayerBehaviorAnalyzer()
    {
        // 이미 존재하는지 확인
        behaviorAnalyzer = FindObjectOfType<PlayerBehaviorAnalyzer>();
        
        if (behaviorAnalyzer == null)
        {
            if (playerBehaviorAnalyzerPrefab != null)
            {
                GameObject analyzerObject = Instantiate(playerBehaviorAnalyzerPrefab);
                behaviorAnalyzer = analyzerObject.GetComponent<PlayerBehaviorAnalyzer>();
            }
            else
            {
                // 동적으로 생성
                GameObject analyzerObject = new GameObject("PlayerBehaviorAnalyzer");
                behaviorAnalyzer = analyzerObject.AddComponent<PlayerBehaviorAnalyzer>();
                DontDestroyOnLoad(analyzerObject);
            }
            
            Debug.Log("PlayerBehaviorAnalyzer created");
        }
        else
        {
            Debug.Log("PlayerBehaviorAnalyzer already exists");
        }
    }

    /// <summary>
    /// DynamicDifficultyAdjuster 초기화
    /// </summary>
    private void InitializeDynamicDifficultyAdjuster()
    {
        // 이미 존재하는지 확인
        difficultyAdjuster = FindObjectOfType<DynamicDifficultyAdjuster>();
        
        if (difficultyAdjuster == null)
        {
            if (dynamicDifficultyAdjusterPrefab != null)
            {
                GameObject adjusterObject = Instantiate(dynamicDifficultyAdjusterPrefab);
                difficultyAdjuster = adjusterObject.GetComponent<DynamicDifficultyAdjuster>();
            }
            else
            {
                // 동적으로 생성
                GameObject adjusterObject = new GameObject("DynamicDifficultyAdjuster");
                difficultyAdjuster = adjusterObject.AddComponent<DynamicDifficultyAdjuster>();
                DontDestroyOnLoad(adjusterObject);
            }
            
            Debug.Log("DynamicDifficultyAdjuster created");
        }
        else
        {
            Debug.Log("DynamicDifficultyAdjuster already exists");
        }
    }

    /// <summary>
    /// 기존 몬스터들에 적극성 시스템 추가
    /// </summary>
    private void AddAggressionSystemToExistingMonsters()
    {
        MonsterBase[] existingMonsters = FindObjectsOfType<MonsterBase>();
        int addedCount = 0;
        
        foreach (MonsterBase monster in existingMonsters)
        {
            // AdaptiveAggressionSystem이 없는 몬스터에 추가
            if (monster.GetComponent<AdaptiveAggressionSystem>() == null)
            {
                monster.gameObject.AddComponent<AdaptiveAggressionSystem>();
                addedCount++;
            }
            
            // EncirclementAI가 없는 몬스터에 추가
            if (monster.GetComponent<EncirclementAI>() == null)
            {
                monster.gameObject.AddComponent<EncirclementAI>();
            }
        }
        
        Debug.Log($"Added AdaptiveAggressionSystem to {addedCount} existing monsters");
    }

    /// <summary>
    /// 새로운 몬스터에 적극성 시스템 추가
    /// </summary>
    public void AddAggressionSystemToMonster(MonsterBase monster)
    {
        if (monster == null) return;
        
        // AdaptiveAggressionSystem 추가
        if (monster.GetComponent<AdaptiveAggressionSystem>() == null)
        {
            monster.gameObject.AddComponent<AdaptiveAggressionSystem>();
        }
        
        // EncirclementAI 추가
        if (monster.GetComponent<EncirclementAI>() == null)
        {
            monster.gameObject.AddComponent<EncirclementAI>();
        }
        
        Debug.Log($"Added aggression systems to {monster.name}");
    }

    /// <summary>
    /// 모든 적극성 시스템 활성화/비활성화
    /// </summary>
    public void SetSystemsEnabled(bool enabled)
    {
        // 모든 AdaptiveAggressionSystem 찾기
        AdaptiveAggressionSystem[] aggressionSystems = FindObjectsOfType<AdaptiveAggressionSystem>();
        foreach (var system in aggressionSystems)
        {
            system.SetAggressionEnabled(enabled);
        }
        
        // PlayerBehaviorAnalyzer 활성화/비활성화
        if (behaviorAnalyzer != null)
        {
            behaviorAnalyzer.enabled = enabled;
        }
        
        // DynamicDifficultyAdjuster 활성화/비활성화
        if (difficultyAdjuster != null)
        {
            difficultyAdjuster.enabled = enabled;
        }
        
        Debug.Log($"Aggression systems {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// 모든 시스템을 기본 설정으로 리셋
    /// </summary>
    public void ResetAllSystems()
    {
        // 모든 AdaptiveAggressionSystem 리셋
        AdaptiveAggressionSystem[] aggressionSystems = FindObjectsOfType<AdaptiveAggressionSystem>();
        foreach (var system in aggressionSystems)
        {
            system.ResetToDefaults();
        }
        
        Debug.Log("All aggression systems reset to defaults");
    }

    /// <summary>
    /// 시스템 상태 정보 반환
    /// </summary>
    public string GetSystemStatus()
    {
        int totalMonsters = FindObjectsOfType<MonsterBase>().Length;
        int aggressionSystemCount = FindObjectsOfType<AdaptiveAggressionSystem>().Length;
        int encirclementAICount = FindObjectsOfType<EncirclementAI>().Length;
        
        bool behaviorAnalyzerActive = behaviorAnalyzer != null && behaviorAnalyzer.enabled;
        bool difficultyAdjusterActive = difficultyAdjuster != null && difficultyAdjuster.enabled;
        
        return $"Total Monsters: {totalMonsters}\n" +
               $"Aggression Systems: {aggressionSystemCount}\n" +
               $"Encirclement AIs: {encirclementAICount}\n" +
               $"Behavior Analyzer: {(behaviorAnalyzerActive ? "Active" : "Inactive")}\n" +
               $"Difficulty Adjuster: {(difficultyAdjusterActive ? "Active" : "Inactive")}";
    }

    /// <summary>
    /// 플레이어 행동 분석기 참조 반환
    /// </summary>
    public PlayerBehaviorAnalyzer GetBehaviorAnalyzer()
    {
        return behaviorAnalyzer;
    }

    /// <summary>
    /// 동적 난이도 조절기 참조 반환
    /// </summary>
    public DynamicDifficultyAdjuster GetDifficultyAdjuster()
    {
        return difficultyAdjuster;
    }

    // 디버그용 GUI
    private void OnGUI()
    {
        if (!enableDebugUI || !Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 300));
        GUILayout.Label("Aggression System Manager", GUI.skin.box);
        
        // 시스템 상태 표시
        GUILayout.Label(GetSystemStatus());
        
        GUILayout.Space(10);
        
        // 제어 버튼들
        if (GUILayout.Button("Reset All Systems"))
        {
            ResetAllSystems();
        }
        
        if (GUILayout.Button("Reinitialize Systems"))
        {
            InitializeSystems();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Enable Systems"))
        {
            SetSystemsEnabled(true);
        }
        if (GUILayout.Button("Disable Systems"))
        {
            SetSystemsEnabled(false);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 현재 난이도 정보 표시
        if (difficultyAdjuster != null)
        {
            var adjustment = difficultyAdjuster.GetCurrentAdjustment();
            GUILayout.Label($"Current Difficulty: {adjustment.overallDifficulty:F2}");
            GUILayout.Label($"Aggression Mult: {adjustment.aggressionMultiplier:F2}");
            GUILayout.Label($"Speed Mult: {adjustment.speedMultiplier:F2}");
        }
        
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
