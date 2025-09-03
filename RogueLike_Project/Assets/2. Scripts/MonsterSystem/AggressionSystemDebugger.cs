using UnityEngine;
using System.Linq;

/// <summary>
/// 적극성 시스템 디버깅을 위한 도구
/// </summary>
public class AggressionSystemDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool enableGUI = true;
    [SerializeField] private bool enableGizmos = true;
    [SerializeField] private float updateInterval = 1f;
    
    private float lastUpdateTime;
    private MonsterBase[] monsters;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        RefreshMonsterList();
    }

    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            RefreshMonsterList();
            DebugMonsterStates();
            lastUpdateTime = Time.time;
        }
    }

    private void RefreshMonsterList()
    {
        monsters = FindObjectsOfType<MonsterBase>();
    }

    private void DebugMonsterStates()
    {
        if (!enableDebugLogs || monsters == null) return;

        foreach (var monster in monsters)
        {
            if (monster == null) continue;

            var aggressionSystem = monster.GetComponent<AdaptiveAggressionSystem>();
            var navAgent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            string debugInfo = $"[{monster.name}] ";
            
            // 기본 상태
            debugInfo += $"State: {GetMonsterState(monster)}, ";
            
            // NavMeshAgent 상태
            if (navAgent != null)
            {
                debugInfo += $"NavAgent: {(navAgent.enabled ? "ON" : "OFF")}, ";
                debugInfo += $"OnMesh: {navAgent.isOnNavMesh}, ";
                debugInfo += $"Speed: {navAgent.speed:F1}, ";
                debugInfo += $"Stopped: {navAgent.isStopped}, ";
                
                if (navAgent.hasPath)
                {
                    debugInfo += $"PathStatus: {navAgent.pathStatus}, ";
                    debugInfo += $"Remaining: {navAgent.remainingDistance:F1}, ";
                }
            }
            else
            {
                debugInfo += "NavAgent: NULL, ";
            }
            
            // 적극성 시스템 상태
            if (aggressionSystem != null)
            {
                debugInfo += $"Aggression: {aggressionSystem.GetAggressionLevel():F2}, ";
                debugInfo += $"Aggressive: {aggressionSystem.IsAggressive()}, ";
            }
            
            // 플레이어와의 거리
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(monster.transform.position, playerTransform.position);
                debugInfo += $"PlayerDist: {distance:F1}";
            }
            
            Debug.Log(debugInfo);
        }
    }

    private string GetMonsterState(MonsterBase monster)
    {
        // 리플렉션을 사용하여 private state 필드에 접근
        var stateField = typeof(MonsterBase).GetField("state", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (stateField != null)
        {
            return stateField.GetValue(monster)?.ToString() ?? "UNKNOWN";
        }
        
        return "UNKNOWN";
    }

    private void OnGUI()
    {
        if (!enableGUI || monsters == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 600));
        GUILayout.Label("Aggression System Debugger", GUI.skin.box);
        
        GUILayout.Label($"Monsters Found: {monsters.Length}");
        GUILayout.Label($"Player Position: {(playerTransform ? playerTransform.position.ToString("F1") : "NULL")}");
        
        GUILayout.Space(10);
        
        // 각 몬스터 상태 표시
        foreach (var monster in monsters.Take(10)) // 최대 10개만 표시
        {
            if (monster == null) continue;
            
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Monster: {monster.name}");
            
            var aggressionSystem = monster.GetComponent<AdaptiveAggressionSystem>();
            var navAgent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            // 상태 정보
            GUILayout.Label($"State: {GetMonsterState(monster)}");
            
            if (navAgent != null)
            {
                GUILayout.Label($"NavAgent: {(navAgent.enabled ? "Enabled" : "Disabled")}");
                GUILayout.Label($"On NavMesh: {navAgent.isOnNavMesh}");
                GUILayout.Label($"Speed: {navAgent.speed:F1}");
                GUILayout.Label($"Has Path: {navAgent.hasPath}");
                if (navAgent.hasPath)
                {
                    GUILayout.Label($"Path Status: {navAgent.pathStatus}");
                    GUILayout.Label($"Remaining Distance: {navAgent.remainingDistance:F1}");
                }
            }
            
            if (aggressionSystem != null)
            {
                GUILayout.Label($"Aggression Level: {aggressionSystem.GetAggressionLevel():F2}");
                GUILayout.Label($"Is Aggressive: {aggressionSystem.IsAggressive()}");
            }
            
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(monster.transform.position, playerTransform.position);
                GUILayout.Label($"Distance to Player: {distance:F1}");
            }
            
            // 강제 추적 버튼
            if (GUILayout.Button("Force Chase"))
            {
                ForceMonsterChase(monster);
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        GUILayout.Space(10);
        
        // 전역 제어 버튼들
        if (GUILayout.Button("Force All Monsters to Chase"))
        {
            foreach (var monster in monsters)
            {
                ForceMonsterChase(monster);
            }
        }
        
        if (GUILayout.Button("Reset All Aggression Systems"))
        {
            foreach (var monster in monsters)
            {
                var aggressionSystem = monster.GetComponent<AdaptiveAggressionSystem>();
                aggressionSystem?.ResetToDefaults();
            }
        }
        
        GUILayout.EndArea();
    }

    private void ForceMonsterChase(MonsterBase monster)
    {
        if (monster == null) return;
        
        // State를 CHASE로 강제 변경
        var stateField = typeof(MonsterBase).GetField("state", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (stateField != null)
        {
            // State enum의 CHASE 값 (1)
            stateField.SetValue(monster, 1);
        }
        
        // NavMeshAgent 활성화
        var navAgent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            
            if (playerTransform != null && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(playerTransform.position);
            }
        }
        
        Debug.Log($"Forced {monster.name} to chase player");
    }

    private void OnDrawGizmos()
    {
        if (!enableGizmos || monsters == null || playerTransform == null) return;

        // 플레이어 위치 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerTransform.position, 1f);
        
        // 각 몬스터와 플레이어 간의 연결선 표시
        foreach (var monster in monsters)
        {
            if (monster == null) continue;
            
            var aggressionSystem = monster.GetComponent<AdaptiveAggressionSystem>();
            
            // 적극성 수준에 따른 색상
            Color lineColor = Color.white;
            if (aggressionSystem != null)
            {
                float aggression = aggressionSystem.GetAggressionLevel();
                lineColor = Color.Lerp(Color.blue, Color.red, aggression);
            }
            
            Gizmos.color = lineColor;
            Gizmos.DrawLine(monster.transform.position, playerTransform.position);
            
            // 몬스터 위치 표시
            Gizmos.DrawWireSphere(monster.transform.position, 0.5f);
            
            // NavMeshAgent 경로 표시
            var navAgent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null && navAgent.hasPath)
            {
                Gizmos.color = Color.yellow;
                var path = navAgent.path.corners;
                for (int i = 1; i < path.Length; i++)
                {
                    Gizmos.DrawLine(path[i-1], path[i]);
                }
            }
        }
    }

    // 외부에서 호출 가능한 유틸리티 메서드들
    public void EnableAllAggressionSystems()
    {
        var systems = FindObjectsOfType<AdaptiveAggressionSystem>();
        foreach (var system in systems)
        {
            system.SetAggressionEnabled(true);
        }
        Debug.Log($"Enabled {systems.Length} aggression systems");
    }

    public void DisableAllAggressionSystems()
    {
        var systems = FindObjectsOfType<AdaptiveAggressionSystem>();
        foreach (var system in systems)
        {
            system.SetAggressionEnabled(false);
        }
        Debug.Log($"Disabled {systems.Length} aggression systems");
    }

    public void LogSystemStatus()
    {
        Debug.Log("=== Aggression System Status ===");
        Debug.Log($"Player Behavior Analyzer: {(PlayerBehaviorAnalyzer.Instance != null ? "Active" : "Missing")}");
        Debug.Log($"Dynamic Difficulty Adjuster: {(DynamicDifficultyAdjuster.Instance != null ? "Active" : "Missing")}");
        Debug.Log($"Aggression System Manager: {(AggressionSystemManager.Instance != null ? "Active" : "Missing")}");
        
        var aggressionSystems = FindObjectsOfType<AdaptiveAggressionSystem>();
        Debug.Log($"Active Aggression Systems: {aggressionSystems.Length}");
        
        var encirclementAIs = FindObjectsOfType<EncirclementAI>();
        Debug.Log($"Active Encirclement AIs: {encirclementAIs.Length}");
    }
}
