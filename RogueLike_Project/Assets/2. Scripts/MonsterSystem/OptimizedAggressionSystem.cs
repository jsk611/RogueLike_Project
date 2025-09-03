using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 성능 최적화된 적극성 시스템
/// 다수의 몬스터가 있을 때 성능 저하를 최소화
/// </summary>
[RequireComponent(typeof(MonsterBase))]
public class OptimizedAggressionSystem : MonoBehaviour
{
    [Header("Aggression Settings")]
    [SerializeField] private float baseAggressionThreshold = 8f;
    [SerializeField] private float maxAggressionDistance = 15f;
    [SerializeField] private float aggressionBuildupSpeed = 2f;
    [SerializeField] private float aggressionDecaySpeed = 1f;
    
    [Header("Speed Modifiers")]
    [SerializeField] private float minSpeedMultiplier = 1f;
    [SerializeField] private float maxSpeedMultiplier = 2.5f;
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.2f; // 5FPS로 업데이트 (기존 10FPS에서 감소)
    [SerializeField] private float maxUpdateDistance = 50f; // 이 거리를 넘으면 업데이트 중단
    [SerializeField] private bool enableDistanceCulling = true; // 거리 기반 컬링 활성화
    [SerializeField] private bool enableLODSystem = true; // LOD 시스템 활성화
    
    [Header("LOD Settings")]
    [SerializeField] private float highDetailDistance = 15f; // 고품질 업데이트 거리
    [SerializeField] private float mediumDetailDistance = 30f; // 중품질 업데이트 거리
    [SerializeField] private float lowDetailDistance = 50f; // 저품질 업데이트 거리

    // 컴포넌트 참조 (캐시됨)
    private MonsterBase monsterBase;
    private NavMeshAgent navMeshAgent;
    private Transform playerTransform;
    private static PlayerBehaviorAnalyzer behaviorAnalyzer; // static으로 공유
    private static DynamicDifficultyAdjuster difficultyAdjuster; // static으로 공유
    
    // 적극성 상태
    private float currentAggressionLevel = 0f;
    private float baseMovementSpeed;
    private float lastUpdateTime;
    private float distanceToPlayer;
    
    // 성능 최적화 변수
    private UpdateLevel currentUpdateLevel = UpdateLevel.High;
    private bool isActive = true;
    private int frameOffset; // 프레임 분산을 위한 오프셋
    
    // LOD 업데이트 레벨
    public enum UpdateLevel
    {
        Disabled,   // 업데이트 안함
        Low,        // 1초마다
        Medium,     // 0.5초마다  
        High        // 0.2초마다
    }
    
    // 정적 최적화 데이터
    private static readonly Dictionary<UpdateLevel, float> UpdateIntervals = new Dictionary<UpdateLevel, float>
    {
        { UpdateLevel.Disabled, float.MaxValue },
        { UpdateLevel.Low, 1f },
        { UpdateLevel.Medium, 0.5f },
        { UpdateLevel.High, 0.2f }
    };
    
    // 전역 업데이트 매니저 (프레임 분산)
    private static List<OptimizedAggressionSystem> allSystems = new List<OptimizedAggressionSystem>();
    private static int updateIndex = 0;

    private void Awake()
    {
        // 프레임 분산을 위한 랜덤 오프셋
        frameOffset = Random.Range(0, 10);
        
        // 전역 리스트에 추가
        allSystems.Add(this);
    }

    private void Start()
    {
        InitializeComponents();
        InitializeStaticReferences();
        StartOptimizedSystem();
    }

    private void InitializeComponents()
    {
        monsterBase = GetComponent<MonsterBase>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        
        if (playerTransform == null)
        {
            Debug.LogError($"{name}: Player not found!");
            enabled = false;
            return;
        }
        
        baseMovementSpeed = navMeshAgent.speed > 0 ? navMeshAgent.speed : 3.5f;
        if (navMeshAgent.speed <= 0)
        {
            navMeshAgent.speed = baseMovementSpeed;
        }
    }

    private void InitializeStaticReferences()
    {
        // 싱글톤 참조를 static으로 캐시 (메모리 절약)
        if (behaviorAnalyzer == null)
        {
            behaviorAnalyzer = PlayerBehaviorAnalyzer.Instance;
        }
        
        if (difficultyAdjuster == null)
        {
            difficultyAdjuster = DynamicDifficultyAdjuster.Instance;
        }
    }

