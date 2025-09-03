using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 플레이어의 실력과 게임 상황을 실시간으로 분석하여 몬스터의 적극성과 난이도를 동적으로 조절하는 시스템
/// </summary>
public class DynamicDifficultyAdjuster : MonoBehaviour
{
    [Header("Difficulty Analysis Settings")]
    [SerializeField] private float analysisInterval = 2f; // 분석 간격
    [SerializeField] private float difficultyAdjustmentSpeed = 0.5f; // 난이도 조절 속도
    [SerializeField] private int performanceHistorySize = 30; // 성능 기록 크기
    [SerializeField] private float targetChallengeLevel = 0.7f; // 목표 도전 수준 (0-1)
    
    [Header("Player Performance Metrics")]
    [SerializeField] private float healthThreshold = 0.3f; // 체력 임계값
    [SerializeField] private float escapeFrequencyThreshold = 0.6f; // 도망 빈도 임계값
    [SerializeField] private float aggressionBonusThreshold = 0.8f; // 공격성 보너스 임계값
    [SerializeField] private float stressLevelThreshold = 0.7f; // 스트레스 임계값
    
    [Header("Difficulty Adjustment Ranges")]
    [SerializeField] private float minAggressionMultiplier = 0.3f; // 최소 적극성 배수
    [SerializeField] private float maxAggressionMultiplier = 2.5f; // 최대 적극성 배수
    [SerializeField] private float minSpeedMultiplier = 0.5f; // 최소 속도 배수
    [SerializeField] private float maxSpeedMultiplier = 2f; // 최대 속도 배수
    [SerializeField] private float minDistanceThreshold = 3f; // 최소 거리 임계값
    [SerializeField] private float maxDistanceThreshold = 15f; // 최대 거리 임계값

    [Header("Adaptive Learning")]
    [SerializeField] private bool enableMachineLearning = true; // 머신러닝 활성화
    [SerializeField] private float learningRate = 0.1f; // 학습률
    [SerializeField] private int patternRecognitionDepth = 10; // 패턴 인식 깊이

    // 플레이어 성능 데이터 구조
    [System.Serializable]
    public struct PerformanceMetrics
    {
        public float timestamp;
        public float healthPercentage;
        public float damageDealtPerSecond;
        public float damageTakenPerSecond;
        public float averageDistanceFromEnemies;
        public float escapeFrequency;
        public float aggressionLevel;
        public float stressLevel;
        public int enemiesKilled;
        public float survivalTime;
        public bool isUnderPressure; // 압박 상황인지
        public float reactionTime; // 반응 시간
        
        public PerformanceMetrics(float time, float health, float dealtDPS, float takenDPS, 
            float avgDistance, float escape, float aggression, float stress, int kills, 
            float survival, bool pressure, float reaction)
        {
            timestamp = time;
            healthPercentage = health;
            damageDealtPerSecond = dealtDPS;
            damageTakenPerSecond = takenDPS;
            averageDistanceFromEnemies = avgDistance;
            escapeFrequency = escape;
            aggressionLevel = aggression;
            stressLevel = stress;
            enemiesKilled = kills;
            survivalTime = survival;
            isUnderPressure = pressure;
            reactionTime = reaction;
        }
    }

    // 난이도 조절 결과
    [System.Serializable]
    public struct DifficultyAdjustment
    {
        public float aggressionMultiplier; // 적극성 배수
        public float speedMultiplier; // 속도 배수
        public float distanceThreshold; // 거리 임계값
        public float coordinationBonus; // 협력 보너스
        public float predictionAccuracy; // 예측 정확도
        public float adaptiveResponse; // 적응 반응도
        public float overallDifficulty; // 전체 난이도 (0-1)
        public string adjustmentReason; // 조절 이유
        
        public DifficultyAdjustment(float aggro, float speed, float distance, float coord, 
            float prediction, float adaptive, float overall, string reason)
        {
            aggressionMultiplier = aggro;
            speedMultiplier = speed;
            distanceThreshold = distance;
            coordinationBonus = coord;
            predictionAccuracy = prediction;
            adaptiveResponse = adaptive;
            overallDifficulty = overall;
            adjustmentReason = reason;
        }
    }

