using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 기존 MonsterBase와 통합되어 적극성 시스템을 제공하는 컴포넌트
/// 거리 기반 적극성 증가 + 예측적 포위 전술 + 동적 난이도 조절을 통합
/// </summary>
[RequireComponent(typeof(MonsterBase))]
public class AdaptiveAggressionSystem : MonoBehaviour
{
    [Header("Aggression Settings")]
    [SerializeField] private float baseAggressionThreshold = 8f; // 기본 적극성 활성화 거리
    [SerializeField] private float maxAggressionDistance = 15f; // 최대 적극성 거리
    [SerializeField] private float aggressionBuildupSpeed = 2f; // 적극성 축적 속도
    [SerializeField] private float aggressionDecaySpeed = 1f; // 적극성 감소 속도
    [SerializeField] private AnimationCurve aggressionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 적극성 곡선
    
    [Header("Speed Modifiers")]
    [SerializeField] private float minSpeedMultiplier = 1f; // 최소 속도 배수
    [SerializeField] private float maxSpeedMultiplier = 2.5f; // 최대 속도 배수
    [SerializeField] private float originalSpeedRecoveryTime = 3f; // 원래 속도 복구 시간
    
    [Header("Behavior Modifiers")]
    [SerializeField] private bool enablePredictiveMovement = true; // 예측 이동 활성화
    [SerializeField] private bool enableEncirclementTactics = true; // 포위 전술 활성화
    [SerializeField] private bool enableDynamicDifficulty = true; // 동적 난이도 활성화
    [SerializeField] private float tacticalUpdateInterval = 0.5f; // 전술 업데이트 간격
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource; // 오디오 소스
    [SerializeField] private AudioClip[] aggressionSounds; // 적극성 사운드
    [SerializeField] private float soundCooldown = 2f; // 사운드 쿨다운

    // 컴포넌트 참조
    private MonsterBase monsterBase;
    private NavMeshAgent navMeshAgent;
    private EncirclementAI encirclementAI;
    private Transform playerTransform;
    private PlayerBehaviorAnalyzer behaviorAnalyzer;
    private DynamicDifficultyAdjuster difficultyAdjuster;
    
    // 적극성 상태
    private float currentAggressionLevel = 0f; // 현재 적극성 수준 (0-1)
    private float baseMovementSpeed; // 기본 이동 속도
    private float lastTacticalUpdate; // 마지막 전술 업데이트 시간
    private float lastSoundTime; // 마지막 사운드 재생 시간
    private bool wasAggressive = false; // 이전 프레임에서 적극적이었는지
    
    // 예측 이동 관련
    private Vector3 predictedPlayerPosition;
    private float predictionConfidence = 0f;
    private Coroutine aggressionCoroutine;
    
    // 시각적 피드백 관련
    private Material originalMaterial;
    private Color currentColor;

    private void Start()
    {
        InitializeComponents();
        StartAggressionSystem();
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        monsterBase = GetComponent<MonsterBase>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        // 선택적 컴포넌트들
        encirclementAI = GetComponent<EncirclementAI>();
        if (encirclementAI == null && enableEncirclementTactics)
        {
            encirclementAI = gameObject.AddComponent<EncirclementAI>();
        }
        
        // 플레이어 참조
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError($"{name}: Player not found!");
            enabled = false;
            return;
        }
        
        // 싱글톤 시스템들 참조
        behaviorAnalyzer = PlayerBehaviorAnalyzer.Instance;
        difficultyAdjuster = DynamicDifficultyAdjuster.Instance;
        
                 // 기본 속도 저장 (NavMeshAgent가 0이면 기본값 설정)
        baseMovementSpeed = navMeshAgent.speed > 0 ? navMeshAgent.speed : 3.5f;
        if (navMeshAgent.speed <= 0)
        {
            navMeshAgent.speed = baseMovementSpeed;
        }
        