    private void StartOptimizedSystem()
    {
        StartCoroutine(OptimizedUpdateLoop());
    }

    private IEnumerator OptimizedUpdateLoop()
    {
        // 프레임 분산을 위한 초기 대기
        yield return new WaitForSeconds(frameOffset * 0.02f);
        
        while (enabled && monsterBase != null && isActive)
        {
            // 거리 기반 컬링
            if (enableDistanceCulling)
            {
                distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                
                if (distanceToPlayer > maxUpdateDistance)
                {
                    // 너무 멀면 업데이트 중단
                    yield return new WaitForSeconds(2f);
                    continue;
                }
                
                // LOD 시스템
                if (enableLODSystem)
                {
                    UpdateLODLevel();
                }
            }
            
            // 현재 LOD 레벨에 따른 업데이트
            if (ShouldUpdate())
            {
                UpdateAggressionLevel();
                ApplyAggressionEffects();
                lastUpdateTime = Time.time;
            }
            
            // 현재 LOD 레벨에 따른 대기 시간
            float waitTime = UpdateIntervals[currentUpdateLevel];
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void UpdateLODLevel()
    {
        UpdateLevel newLevel;
        
        if (distanceToPlayer <= highDetailDistance)
        {
            newLevel = UpdateLevel.High;
        }
        else if (distanceToPlayer <= mediumDetailDistance)
        {
            newLevel = UpdateLevel.Medium;
        }
        else if (distanceToPlayer <= lowDetailDistance)
        {
            newLevel = UpdateLevel.Low;
        }
        else
        {
            newLevel = UpdateLevel.Disabled;
        }
        
        currentUpdateLevel = newLevel;
    }

    private bool ShouldUpdate()
    {
        if (currentUpdateLevel == UpdateLevel.Disabled)
            return false;
            
        float timeSinceLastUpdate = Time.time - lastUpdateTime;
        return timeSinceLastUpdate >= UpdateIntervals[currentUpdateLevel];
    }

    private void UpdateAggressionLevel()
    {
        // 거리가 이미 계산되어 있으므로 재사용
        if (distanceToPlayer <= 0)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }
        
        // 동적 난이도 조절 적용 (캐시된 static 참조 사용)
        float dynamicThreshold = baseAggressionThreshold;
        if (difficultyAdjuster != null)
        {
            dynamicThreshold = difficultyAdjuster.GetDistanceThreshold();
        }
        
        // 적극성 활성화 여부 판단
        bool shouldBeAggressive = distanceToPlayer > dynamicThreshold;
        
        if (shouldBeAggressive)
        {
            // 적극성 증가 (최적화된 계산)
            float distanceRatio = Mathf.Clamp01((distanceToPlayer - dynamicThreshold) / 
                                                (maxAggressionDistance - dynamicThreshold));
            float targetAggression = distanceRatio; // 단순화된 곡선
            
            currentAggressionLevel = Mathf.MoveTowards(currentAggressionLevel, targetAggression, 
                                                      aggressionBuildupSpeed * UpdateIntervals[currentUpdateLevel]);
        }
        else
        {
            // 적극성 감소
            currentAggressionLevel = Mathf.MoveTowards(currentAggressionLevel, 0f, 
                                                      aggressionDecaySpeed * UpdateIntervals[currentUpdateLevel]);
        }
        
        // 동적 난이도 적용
        if (difficultyAdjuster != null)
        {
            float aggressionMultiplier = difficultyAdjuster.GetAggressionMultiplier();
            currentAggressionLevel = Mathf.Clamp01(currentAggressionLevel * aggressionMultiplier);
        }
    }

    private void ApplyAggressionEffects()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return;
        
        // 속도 조절 (최적화된 계산)
        float speedMultiplier = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, currentAggressionLevel);
        
        // 동적 난이도 속도 배수 적용
        if (difficultyAdjuster != null)
        {
            speedMultiplier *= difficultyAdjuster.GetSpeedMultiplier();
        }
        
        float newSpeed = baseMovementSpeed * speedMultiplier;
        navMeshAgent.speed = newSpeed;
        