    // 학습된 플레이어 패턴
    [System.Serializable]
    public struct LearnedPattern
    {
        public string patternName;
        public List<float> inputFeatures; // 입력 특성
        public float expectedDifficulty; // 예상 난이도
        public float confidence; // 신뢰도
        public int occurrenceCount; // 발생 횟수
        
        public LearnedPattern(string name, List<float> features, float difficulty, float conf)
        {
            patternName = name;
            inputFeatures = new List<float>(features);
            expectedDifficulty = difficulty;
            confidence = conf;
            occurrenceCount = 1;
        }
    }

    private PlayerBehaviorAnalyzer behaviorAnalyzer;
    private List<PerformanceMetrics> performanceHistory = new List<PerformanceMetrics>();
    private List<LearnedPattern> learnedPatterns = new List<LearnedPattern>();
    private DifficultyAdjustment currentAdjustment;
    private float lastAnalysisTime;
    private float gameStartTime;
    
    // 플레이어 참조
    private Transform playerTransform;
    private PlayerStatus playerStatus;
    
    // 성능 추적 변수
    private float lastHealthCheck;
    private int lastEnemyKillCount;
    private float totalDamageDealt;
    private float totalDamageTaken;
    private float lastDamageCheckTime;
    private List<float> recentReactionTimes = new List<float>();

    // 싱글톤 패턴
    public static DynamicDifficultyAdjuster Instance { get; private set; }

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
        InitializeSystem();
        gameStartTime = Time.time;
        lastAnalysisTime = Time.time;
        lastDamageCheckTime = Time.time;
        
