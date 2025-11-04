using UnityEngine;

/// <summary>
/// 점수 이벤트 타입 정의
/// </summary>
public enum ScoreEventType 
{ 
    Kill,           // 일반 적 처치
    BossKill,       // 보스 처치
    Coin,           // 코인 획득
    Objective,      // 목표 달성
    HitPenalty      // 피격 패널티
}

/// <summary>
/// 게임 점수 규칙을 정의하는 ScriptableObject
/// 디자이너가 Inspector에서 밸런스를 쉽게 조정할 수 있습니다.
/// </summary>
[CreateAssetMenu(menuName = "Game/Score Rules", fileName = "ScoreRules")]
public class ScoreRules : ScriptableObject
{
    [Header("Base Points")]
    [Tooltip("일반 적 처치 시 기본 점수")]
    [Range(1, 1000)]
    public int killBase = 100;
    
    [Tooltip("보스 처치 시 기본 점수 배율")]
    [Range(1f, 10f)]
    public float bossMultiplier = 2f;
    
    [Tooltip("코인 1개당 점수")]
    [Range(1, 100)]
    public int coinValue = 10;
    
    [Tooltip("목표 달성 시 보너스 점수")]
    [Range(100, 10000)]
    public int objectiveReward = 500;
    
    [Tooltip("피격 시 감점 (양수 입력 시 감점으로 처리)")]
    [Range(0, 500)]
    public int hitPenalty = 50;

    [Header("Combo System")]
    [Tooltip("콤보 시스템 사용 여부")]
    public bool useCombo = true;
    
    [Tooltip("콤보 유지 허용 시간(초). 이 시간이 지나면 콤보가 초기화됩니다")]
    [Range(0.5f, 10f)]
    public float comboWindow = 3f;
    
    [Tooltip("콤보 최대 단계 (이 값 이상은 배율이 고정)")]
    [Range(1, 100)]
    public int comboMax = 20;
    
    [Tooltip("콤보 배율 곡선 (X: 0~1 정규화된 콤보, Y: 점수 배율)\n예시: 0콤보=1배, 최대콤보=2배")]
    public AnimationCurve comboCurve = AnimationCurve.Linear(0, 1, 1, 2);

    [Header("Difficulty Scaling")]
    [Tooltip("난이도에 따른 점수 배율 (Easy: 0.5, Normal: 1.0, Hard: 1.5)")]
    [Range(0.1f, 5f)]
    public float difficultyMultiplier = 1f;

    [Header("Score Limits")]
    [Tooltip("단일 이벤트당 최대 획득 가능 점수 (0이면 제한 없음)")]
    [Min(0)]
    public int maxScorePerEvent = 10000;
    
    [Tooltip("음수 점수 허용 여부 (false면 0 이하로 내려가지 않음)")]
    public bool allowNegativeScore = false;

    /// <summary>
    /// 이벤트 타입에 따른 기본 점수를 반환합니다
    /// </summary>
    /// <param name="type">점수 이벤트 타입</param>
    /// <param name="comboCount">현재 콤보 카운트 (선택사항)</param>
    /// <returns>최종 계산된 점수</returns>
    public int PointsFor(ScoreEventType type, int comboCount = 0)
    {
        int basePoints = GetBasePoints(type);
        float comboMult = ComboMultiplier(comboCount);
        
        // 최종 점수 계산
        int finalScore = Mathf.RoundToInt(basePoints * comboMult * difficultyMultiplier);
        
        // 점수 상한 적용
        if (maxScorePerEvent > 0)
        {
            finalScore = Mathf.Min(finalScore, maxScorePerEvent);
        }
        
        // 음수 점수 제한
        if (!allowNegativeScore && finalScore < 0)
        {
            finalScore = 0;
        }
        
        return finalScore;
    }

    /// <summary>
    /// 이벤트 타입별 기본 점수를 반환합니다 (콤보/난이도 배율 미적용)
    /// </summary>
    private int GetBasePoints(ScoreEventType type)
    {
        switch (type)
        {
            case ScoreEventType.BossKill:
                return Mathf.RoundToInt(killBase * bossMultiplier);
            
            case ScoreEventType.Kill:
                return killBase;
            
            case ScoreEventType.Coin:
                return coinValue;
            
            case ScoreEventType.Objective:
                return objectiveReward;
            
            case ScoreEventType.HitPenalty:
                return -Mathf.Abs(hitPenalty); // 항상 음수로 반환
            
            default:
                Debug.LogWarning($"알 수 없는 ScoreEventType: {type}");
                return 0;
        }
    }