        // 오디오 소스 설정
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D 사운드
            audioSource.volume = 0.7f;
        }
    }


    /// <summary>
    /// 적극성 시스템 시작
    /// </summary>
    private void StartAggressionSystem()
    {
        if (aggressionCoroutine != null)
        {
            StopCoroutine(aggressionCoroutine);
        }
        
        aggressionCoroutine = StartCoroutine(AggressionUpdateLoop());
    }

    /// <summary>
    /// 적극성 업데이트 루프
    /// </summary>
    private IEnumerator AggressionUpdateLoop()
    {
        while (enabled && monsterBase != null)
        {
            UpdateAggressionLevel();
            ApplyAggressionEffects();
            UpdateTacticalBehavior();
            UpdateAudioFeedback();
            
            yield return new WaitForSeconds(0.1f); // 10FPS로 업데이트
        }
    }

    /// <summary>
    /// 적극성 수준 업데이트
    /// </summary>
    private void UpdateAggressionLevel()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // 동적 난이도 조절 적용
        float dynamicThreshold = baseAggressionThreshold;
        if (enableDynamicDifficulty && difficultyAdjuster != null)
        {
            dynamicThreshold = difficultyAdjuster.GetDistanceThreshold();
        }
        
        // 적극성 활성화 여부 판단
        bool shouldBeAggressive = distanceToPlayer > dynamicThreshold;
        
        if (shouldBeAggressive)
        {
            // 적극성 증가
            float distanceRatio = Mathf.Clamp01((distanceToPlayer - dynamicThreshold) / 
                                                (maxAggressionDistance - dynamicThreshold));
            float targetAggression = aggressionCurve.Evaluate(distanceRatio);
            
            currentAggressionLevel = Mathf.MoveTowards(currentAggressionLevel, targetAggression, 
                                                      aggressionBuildupSpeed * Time.deltaTime);
        }
        else
        {
            // 적극성 감소
            currentAggressionLevel = Mathf.MoveTowards(currentAggressionLevel, 0f, 
                                                      aggressionDecaySpeed * Time.deltaTime);
        }
        
        // 동적 난이도 적용
        if (enableDynamicDifficulty && difficultyAdjuster != null)
        {
            float aggressionMultiplier = difficultyAdjuster.GetAggressionMultiplier();
            currentAggressionLevel *= aggressionMultiplier;
            currentAggressionLevel = Mathf.Clamp01(currentAggressionLevel);
        }
    }

         /// <summary>
     /// 적극성 효과 적용
     /// </summary>
     private void ApplyAggressionEffects()
     {
         if (navMeshAgent == null || !navMeshAgent.enabled) return;
         
         // NavMeshAgent가 NavMesh 위에 있는지 확인
         if (!navMeshAgent.isOnNavMesh)
         {
             Debug.LogWarning($"{name}: NavMeshAgent is not on NavMesh!");
             return;
         }
         
         // 속도 조절
         float speedMultiplier = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, currentAggressionLevel);
         
         // 동적 난이도 속도 배수 적용
         if (enableDynamicDifficulty && difficultyAdjuster != null)
         {
             speedMultiplier *= difficultyAdjuster.GetSpeedMultiplier();
         }
         
         float newSpeed = baseMovementSpeed * speedMultiplier;
         navMeshAgent.speed = newSpeed;
         
         // MonsterBase의 chaseSpeed도 업데이트 (리플렉션 사용)
         var chaseSpeedField = typeof(MonsterBase).GetField("chaseSpeed", 
             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
         if (chaseSpeedField != null)
         {
             chaseSpeedField.SetValue(monsterBase, newSpeed);
         }
         
         // 디버그 로그
         if (currentAggressionLevel > 0.1f)
         {
             Debug.Log($"{name}: Aggression {currentAggressionLevel:F2}, Speed {newSpeed:F1}");
         }
     }

    /// <summary>
    /// 전술적 행동 업데이트
    /// </summary>
    private void UpdateTacticalBehavior()
    {
        if (Time.time - lastTacticalUpdate < tacticalUpdateInterval) return;
        
        lastTacticalUpdate = Time.time;
        
        // 예측 이동 업데이트
        if (enablePredictiveMovement && behaviorAnalyzer != null)
        {
            UpdatePredictiveMovement();
        }
        
                 // 포위 전술 상태 확인 (NavMeshAgent 목표 설정은 MonsterBase에서 처리)
        if (enableEncirclementTactics && encirclementAI != null)
        {
            // EncirclementAI가 자동으로 처리하므로 여기서는 상태만 확인
            bool isEncircling = encirclementAI.IsEncirclementActive();
            if (isEncircling && currentAggressionLevel > 0.5f)
            {
                // 포위 전술이 활성화되어 있음을 표시 (실제 이동은 MonsterBase에서 처리)
                Debug.Log($"{name} is in encirclement mode with aggression level: {currentAggressionLevel:F2}");
            }
        }
    }

    /// <summary>
    /// 예측 이동 업데이트
    /// </summary>
    private void UpdatePredictiveMovement()
    {
        var predictions = behaviorAnalyzer.PredictFuturePositions(1.5f);
        if (predictions.Count > 0)
        {
            var bestPrediction = predictions[0];
            
            // 동적 난이도의 예측 정확도 적용
            float accuracy = 1f;
            if (enableDynamicDifficulty && difficultyAdjuster != null)
            {
                accuracy = difficultyAdjuster.GetPredictionAccuracy();
            }
            
            predictionConfidence = bestPrediction.confidence * accuracy;
            
                         // 적극성이 높을 때만 예측 위치 사용 (실제 이동은 MonsterBase에서 처리)
            if (currentAggressionLevel > 0.3f && predictionConfidence > 0.5f)
            {
                predictedPlayerPosition = bestPrediction.position;
                // 예측 위치 정보만 저장하고, 실제 NavMeshAgent 설정은 MonsterBase가 담당
            }
        }
    }

    /// <summary>
    /// 오디오 피드백 업데이트
    /// </summary>
    private void UpdateAudioFeedback()
    {
        //bool isCurrentlyAggressive = currentAggressionLevel > 0.5f;
        
        //// 적극적 상태로 전환될 때 사운드 재생
        //if (isCurrentlyAggressive && !wasAggressive && 
        //    Time.time - lastSoundTime > soundCooldown && 
        //    aggressionSounds.Length > 0)
        //{
        //    AudioClip soundToPlay = aggressionSounds[Random.Range(0, aggressionSounds.Length)];
        //    audioSource.PlayOneShot(soundToPlay);
        //    lastSoundTime = Time.time;
        //}
        
        //wasAggressive = isCurrentlyAggressive;
    }

    /// <summary>
    /// 현재 적극성 수준 반환
    /// </summary>
    public float GetAggressionLevel()
    {
        return currentAggressionLevel;
    }

    /// <summary>
    /// 적극적 상태인지 확인
    /// </summary>
    public bool IsAggressive()
    {
        return currentAggressionLevel > 0.3f;
    }

    /// <summary>
    /// 매우 적극적 상태인지 확인
    /// </summary>
    public bool IsHighlyAggressive()
    {
        return currentAggressionLevel > 0.7f;
    }

    /// <summary>
    /// 예측된 플레이어 위치 반환
    /// </summary>
    public Vector3 GetPredictedPlayerPosition()
    {
        return predictedPlayerPosition;
    }

    /// <summary>
    /// 예측 신뢰도 반환
    /// </summary>
    public float GetPredictionConfidence()
    {
        return predictionConfidence;
    }

    /// <summary>
    /// 적극성 시스템 강제 활성화
    /// </summary>
    public void ForceAggression(float level, float duration = 5f)
    {
        StartCoroutine(ForceAggressionCoroutine(level, duration));
    }

    /// <summary>
    /// 강제 적극성 코루틴
    /// </summary>
    private IEnumerator ForceAggressionCoroutine(float level, float duration)
    {
        float originalLevel = currentAggressionLevel;
        currentAggressionLevel = Mathf.Clamp01(level);
        
        yield return new WaitForSeconds(duration);
        
        currentAggressionLevel = originalLevel;
    }

    /// <summary>
    /// 적극성 시스템 일시 정지/재개
    /// </summary>
    public void SetAggressionEnabled(bool enabled)
    {
        if (enabled)
        {
            if (aggressionCoroutine == null)
            {
                StartAggressionSystem();
            }
        }
        else
        {
            if (aggressionCoroutine != null)
            {
                StopCoroutine(aggressionCoroutine);
                aggressionCoroutine = null;
            }
            currentAggressionLevel = 0f;
            navMeshAgent.speed = baseMovementSpeed;
        }
    }

    /// <summary>
    /// 기본 설정으로 리셋
    /// </summary>
    public void ResetToDefaults()
    {
        currentAggressionLevel = 0f;
        navMeshAgent.speed = baseMovementSpeed;
        predictedPlayerPosition = Vector3.zero;
        predictionConfidence = 0f;
    }

    private void OnDestroy()
    {
        if (aggressionCoroutine != null)
        {
            StopCoroutine(aggressionCoroutine);
        }
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 적극성 활성화 범위 표시
        Gizmos.color = Color.yellow;
        float threshold = enableDynamicDifficulty && difficultyAdjuster != null ? 
            difficultyAdjuster.GetDistanceThreshold() : baseAggressionThreshold;
        Gizmos.DrawWireSphere(transform.position, threshold);
        
        // 최대 적극성 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAggressionDistance);
        
        // 현재 적극성 수준 표시
        Gizmos.color = Color.Lerp(Color.white, Color.red, currentAggressionLevel);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 3f, currentAggressionLevel * 2f);
        
        // 예측된 플레이어 위치 표시
        if (predictedPlayerPosition != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(predictedPlayerPosition, 0.5f);
            Gizmos.DrawLine(transform.position, predictedPlayerPosition);
            
            // 예측 신뢰도 표시
            Gizmos.color = new Color(0f, 1f, 1f, predictionConfidence);
            Gizmos.DrawSphere(predictedPlayerPosition, 0.3f);
        }
    }

    // 디버그용 GUI
    private void OnGUI()
    {
        if (!Application.isPlaying || !enabled) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (screenPos.z > 0)
        {
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 10, 100, 20), 
                     $"Aggr: {currentAggressionLevel:F2}");
        }
    }
}