        // 기본 난이도 설정
        currentAdjustment = new DifficultyAdjustment(
            1f, 1f, 8f, 1f, 1f, 1f, 0.5f, "Initial settings"
        );
    }

    private void InitializeSystem()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform != null)
        {
            playerStatus = playerTransform.GetComponent<PlayerStatus>();
        }
        
        behaviorAnalyzer = PlayerBehaviorAnalyzer.Instance;
        
        if (playerTransform == null || playerStatus == null)
        {
            Debug.LogError("Player components not found! DynamicDifficultyAdjuster requires Player with PlayerStatus.");
            enabled = false;
            return;
        }
        
        lastHealthCheck = playerStatus.GetHealth();
    }

    private void Update()
    {
        if (playerTransform == null || playerStatus == null) return;

        // 주기적으로 성능 분석 및 난이도 조절
        if (Time.time - lastAnalysisTime >= analysisInterval)
        {
            AnalyzePlayerPerformance();
            AdjustDifficulty();
            lastAnalysisTime = Time.time;
        }
        
        // 실시간 반응 시간 측정
        TrackReactionTime();
    }

    /// <summary>
    /// 플레이어 성능 분석
    /// </summary>
    private void AnalyzePlayerPerformance()
    {
        float currentTime = Time.time;
        float deltaTime = currentTime - lastDamageCheckTime;
        
        // 현재 성능 지표 수집
        float healthPercentage = playerStatus.GetHealth() / playerStatus.GetMaxHealth();
        float damageDealtDPS = totalDamageDealt / deltaTime;
        float damageTakenDPS = totalDamageTaken / deltaTime;
        
        // 적들과의 평균 거리 계산
        float avgDistance = CalculateAverageDistanceFromEnemies();
        
        // 행동 분석 데이터 가져오기
        var behaviorData = behaviorAnalyzer?.GetBehaviorAnalysis() ?? default;
        
        // 적 처치 수 계산
        int currentKillCount = GetCurrentKillCount();
        int newKills = currentKillCount - lastEnemyKillCount;
        
        // 압박 상황 판단
        bool underPressure = IsPlayerUnderPressure(healthPercentage, avgDistance, behaviorData.stressLevel);
        
        // 평균 반응 시간 계산
        float avgReactionTime = recentReactionTimes.Count > 0 ? recentReactionTimes.Average() : 0.5f;
        
        // 성능 지표 생성
        PerformanceMetrics metrics = new PerformanceMetrics(
            currentTime,
            healthPercentage,
            damageDealtDPS,
            damageTakenDPS,
            avgDistance,
            behaviorData.escapeFrequency,
            behaviorData.aggressionLevel,
            behaviorData.stressLevel,
            newKills,
            currentTime - gameStartTime,
            underPressure,
            avgReactionTime
        );
        
        // 히스토리에 추가
        performanceHistory.Add(metrics);
        if (performanceHistory.Count > performanceHistorySize)
        {
            performanceHistory.RemoveAt(0);
        }
        
        // 머신러닝 패턴 학습
        if (enableMachineLearning)
        {
            LearnFromPerformance(metrics);
        }
        
        // 변수 리셋
        totalDamageDealt = 0f;
        totalDamageTaken = 0f;
        lastDamageCheckTime = currentTime;
        lastEnemyKillCount = currentKillCount;
        recentReactionTimes.Clear();
        
        Debug.Log($"Performance Analysis - Health: {healthPercentage:F2}, Stress: {behaviorData.stressLevel:F2}, Aggression: {behaviorData.aggressionLevel:F2}");
    }

    /// <summary>
    /// 적들과의 평균 거리 계산
    /// </summary>
    private float CalculateAverageDistanceFromEnemies()
    {
        MonsterBase[] monsters = FindObjectsOfType<MonsterBase>();
        if (monsters.Length == 0) return float.MaxValue;
        
        float totalDistance = 0f;
        int validMonsters = 0;
        
        foreach (var monster in monsters)
        {
            if (monster != null)
            {
                totalDistance += Vector3.Distance(playerTransform.position, monster.transform.position);
                validMonsters++;
            }
        }
        
        return validMonsters > 0 ? totalDistance / validMonsters : float.MaxValue;
    }

    /// <summary>
    /// 플레이어가 압박 상황에 있는지 판단
    /// </summary>
    private bool IsPlayerUnderPressure(float healthPercentage, float avgDistance, float stressLevel)
    {
        return healthPercentage < healthThreshold || 
               avgDistance < 5f || 
               stressLevel > stressLevelThreshold;
    }

    /// <summary>
    /// 현재 적 처치 수 가져오기
    /// </summary>
    private int GetCurrentKillCount()
    {
        // 실제 게임의 킬 카운트 시스템과 연동 필요
        // 임시로 WaveManager에서 가져오는 방식 사용
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        return waveManager != null ? 0 : 0; // 실제 구현 필요
    }

    /// <summary>
    /// 반응 시간 추적
    /// </summary>
    private void TrackReactionTime()
    {
        // 플레이어의 입력 반응 시간 측정 로직
        // 적이 공격할 때부터 플레이어가 회피하기까지의 시간
        // 실제 구현에서는 더 정교한 측정이 필요
        
        if (Input.anyKeyDown)
        {
            // 간단한 반응 시간 시뮬레이션
            float simulatedReactionTime = Random.Range(0.1f, 0.8f);
            recentReactionTimes.Add(simulatedReactionTime);
            
            if (recentReactionTimes.Count > 10)
            {
                recentReactionTimes.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 성능 데이터로부터 패턴 학습
    /// </summary>
    private void LearnFromPerformance(PerformanceMetrics metrics)
    {
        // 입력 특성 벡터 생성
        List<float> features = new List<float>
        {
            metrics.healthPercentage,
            metrics.damageDealtPerSecond,
            metrics.damageTakenPerSecond,
            metrics.averageDistanceFromEnemies / 20f, // 정규화
            metrics.escapeFrequency,
            metrics.aggressionLevel,
            metrics.stressLevel,
            metrics.reactionTime
        };
        
        // 현재 난이도를 기반으로 예상 난이도 계산
        float expectedDifficulty = CalculateExpectedDifficulty(metrics);
        
        // 기존 패턴과 유사한지 확인
        LearnedPattern similarPattern = FindSimilarPattern(features);
        
        if (similarPattern.patternName != null)
        {
            // 기존 패턴 업데이트
            UpdateExistingPattern(similarPattern, expectedDifficulty);
        }
        else
        {
            // 새 패턴 생성
            string patternName = GeneratePatternName(metrics);
            LearnedPattern newPattern = new LearnedPattern(patternName, features, expectedDifficulty, 0.5f);
            learnedPatterns.Add(newPattern);
            
            Debug.Log($"New pattern learned: {patternName}");
        }
    }

    /// <summary>
    /// 예상 난이도 계산
    /// </summary>
    private float CalculateExpectedDifficulty(PerformanceMetrics metrics)
    {
        float difficulty = 0.5f; // 기본값
        
        // 체력이 낮으면 난이도 감소
        if (metrics.healthPercentage < healthThreshold)
        {
            difficulty -= 0.2f;
        }
        
        // 스트레스가 높으면 난이도 감소
        if (metrics.stressLevel > stressLevelThreshold)
        {
            difficulty -= 0.1f;
        }
        
        // 공격성이 높으면 난이도 증가
        if (metrics.aggressionLevel > aggressionBonusThreshold)
        {
            difficulty += 0.2f;
        }
        
        // 도망 빈도가 높으면 난이도 감소
        if (metrics.escapeFrequency > escapeFrequencyThreshold)
        {
            difficulty -= 0.15f;
        }
        
        // 반응 시간이 빠르면 난이도 증가
        if (metrics.reactionTime < 0.3f)
        {
            difficulty += 0.1f;
        }
        
        return Mathf.Clamp01(difficulty);
    }

    /// <summary>
    /// 유사한 패턴 찾기
    /// </summary>
    private LearnedPattern FindSimilarPattern(List<float> features)
    {
        float bestSimilarity = 0f;
        LearnedPattern bestPattern = default;
        
        foreach (var pattern in learnedPatterns)
        {
            float similarity = CalculateFeatureSimilarity(features, pattern.inputFeatures);
            if (similarity > bestSimilarity && similarity > 0.8f) // 80% 이상 유사
            {
                bestSimilarity = similarity;
                bestPattern = pattern;
            }
        }
        
        return bestPattern;
    }

    /// <summary>
    /// 특성 유사도 계산
    /// </summary>
    private float CalculateFeatureSimilarity(List<float> features1, List<float> features2)
    {
        if (features1.Count != features2.Count) return 0f;
        
        float totalDifference = 0f;
        for (int i = 0; i < features1.Count; i++)
        {
            totalDifference += Mathf.Abs(features1[i] - features2[i]);
        }
        
        float averageDifference = totalDifference / features1.Count;
        return Mathf.Clamp01(1f - averageDifference);
    }

    /// <summary>
    /// 기존 패턴 업데이트
    /// </summary>
    private void UpdateExistingPattern(LearnedPattern pattern, float newExpectedDifficulty)
    {
        for (int i = 0; i < learnedPatterns.Count; i++)
        {
            if (learnedPatterns[i].patternName == pattern.patternName)
            {
                var updatedPattern = learnedPatterns[i];
                updatedPattern.expectedDifficulty = Mathf.Lerp(
                    updatedPattern.expectedDifficulty, 
                    newExpectedDifficulty, 
                    learningRate
                );
                updatedPattern.confidence = Mathf.Min(1f, updatedPattern.confidence + 0.1f);
                updatedPattern.occurrenceCount++;
                learnedPatterns[i] = updatedPattern;
                break;
            }
        }
    }

    /// <summary>
    /// 패턴 이름 생성
    /// </summary>
    private string GeneratePatternName(PerformanceMetrics metrics)
    {
        string name = "Pattern_";
        
        if (metrics.healthPercentage < 0.3f) name += "LowHealth_";
        if (metrics.aggressionLevel > 0.7f) name += "Aggressive_";
        if (metrics.escapeFrequency > 0.6f) name += "Defensive_";
        if (metrics.stressLevel > 0.7f) name += "Stressed_";
        if (metrics.reactionTime < 0.3f) name += "FastReaction_";
        
        name += Time.time.ToString("F0");
        return name;
    }

    /// <summary>
    /// 난이도 조절
    /// </summary>
    private void AdjustDifficulty()
    {
        if (performanceHistory.Count < 3) return;
        
        // 최근 성능 데이터 분석
        var recentMetrics = performanceHistory.TakeLast(5).ToList();
        float avgHealth = recentMetrics.Average(m => m.healthPercentage);
        float avgStress = recentMetrics.Average(m => m.stressLevel);
        float avgAggression = recentMetrics.Average(m => m.aggressionLevel);
        float avgEscape = recentMetrics.Average(m => m.escapeFrequency);
        float avgReaction = recentMetrics.Average(m => m.reactionTime);
        
        // 현재 도전 수준 계산
        float currentChallenge = CalculateCurrentChallengeLevel(avgHealth, avgStress, avgAggression);
        
        // 목표 도전 수준과의 차이
        float challengeDifference = targetChallengeLevel - currentChallenge;
        
        // 난이도 조절 계산
        float aggressionMultiplier = Mathf.Lerp(
            currentAdjustment.aggressionMultiplier,
            Mathf.Clamp(1f + challengeDifference, minAggressionMultiplier, maxAggressionMultiplier),
            difficultyAdjustmentSpeed * Time.deltaTime
        );
        
        float speedMultiplier = Mathf.Lerp(
            currentAdjustment.speedMultiplier,
            Mathf.Clamp(1f + challengeDifference * 0.5f, minSpeedMultiplier, maxSpeedMultiplier),
            difficultyAdjustmentSpeed * Time.deltaTime
        );
        
        float distanceThreshold = Mathf.Lerp(
            currentAdjustment.distanceThreshold,
            Mathf.Clamp(8f - challengeDifference * 3f, minDistanceThreshold, maxDistanceThreshold),
            difficultyAdjustmentSpeed * Time.deltaTime
        );
        
        // 특별한 상황 처리
        float coordinationBonus = 1f;
        float predictionAccuracy = 1f;
        float adaptiveResponse = 1f;
        string adjustmentReason = "Normal adjustment";
        
        // 플레이어가 너무 쉽게 플레이하고 있는 경우
        if (avgHealth > 0.8f && avgStress < 0.3f && avgAggression > 0.7f)
        {
            aggressionMultiplier *= 1.3f;
            coordinationBonus = 1.5f;
            predictionAccuracy = 1.3f;
            adjustmentReason = "Player too comfortable - increasing challenge";
        }
        // 플레이어가 너무 힘들어하는 경우
        else if (avgHealth < 0.3f && avgStress > 0.7f && avgEscape > 0.7f)
        {
            aggressionMultiplier *= 0.7f;
            speedMultiplier *= 0.8f;
            distanceThreshold *= 1.2f;
            adjustmentReason = "Player struggling - reducing pressure";
        }
        // 플레이어가 숙련된 경우 (빠른 반응 + 높은 공격성)
        else if (avgReaction < 0.3f && avgAggression > 0.6f)
        {
            predictionAccuracy = 1.4f;
            adaptiveResponse = 1.3f;
            adjustmentReason = "Skilled player detected - enhancing AI intelligence";
        }
        
        // 머신러닝 기반 조절
        if (enableMachineLearning && learnedPatterns.Count > 0)
        {
            ApplyMachineLearningAdjustments(ref aggressionMultiplier, ref speedMultiplier, 
                ref distanceThreshold, ref coordinationBonus, recentMetrics);
        }
        
        // 전체 난이도 계산
        float overallDifficulty = (aggressionMultiplier + speedMultiplier + (1f / (distanceThreshold / 8f))) / 3f;
        
        // 새로운 조절값 적용
        currentAdjustment = new DifficultyAdjustment(
            aggressionMultiplier,
            speedMultiplier,
            distanceThreshold,
            coordinationBonus,
            predictionAccuracy,
            adaptiveResponse,
            overallDifficulty,
            adjustmentReason
        );
        
        Debug.Log($"Difficulty Adjusted: {adjustmentReason} - Overall: {overallDifficulty:F2}");
    }

    /// <summary>
    /// 현재 도전 수준 계산
    /// </summary>
    private float CalculateCurrentChallengeLevel(float avgHealth, float avgStress, float avgAggression)
    {
        // 도전 수준 = (스트레스 + (1-체력) + (1-공격성)) / 3
        return (avgStress + (1f - avgHealth) + (1f - avgAggression)) / 3f;
    }

    /// <summary>
    /// 머신러닝 기반 조절 적용
    /// </summary>
    private void ApplyMachineLearningAdjustments(ref float aggressionMult, ref float speedMult, 
        ref float distanceThresh, ref float coordBonus, List<PerformanceMetrics> recentMetrics)
    {
        if (recentMetrics.Count == 0) return;
        
        var latestMetrics = recentMetrics.Last();
        List<float> currentFeatures = new List<float>
        {
            latestMetrics.healthPercentage,
            latestMetrics.damageDealtPerSecond,
            latestMetrics.damageTakenPerSecond,
            latestMetrics.averageDistanceFromEnemies / 20f,
            latestMetrics.escapeFrequency,
            latestMetrics.aggressionLevel,
            latestMetrics.stressLevel,
            latestMetrics.reactionTime
        };
        
        // 가장 유사한 학습된 패턴 찾기
        LearnedPattern bestMatch = FindSimilarPattern(currentFeatures);
        
        if (bestMatch.patternName != null && bestMatch.confidence > 0.6f)
        {
            // 학습된 패턴을 기반으로 난이도 조절
            float patternInfluence = bestMatch.confidence * 0.3f; // 30% 영향
            
            if (bestMatch.expectedDifficulty > 0.7f)
            {
                aggressionMult *= (1f + patternInfluence);
                speedMult *= (1f + patternInfluence * 0.5f);
            }
            else if (bestMatch.expectedDifficulty < 0.3f)
            {
                aggressionMult *= (1f - patternInfluence);
                speedMult *= (1f - patternInfluence * 0.5f);
                distanceThresh *= (1f + patternInfluence);
            }
            
            Debug.Log($"Applied ML adjustment based on pattern: {bestMatch.patternName}");
        }
    }

    /// <summary>
    /// 현재 난이도 조절 값 반환
    /// </summary>
    public DifficultyAdjustment GetCurrentAdjustment()
    {
        return currentAdjustment;
    }

    /// <summary>
    /// 몬스터의 적극성 배수 반환
    /// </summary>
    public float GetAggressionMultiplier()
    {
        return currentAdjustment.aggressionMultiplier;
    }

    /// <summary>
    /// 몬스터의 속도 배수 반환
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return currentAdjustment.speedMultiplier;
    }

    /// <summary>
    /// 적극성 활성화 거리 임계값 반환
    /// </summary>
    public float GetDistanceThreshold()
    {
        return currentAdjustment.distanceThreshold;
    }

    /// <summary>
    /// 협력 보너스 반환
    /// </summary>
    public float GetCoordinationBonus()
    {
        return currentAdjustment.coordinationBonus;
    }

    /// <summary>
    /// 예측 정확도 배수 반환
    /// </summary>
    public float GetPredictionAccuracy()
    {
        return currentAdjustment.predictionAccuracy;
    }

    /// <summary>
    /// 적응 반응도 배수 반환
    /// </summary>
    public float GetAdaptiveResponse()
    {
        return currentAdjustment.adaptiveResponse;
    }

    /// <summary>
    /// 현재 전체 난이도 반환
    /// </summary>
    public float GetOverallDifficulty()
    {
        return currentAdjustment.overallDifficulty;
    }

    /// <summary>
    /// 최근 성능 지표 반환
    /// </summary>
    public PerformanceMetrics GetLatestPerformanceMetrics()
    {
        return performanceHistory.Count > 0 ? performanceHistory.Last() : default;
    }

    /// <summary>
    /// 데미지 기록 (외부에서 호출)
    /// </summary>
    public void RecordDamageDealt(float damage)
    {
        totalDamageDealt += damage;
    }

    /// <summary>
    /// 받은 데미지 기록 (외부에서 호출)
    /// </summary>
    public void RecordDamageTaken(float damage)
    {
        totalDamageTaken += damage;
    }

    // 디버그용 GUI
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("Dynamic Difficulty Adjuster", GUI.skin.box);
        
        GUILayout.Label($"Aggression Multiplier: {currentAdjustment.aggressionMultiplier:F2}");
        GUILayout.Label($"Speed Multiplier: {currentAdjustment.speedMultiplier:F2}");
        GUILayout.Label($"Distance Threshold: {currentAdjustment.distanceThreshold:F1}");
        GUILayout.Label($"Overall Difficulty: {currentAdjustment.overallDifficulty:F2}");
        GUILayout.Label($"Reason: {currentAdjustment.adjustmentReason}");
        
        GUILayout.Space(10);
        
        if (performanceHistory.Count > 0)
        {
            var latest = performanceHistory.Last();
            GUILayout.Label("Latest Performance:");
            GUILayout.Label($"Health: {latest.healthPercentage:F2}");
            GUILayout.Label($"Stress: {latest.stressLevel:F2}");
            GUILayout.Label($"Aggression: {latest.aggressionLevel:F2}");
            GUILayout.Label($"Escape Frequency: {latest.escapeFrequency:F2}");
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"Learned Patterns: {learnedPatterns.Count}");
        
        GUILayout.EndArea();
    }
}
