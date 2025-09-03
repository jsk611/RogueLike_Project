using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 성능 최적화된 적극성 시스템 매니저
/// 다수의 몬스터가 있을 때 전체적인 성능을 관리
/// </summary>
public class PerformanceOptimizedManager : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private int maxHighDetailMonsters = 5; // 고품질 업데이트 최대 몬스터 수
    [SerializeField] private int maxMediumDetailMonsters = 10; // 중품질 업데이트 최대 몬스터 수
    [SerializeField] private float performanceCheckInterval = 2f; // 성능 체크 간격
    [SerializeField] private int targetFPS = 60; // 목표 FPS
    [SerializeField] private bool enableAdaptiveOptimization = true; // 적응형 최적화
    
    [Header("Debug Settings")]
    [SerializeField] private bool showPerformanceGUI = true;
    [SerializeField] private bool enablePerformanceLogs = false;
    
    // 성능 모니터링
    private float lastPerformanceCheck;
    private List<float> recentFrameTimes = new List<float>();
    private int frameCount = 0;
    private float averageFPS = 60f;
    
    // 시스템 상태
    private Dictionary<OptimizedAggressionSystem, float> systemDistances = new Dictionary<OptimizedAggressionSystem, float>();
    private Transform playerTransform;
    
    // 싱글톤
    public static PerformanceOptimizedManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! PerformanceOptimizedManager requires a Player.");
            enabled = false;
        }
    }

    private void Update()
    {
        MonitorPerformance();
        
        if (Time.time - lastPerformanceCheck >= performanceCheckInterval)
        {
            OptimizeSystemPerformance();
            lastPerformanceCheck = Time.time;
        }
    }

    private void MonitorPerformance()
    {
        frameCount++;
        recentFrameTimes.Add(Time.unscaledDeltaTime);
        
        // 최근 30프레임만 유지
        if (recentFrameTimes.Count > 30)
        {
            recentFrameTimes.RemoveAt(0);
        }
        
        // 평균 FPS 계산
        if (recentFrameTimes.Count > 0)
        {
            float averageFrameTime = recentFrameTimes.Average();
            averageFPS = 1f / averageFrameTime;
        }
    }

    private void OptimizeSystemPerformance()
    {
        var allSystems = FindObjectsOfType<OptimizedAggressionSystem>();
        if (allSystems.Length == 0) return;
        
        // 플레이어와의 거리 계산 및 정렬
        UpdateSystemDistances(allSystems);
        
        // 성능 기반 최적화
        if (enableAdaptiveOptimization)
        {
            AdaptiveOptimization(allSystems);
        }
        else
        {
            StaticOptimization(allSystems);
        }
        
        if (enablePerformanceLogs)
        {
            LogPerformanceStats(allSystems);
        }
    }

    private void UpdateSystemDistances(OptimizedAggressionSystem[] systems)
    {
        systemDistances.Clear();
        
        foreach (var system in systems)
        {
            if (system == null || playerTransform == null) continue;
            
            float distance = Vector3.Distance(system.transform.position, playerTransform.position);
            systemDistances[system] = distance;
        }
    }

    private void AdaptiveOptimization(OptimizedAggressionSystem[] systems)
    {
        // FPS 기반 동적 최적화
        float performanceRatio = averageFPS / targetFPS;
        
        int dynamicHighDetail = Mathf.RoundToInt(maxHighDetailMonsters * performanceRatio);
        int dynamicMediumDetail = Mathf.RoundToInt(maxMediumDetailMonsters * performanceRatio);
        
        // 최소값 보장
        dynamicHighDetail = Mathf.Max(1, dynamicHighDetail);
        dynamicMediumDetail = Mathf.Max(2, dynamicMediumDetail);
        
        // 성능이 좋으면 더 많은 몬스터를 고품질로
        if (averageFPS > targetFPS * 1.2f)
        {
            dynamicHighDetail = Mathf.Min(systems.Length, maxHighDetailMonsters * 2);
            dynamicMediumDetail = Mathf.Min(systems.Length, maxMediumDetailMonsters * 2);
        }
        // 성능이 나쁘면 품질 감소
        else if (averageFPS < targetFPS * 0.8f)
        {
            dynamicHighDetail = Mathf.Max(1, maxHighDetailMonsters / 2);
            dynamicMediumDetail = Mathf.Max(2, maxMediumDetailMonsters / 2);
        }
        
        ApplyLODDistribution(systems, dynamicHighDetail, dynamicMediumDetail);
    }

    private void StaticOptimization(OptimizedAggressionSystem[] systems)
    {
        ApplyLODDistribution(systems, maxHighDetailMonsters, maxMediumDetailMonsters);
    }

    private void ApplyLODDistribution(OptimizedAggressionSystem[] systems, int highDetailCount, int mediumDetailCount)
    {
        // 거리순으로 정렬
        var sortedSystems = systemDistances.OrderBy(kvp => kvp.Value).ToList();
        
        int assignedHigh = 0;
        int assignedMedium = 0;
        
        foreach (var kvp in sortedSystems)
        {
            var system = kvp.Key;
            var distance = kvp.Value;
            
            if (system == null) continue;
            
            OptimizedAggressionSystem.UpdateLevel targetLevel;
            
            // 거리와 성능에 기반한 LOD 할당
            if (assignedHigh < highDetailCount && distance <= 15f)
            {
                targetLevel = OptimizedAggressionSystem.UpdateLevel.High;
                assignedHigh++;
            }
            else if (assignedMedium < mediumDetailCount && distance <= 30f)
            {
                targetLevel = OptimizedAggressionSystem.UpdateLevel.Medium;
                assignedMedium++;
            }
            else if (distance <= 50f)
            {
                targetLevel = OptimizedAggressionSystem.UpdateLevel.Low;
            }
            else
            {
                targetLevel = OptimizedAggressionSystem.UpdateLevel.Disabled;
            }
            
            // LOD 레벨 강제 설정
            SetSystemLOD(system, targetLevel);
        }
    }

    private void SetSystemLOD(OptimizedAggressionSystem system, OptimizedAggressionSystem.UpdateLevel level)
    {
        // 리플렉션을 사용하여 private 필드에 접근
        var levelField = typeof(OptimizedAggressionSystem).GetField("currentUpdateLevel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (levelField != null)
        {
            levelField.SetValue(system, level);
        }
    }

    private void LogPerformanceStats(OptimizedAggressionSystem[] systems)
    {
        var stats = OptimizedAggressionSystem.GetSystemStats();
        Debug.Log($"Performance Stats - FPS: {averageFPS:F1}, " +
                  $"Systems: {stats.total}, High: {stats.high}, Medium: {stats.medium}, " +
                  $"Low: {stats.low}, Disabled: {stats.disabled}");
    }

    /// <summary>
    /// 전체 시스템 성능 레벨 강제 설정
    /// </summary>
    public void ForceGlobalPerformanceLevel(OptimizedAggressionSystem.UpdateLevel level)
    {
        OptimizedAggressionSystem.SetGlobalUpdateLevel(level);
        Debug.Log($"Forced all systems to {level} performance level");
    }

    /// <summary>
    /// 성능 기반 자동 최적화 토글
    /// </summary>
    public void ToggleAdaptiveOptimization()
    {
        enableAdaptiveOptimization = !enableAdaptiveOptimization;
        Debug.Log($"Adaptive optimization: {(enableAdaptiveOptimization ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// 현재 성능 상태 반환
    /// </summary>
    public (float fps, int totalSystems, string status) GetPerformanceStatus()
    {
        var stats = OptimizedAggressionSystem.GetSystemStats();
        string status = averageFPS >= targetFPS ? "Good" : averageFPS >= targetFPS * 0.8f ? "Fair" : "Poor";
        return (averageFPS, stats.total, status);
    }

    /// <summary>
    /// 메모리 정리 및 최적화
    /// </summary>
    public void CleanupAndOptimize()
    {
        OptimizedAggressionSystem.CleanupNullReferences();
        System.GC.Collect();
        Debug.Log("Performed cleanup and optimization");
    }

    // GUI 표시
    private void OnGUI()
    {
        if (!showPerformanceGUI) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
        GUILayout.Label("Performance Monitor", GUI.skin.box);
        
        // 성능 정보
        GUILayout.Label($"FPS: {averageFPS:F1} (Target: {targetFPS})");
        
        var stats = OptimizedAggressionSystem.GetSystemStats();
        GUILayout.Label($"Total Systems: {stats.total}");
        GUILayout.Label($"High Detail: {stats.high}");
        GUILayout.Label($"Medium Detail: {stats.medium}");
        GUILayout.Label($"Low Detail: {stats.low}");
        GUILayout.Label($"Disabled: {stats.disabled}");
        
        // 성능 상태 색상
        Color statusColor = averageFPS >= targetFPS ? Color.green : 
                           averageFPS >= targetFPS * 0.8f ? Color.yellow : Color.red;
        GUI.color = statusColor;
        string status = averageFPS >= targetFPS ? "GOOD" : 
                       averageFPS >= targetFPS * 0.8f ? "FAIR" : "POOR";
        GUILayout.Label($"Status: {status}");
        GUI.color = Color.white;
        
        GUILayout.Space(10);
        
        // 제어 버튼들
        if (GUILayout.Button("Force High Quality"))
        {
            ForceGlobalPerformanceLevel(OptimizedAggressionSystem.UpdateLevel.High);
        }
        
        if (GUILayout.Button("Force Low Quality"))
        {
            ForceGlobalPerformanceLevel(OptimizedAggressionSystem.UpdateLevel.Low);
        }
        
        if (GUILayout.Button("Auto Optimize"))
        {
            ToggleAdaptiveOptimization();
        }
        
        if (GUILayout.Button("Cleanup"))
        {
            CleanupAndOptimize();
        }
        
        GUILayout.EndArea();
    }

    // 성능 통계를 위한 정적 메서드들
    public static class PerformanceProfiler
    {
        private static Dictionary<string, float> timings = new Dictionary<string, float>();
        
        public static void StartTiming(string key)
        {
            timings[key] = Time.realtimeSinceStartup;
        }
        
        public static float EndTiming(string key)
        {
            if (timings.TryGetValue(key, out float startTime))
            {
                float elapsed = Time.realtimeSinceStartup - startTime;
                timings.Remove(key);
                return elapsed;
            }
            return 0f;
        }
        
        public static void LogTiming(string operation, float time)
        {
            if (time > 0.001f) // 1ms 이상인 경우만 로그
            {
                Debug.Log($"[Performance] {operation}: {time * 1000f:F2}ms");
            }
        }
    }
}
