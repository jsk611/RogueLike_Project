using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 플레이어의 행동 패턴을 분석하고 미래 위치를 예측하는 시스템
/// </summary>
public class PlayerBehaviorAnalyzer : MonoBehaviour
{
    [Header("Analysis Settings")]
    [SerializeField] private float positionSampleInterval = 0.2f; // 위치 샘플링 간격
    [SerializeField] private int maxSampleCount = 50; // 최대 저장할 샘플 수
    [SerializeField] private float predictionTimeWindow = 2.0f; // 예측 시간 윈도우
    [SerializeField] private float escapeDetectionThreshold = 3.0f; // 도망 패턴 감지 임계값
    
    [Header("Behavior Pattern Weights")]
    [SerializeField] private float velocityWeight = 0.4f;
    [SerializeField] private float directionConsistencyWeight = 0.3f;
    [SerializeField] private float escapePatternWeight = 0.3f;

    // 플레이어 위치 데이터 구조
    [System.Serializable]
    public struct PositionSample
    {
        public Vector3 position;
        public Vector3 velocity;
        public float timestamp;
        public bool isEscaping; // 도망치고 있는지 여부
        public float distanceFromNearestEnemy;
        
        public PositionSample(Vector3 pos, Vector3 vel, float time, bool escaping, float enemyDist)
        {
            position = pos;
            velocity = vel;
            timestamp = time;
            isEscaping = escaping;
            distanceFromNearestEnemy = enemyDist;
        }
    }

    // 예측된 위치 정보
    [System.Serializable]
    public struct PredictedPosition
    {
        public Vector3 position;
        public float confidence; // 예측 신뢰도 (0-1)
        public float timeToReach;
        public bool isEscapeRoute; // 도망 경로인지 여부
        
        public PredictedPosition(Vector3 pos, float conf, float time, bool escape)
        {
            position = pos;
            confidence = conf;
            timeToReach = time;
            isEscapeRoute = escape;
        }
    }

    // 플레이어 행동 패턴 분석 결과
    [System.Serializable]
    public struct BehaviorAnalysis
    {
        public float aggressionLevel; // 공격성 수준 (0-1)
        public float escapeFrequency; // 도망 빈도 (0-1)
        public Vector3 preferredDirection; // 선호하는 이동 방향
        public float averageSpeed; // 평균 이동 속도
        public float predictability; // 예측 가능성 (0-1)
        public float stressLevel; // 스트레스 수준 (0-1)
    }

    private List<PositionSample> positionHistory = new List<PositionSample>();
    private Transform playerTransform;
    private float lastSampleTime;
    private Vector3 lastPlayerPosition;
    
    // 캐시된 분석 결과
    private BehaviorAnalysis cachedAnalysis;
    private float lastAnalysisTime;
    private const float ANALYSIS_UPDATE_INTERVAL = 1.0f;