        // MonsterBase의 chaseSpeed 업데이트 (리플렉션 최적화)
        SetMonsterChaseSpeed(newSpeed);
    }

    // 리플렉션 최적화: 캐시된 FieldInfo 사용
    private static System.Reflection.FieldInfo chaseSpeedField;
    
    private void SetMonsterChaseSpeed(float speed)
    {
        if (chaseSpeedField == null)
        {
            chaseSpeedField = typeof(MonsterBase).GetField("chaseSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        
        chaseSpeedField?.SetValue(monsterBase, speed);
    }

    #region Public API (최적화됨)
    
    public float GetAggressionLevel()
    {
        return currentAggressionLevel;
    }
    
    public bool IsAggressive()
    {
        return currentAggressionLevel > 0.3f;
    }
    
    public bool IsHighlyAggressive()
    {
        return currentAggressionLevel > 0.7f;
    }
    
    public Vector3 GetPredictedPlayerPosition()
    {
        // 예측 위치는 고품질 LOD에서만 계산
        if (currentUpdateLevel != UpdateLevel.High || behaviorAnalyzer == null)
            return Vector3.zero;
            
        var predictions = behaviorAnalyzer.PredictFuturePositions(1.5f);
        return predictions.Count > 0 ? predictions[0].position : Vector3.zero;
    }
    
    public float GetPredictionConfidence()
    {
        if (currentUpdateLevel != UpdateLevel.High || behaviorAnalyzer == null)
            return 0f;
            
        var predictions = behaviorAnalyzer.PredictFuturePositions(1.5f);
        return predictions.Count > 0 ? predictions[0].confidence : 0f;
    }
    
    public void SetAggressionEnabled(bool enabled)
    {
        isActive = enabled;
        if (!enabled)
        {
            currentAggressionLevel = 0f;
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = baseMovementSpeed;
            }
        }
    }
    
    public void ResetToDefaults()
    {
        currentAggressionLevel = 0f;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = baseMovementSpeed;
        }
    }
    
    public UpdateLevel GetCurrentUpdateLevel()
    {
        return currentUpdateLevel;
    }
    
    public float GetDistanceToPlayer()
    {
        return distanceToPlayer;
    }
    
    #endregion

    #region Static Optimization Methods
    
    /// <summary>
    /// 모든 적극성 시스템의 업데이트 레벨을 일괄 조정
    /// </summary>
    public static void SetGlobalUpdateLevel(UpdateLevel level)
    {
        foreach (var system in allSystems)
        {
            if (system != null)
            {
                system.currentUpdateLevel = level;
            }
        }
    }
    
    /// <summary>
    /// 성능 모니터링을 위한 통계 정보
    /// </summary>
    public static (int total, int high, int medium, int low, int disabled) GetSystemStats()
    {
        int total = 0, high = 0, medium = 0, low = 0, disabled = 0;
        
        foreach (var system in allSystems)
        {
            if (system == null) continue;
            
            total++;
            switch (system.currentUpdateLevel)
            {
                case UpdateLevel.High: high++; break;
                case UpdateLevel.Medium: medium++; break;
                case UpdateLevel.Low: low++; break;
                case UpdateLevel.Disabled: disabled++; break;
            }
        }
        
        return (total, high, medium, low, disabled);
    }
    
    /// <summary>
    /// 메모리 정리
    /// </summary>
    public static void CleanupNullReferences()
    {
        allSystems.RemoveAll(system => system == null);
    }
    
    #endregion

    private void OnDestroy()
    {
        // 전역 리스트에서 제거
        allSystems.Remove(this);
    }

    // 성능 모니터링용 디버그 정보
    private void OnGUI()
    {
        if (!Application.isPlaying || !enabled) return;
        
        // 화면에 너무 많은 정보가 표시되지 않도록 제한
        if (distanceToPlayer > 20f) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
        {
            string info = $"A:{currentAggressionLevel:F1} L:{currentUpdateLevel} D:{distanceToPlayer:F0}";
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 10, 100, 20), info);
        }
    }

    // 기즈모도 최적화
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || distanceToPlayer > 30f) return;
        
        // 현재 LOD 레벨에 따른 색상
        Color levelColor = currentUpdateLevel switch
        {
            UpdateLevel.High => Color.green,
            UpdateLevel.Medium => Color.yellow,
            UpdateLevel.Low => Color.cyan,
            UpdateLevel.Disabled => Color.red,
            _ => Color.white
        };
        
        Gizmos.color = levelColor;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 3f, currentAggressionLevel);
    }
}