    /// <summary>
    /// 현재 콤보 카운트에 따른 배율을 계산합니다
    /// </summary>
    /// <param name="comboCount">현재 콤보 카운트</param>
    /// <returns>콤보 배율 (1.0 이상)</returns>
    public float ComboMultiplier(int comboCount)
    {
        if (!useCombo || comboCount <= 0)
            return 1f;
        
        // 콤보 카운트를 0~1로 정규화
        int clampedCombo = Mathf.Clamp(comboCount, 0, comboMax);
        float normalizedCombo = comboMax > 0 ? (float)clampedCombo / comboMax : 0f;
        
        // 곡선에서 배율 계산
        if (comboCurve != null && comboCurve.length > 0)
        {
            float multiplier = comboCurve.Evaluate(normalizedCombo);
            return Mathf.Max(1f, multiplier); // 최소 1배 보장
        }
        
        return 1f;
    }

    /// <summary>
    /// 현재 콤보 단계의 점수 배율을 UI 표시용으로 반환합니다
    /// </summary>
    /// <param name="comboCount">현재 콤보 카운트</param>
    /// <returns>배율 문자열 (예: "x2.5")</returns>
    public string GetComboMultiplierText(int comboCount)
    {
        float multiplier = ComboMultiplier(comboCount);
        return $"x{multiplier:F1}";
    }
#if UNITY_EDITOR
    /// <summary>
    /// 점수 규칙의 유효성을 검증합니다 (에디터 전용)
    /// </summary>
    private void OnValidate()
    {
        // 음수 값 방지
        killBase = Mathf.Max(1, killBase);
        bossMultiplier = Mathf.Max(1f, bossMultiplier);
        coinValue = Mathf.Max(1, coinValue);
        objectiveReward = Mathf.Max(1, objectiveReward);
        hitPenalty = Mathf.Max(0, hitPenalty);
        
        comboWindow = Mathf.Max(0.1f, comboWindow);
        comboMax = Mathf.Max(1, comboMax);
        difficultyMultiplier = Mathf.Max(0.1f, difficultyMultiplier);
        maxScorePerEvent = Mathf.Max(0, maxScorePerEvent);
        
        // 콤보 곡선 기본값 설정
        if (comboCurve == null || comboCurve.length == 0)
        {
            comboCurve = AnimationCurve.Linear(0, 1, 1, 2);
        }
    }
#endif 

#if UNITY_EDITOR
    [ContextMenu("Reset to Default Values")]
    private void ResetToDefaults()
    {
        killBase = 100;
        bossMultiplier = 2f;
        coinValue = 10;
        objectiveReward = 500;
        hitPenalty = 50;
        
        useCombo = true;
        comboWindow = 3f;
        comboMax = 20;
        comboCurve = AnimationCurve.Linear(0, 1, 1, 2);
        
        difficultyMultiplier = 1f;
        maxScorePerEvent = 10000;
        allowNegativeScore = false;
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("ScoreRules가 기본값으로 초기화되었습니다.");
    }
    
    [ContextMenu("Test Score Calculations")]
    private void TestScoreCalculations()
    {
        Debug.Log("=== Score Rules Test ===");
        Debug.Log($"Kill (0 combo): {PointsFor(ScoreEventType.Kill, 0)}");
        Debug.Log($"Kill (10 combo): {PointsFor(ScoreEventType.Kill, 10)}");
        Debug.Log($"Kill (20 combo): {PointsFor(ScoreEventType.Kill, 20)}");
        Debug.Log($"Boss Kill (0 combo): {PointsFor(ScoreEventType.BossKill, 0)}");
        Debug.Log($"Coin: {PointsFor(ScoreEventType.Coin)}");
        Debug.Log($"Objective: {PointsFor(ScoreEventType.Objective)}");
        Debug.Log($"Hit Penalty: {PointsFor(ScoreEventType.HitPenalty)}");
    }
#endif
}