    // 싱글톤 패턴
    public static PlayerBehaviorAnalyzer Instance { get; private set; }

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
            Debug.LogError("Player not found! PlayerBehaviorAnalyzer requires a GameObject with 'Player' tag.");
            return;
        }
        
        lastPlayerPosition = playerTransform.position;
        lastSampleTime = Time.time;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 주기적으로 플레이어 위치 샘플링
        if (Time.time - lastSampleTime >= positionSampleInterval)
        {
            SamplePlayerPosition();
            lastSampleTime = Time.time;
        }
    }

    /// <summary>
    /// 플레이어 위치를 샘플링하고 분석 데이터에 추가
    /// </summary>
    private void SamplePlayerPosition()
    {
        Vector3 currentPosition = playerTransform.position;
        Vector3 velocity = (currentPosition - lastPlayerPosition) / positionSampleInterval;
        
        // 가장 가까운 적과의 거리 계산
        float nearestEnemyDistance = GetDistanceToNearestEnemy(currentPosition);
        
        // 도망 패턴 감지
        bool isEscaping = DetectEscapePattern(velocity, nearestEnemyDistance);
        
        PositionSample sample = new PositionSample(
            currentPosition, 
            velocity, 
            Time.time, 
            isEscaping, 
            nearestEnemyDistance
        );
        
        positionHistory.Add(sample);
        
        // 최대 샘플 수 제한
        if (positionHistory.Count > maxSampleCount)
        {
            positionHistory.RemoveAt(0);
        }
        
        lastPlayerPosition = currentPosition;
    }

    /// <summary>
    /// 가장 가까운 적과의 거리 계산
    /// </summary>
    private float GetDistanceToNearestEnemy(Vector3 playerPos)
    {
        MonsterBase[] monsters = FindObjectsOfType<MonsterBase>();
        if (monsters.Length == 0) return float.MaxValue;
        
        float minDistance = float.MaxValue;
        foreach (var monster in monsters)
        {
            float distance = Vector3.Distance(playerPos, monster.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        
        return minDistance;
    }

    /// <summary>
    /// 도망 패턴 감지
    /// </summary>
    private bool DetectEscapePattern(Vector3 velocity, float enemyDistance)
    {
        if (positionHistory.Count < 3) return false;
        
        // 최근 3개 샘플의 적과의 거리 변화 분석
        var recentSamples = positionHistory.TakeLast(3).ToList();
        bool distanceIncreasing = true;
        
        for (int i = 1; i < recentSamples.Count; i++)
        {
            if (recentSamples[i].distanceFromNearestEnemy <= recentSamples[i-1].distanceFromNearestEnemy)
            {
                distanceIncreasing = false;
                break;
            }
        }
        
        // 속도가 임계값을 넘고 적과의 거리가 증가하고 있으면 도망으로 판단
        return velocity.magnitude > escapeDetectionThreshold && distanceIncreasing;
    }

    /// <summary>
    /// 플레이어의 미래 위치 예측
    /// </summary>
    public List<PredictedPosition> PredictFuturePositions(float timeHorizon = -1f)
    {
        if (timeHorizon < 0) timeHorizon = predictionTimeWindow;
        
        List<PredictedPosition> predictions = new List<PredictedPosition>();
        
        if (positionHistory.Count < 5) 
        {
            // 데이터가 부족한 경우 현재 속도 기반 단순 예측
            Vector3 currentVelocity = GetCurrentVelocity();
            Vector3 predictedPos = playerTransform.position + currentVelocity * timeHorizon;
            predictions.Add(new PredictedPosition(predictedPos, 0.3f, timeHorizon, false));
            return predictions;
        }
        
        // 여러 시간 포인트에 대한 예측
        float[] timePoints = { 0.5f, 1.0f, 1.5f, 2.0f };
        
        foreach (float t in timePoints)
        {
            if (t > timeHorizon) break;
            
            Vector3 predictedPosition = PredictPositionAtTime(t);
            float confidence = CalculatePredictionConfidence(t);
            bool isEscape = IsLikelyEscapeRoute(predictedPosition);
            
            predictions.Add(new PredictedPosition(predictedPosition, confidence, t, isEscape));
        }
        
        return predictions;
    }

    /// <summary>
    /// 특정 시간 후의 위치 예측
    /// </summary>
    private Vector3 PredictPositionAtTime(float deltaTime)
    {
        if (positionHistory.Count < 2) return playerTransform.position;
        
        // 최근 샘플들을 기반으로 가중 평균 속도 계산
        var recentSamples = positionHistory.TakeLast(10).ToList();
        Vector3 weightedVelocity = Vector3.zero;
        float totalWeight = 0f;
        
        for (int i = 0; i < recentSamples.Count; i++)
        {
            float weight = (i + 1) / (float)recentSamples.Count; // 최근 데이터에 더 높은 가중치
            weightedVelocity += recentSamples[i].velocity * weight;
            totalWeight += weight;
        }
        
        if (totalWeight > 0)
        {
            weightedVelocity /= totalWeight;
        }
        
        // 도망 패턴이 감지된 경우 가속도 적용
        BehaviorAnalysis analysis = GetBehaviorAnalysis();
        if (analysis.escapeFrequency > 0.6f)
        {
            weightedVelocity *= 1.2f; // 도망칠 때는 속도가 증가한다고 가정
        }
        
        return playerTransform.position + weightedVelocity * deltaTime;
    }

    /// <summary>
    /// 예측 신뢰도 계산
    /// </summary>
    private float CalculatePredictionConfidence(float timeHorizon)
    {
        if (positionHistory.Count < 5) return 0.2f;
        
        BehaviorAnalysis analysis = GetBehaviorAnalysis();
        
        // 예측 가능성이 높을수록, 시간 범위가 짧을수록 신뢰도 증가
        float baseConfidence = analysis.predictability;
        float timeDecay = Mathf.Exp(-timeHorizon / 2f); // 시간이 지날수록 신뢰도 감소
        
        return Mathf.Clamp01(baseConfidence * timeDecay);
    }

    /// <summary>
    /// 도망 경로인지 판단
    /// </summary>
    private bool IsLikelyEscapeRoute(Vector3 predictedPosition)
    {
        Vector3 currentPos = playerTransform.position;
        float currentNearestDistance = GetDistanceToNearestEnemy(currentPos);
        float predictedNearestDistance = GetDistanceToNearestEnemy(predictedPosition);
        
        return predictedNearestDistance > currentNearestDistance;
    }

    /// <summary>
    /// 현재 속도 반환
    /// </summary>
    public Vector3 GetCurrentVelocity()
    {
        if (positionHistory.Count < 2) return Vector3.zero;
        return positionHistory.Last().velocity;
    }

    /// <summary>
    /// 플레이어 행동 패턴 분석
    /// </summary>
    public BehaviorAnalysis GetBehaviorAnalysis()
    {
        // 캐시된 분석 결과가 최신인지 확인
        if (Time.time - lastAnalysisTime < ANALYSIS_UPDATE_INTERVAL)
        {
            return cachedAnalysis;
        }
        
        if (positionHistory.Count < 10)
        {
            // 데이터 부족 시 기본값 반환
            cachedAnalysis = new BehaviorAnalysis
            {
                aggressionLevel = 0.5f,
                escapeFrequency = 0.3f,
                preferredDirection = Vector3.forward,
                averageSpeed = 3f,
                predictability = 0.3f,
                stressLevel = 0.5f
            };
            return cachedAnalysis;
        }
        
        var recentSamples = positionHistory.TakeLast(30).ToList();
        
        // 도망 빈도 계산
        float escapeCount = recentSamples.Count(s => s.isEscaping);
        float escapeFrequency = escapeCount / recentSamples.Count;
        
        // 평균 속도 계산
        float averageSpeed = recentSamples.Average(s => s.velocity.magnitude);
        
        // 선호 방향 계산
        Vector3 averageDirection = Vector3.zero;
        foreach (var sample in recentSamples)
        {
            if (sample.velocity.magnitude > 0.1f)
            {
                averageDirection += sample.velocity.normalized;
            }
        }
        averageDirection /= recentSamples.Count;
        
        // 예측 가능성 계산 (방향 일관성 기반)
        float directionConsistency = CalculateDirectionConsistency(recentSamples);
        
        // 공격성 수준 (적에게 가까워지는 빈도)
        float aggressionLevel = CalculateAggressionLevel(recentSamples);
        
        // 스트레스 수준 (속도 변화와 방향 변화 기반)
        float stressLevel = CalculateStressLevel(recentSamples);
        
        cachedAnalysis = new BehaviorAnalysis
        {
            aggressionLevel = aggressionLevel,
            escapeFrequency = escapeFrequency,
            preferredDirection = averageDirection.normalized,
            averageSpeed = averageSpeed,
            predictability = directionConsistency,
            stressLevel = stressLevel
        };
        
        lastAnalysisTime = Time.time;
        return cachedAnalysis;
    }

    private float CalculateDirectionConsistency(List<PositionSample> samples)
    {
        if (samples.Count < 3) return 0.5f;
        
        float totalConsistency = 0f;
        int validSamples = 0;
        
        for (int i = 1; i < samples.Count; i++)
        {
            Vector3 dir1 = samples[i-1].velocity.normalized;
            Vector3 dir2 = samples[i].velocity.normalized;
            
            if (dir1.magnitude > 0.1f && dir2.magnitude > 0.1f)
            {
                float dot = Vector3.Dot(dir1, dir2);
                totalConsistency += (dot + 1f) / 2f; // -1~1을 0~1로 변환
                validSamples++;
            }
        }
        
        return validSamples > 0 ? totalConsistency / validSamples : 0.5f;
    }

    private float CalculateAggressionLevel(List<PositionSample> samples)
    {
        if (samples.Count < 3) return 0.5f;
        
        int approachingCount = 0;
        int validSamples = 0;
        
        for (int i = 1; i < samples.Count; i++)
        {
            if (samples[i-1].distanceFromNearestEnemy > samples[i].distanceFromNearestEnemy)
            {
                approachingCount++;
            }
            validSamples++;
        }
        
        return validSamples > 0 ? (float)approachingCount / validSamples : 0.5f;
    }

    private float CalculateStressLevel(List<PositionSample> samples)
    {
        if (samples.Count < 5) return 0.5f;
        
        // 속도 변화의 표준편차로 스트레스 측정
        float avgSpeed = samples.Average(s => s.velocity.magnitude);
        float speedVariance = samples.Average(s => Mathf.Pow(s.velocity.magnitude - avgSpeed, 2));
        float speedStdDev = Mathf.Sqrt(speedVariance);
        
        // 정규화 (0~1 범위)
        return Mathf.Clamp01(speedStdDev / 5f);
    }

    /// <summary>
    /// 특정 위치가 플레이어의 일반적인 이동 패턴과 얼마나 일치하는지 계산
    /// </summary>
    public float GetMovementPatternMatch(Vector3 targetPosition)
    {
        Vector3 currentPos = playerTransform.position;
        Vector3 directionToTarget = (targetPosition - currentPos).normalized;
        
        BehaviorAnalysis analysis = GetBehaviorAnalysis();
        float directionMatch = Vector3.Dot(directionToTarget, analysis.preferredDirection);
        
        return (directionMatch + 1f) / 2f; // -1~1을 0~1로 변환
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        if (positionHistory.Count < 2) return;
        
        // 이동 경로 표시
        Gizmos.color = Color.blue;
        for (int i = 1; i < positionHistory.Count; i++)
        {
            Gizmos.DrawLine(positionHistory[i-1].position, positionHistory[i].position);
        }
        
        // 도망 지점 표시
        Gizmos.color = Color.red;
        foreach (var sample in positionHistory)
        {
            if (sample.isEscaping)
            {
                Gizmos.DrawWireSphere(sample.position, 0.3f);
            }
        }
        
        // 예측 위치 표시
        if (Application.isPlaying)
        {
            var predictions = PredictFuturePositions();
            Gizmos.color = Color.yellow;
            foreach (var pred in predictions)
            {
                float alpha = pred.confidence;
                Gizmos.color = new Color(1f, 1f, 0f, alpha);
                Gizmos.DrawWireSphere(pred.position, 0.5f);
                
                if (pred.isEscapeRoute)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(pred.position, Vector3.one * 0.3f);
                }
            }
        }
    }
}
