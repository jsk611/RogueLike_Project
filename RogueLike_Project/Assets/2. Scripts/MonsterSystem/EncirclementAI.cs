using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

/// <summary>
/// 몬스터들이 협력하여 플레이어를 포위하는 전술 AI 시스템
/// </summary>
public class EncirclementAI : MonoBehaviour
{
    [Header("Encirclement Settings")]
    [SerializeField] private float coordinationRadius = 15f; // 협력 반경
    [SerializeField] private float optimalEncircleDistance = 8f; // 최적 포위 거리
    [SerializeField] private float encirclementActivationDistance = 12f; // 포위 전술 활성화 거리
    [SerializeField] private int maxCoordinatingMonsters = 6; // 최대 협력 몬스터 수
    [SerializeField] private float positionUpdateInterval = 0.5f; // 위치 업데이트 간격
    
    [Header("Tactical Behavior")]
    [SerializeField] private float blockingPositionWeight = 0.4f; // 차단 위치 가중치
    [SerializeField] private float flankingPositionWeight = 0.3f; // 측면 공격 위치 가중치
    [SerializeField] private float ambushPositionWeight = 0.3f; // 매복 위치 가중치
    [SerializeField] private float adaptationSpeed = 2f; // 전술 적응 속도
    
    [Header("Formation Types")]
    [SerializeField] private bool useCircularFormation = true;
    [SerializeField] private bool useTriangularFormation = true;
    [SerializeField] private bool useLineFormation = true;
    [SerializeField] private bool useAmbushFormation = true;

    // 포위 전술 타입
    public enum EncirclementType
    {
        None,
        Circular,      // 원형 포위
        Triangular,    // 삼각 포위
        Line,          // 일렬 차단
        Ambush,        // 매복 포위
        Adaptive       // 적응형 포위
    }

    // 몬스터 역할
    public enum MonsterRole
    {
        Leader,        // 지휘관 (가장 가까운 몬스터)
        Flanker,       // 측면 공격자
        Blocker,       // 차단자
        Ambusher,      // 매복자
        Support        // 지원
    }

    // 전술 포지션 정보
    [System.Serializable]
    public struct TacticalPosition
    {
        public Vector3 targetPosition;
        public MonsterRole assignedRole;
        public float priority; // 우선순위 (0-1)
        public float confidence; // 위치 신뢰도
        public bool isBlocking; // 차단 위치인지
        public bool isFlankingRoute; // 측면 공격 경로인지
        
        public TacticalPosition(Vector3 pos, MonsterRole role, float prio, float conf, bool blocking, bool flanking)
        {
            targetPosition = pos;
            assignedRole = role;
            priority = prio;
            confidence = conf;
            isBlocking = blocking;
            isFlankingRoute = flanking;
        }
    }

    // 협력하는 몬스터 정보
    [System.Serializable]
    public struct CoordinatingMonster
    {
        public MonsterBase monster;
        public MonsterRole role;
        public TacticalPosition assignedPosition;
        public float lastUpdateTime;
        public bool isInPosition;
        
        public CoordinatingMonster(MonsterBase m, MonsterRole r, TacticalPosition pos)
        {
            monster = m;
            role = r;
            assignedPosition = pos;
            lastUpdateTime = Time.time;
            isInPosition = false;
        }
    }

    private MonsterBase ownerMonster;
    private Transform playerTransform;
    private PlayerBehaviorAnalyzer behaviorAnalyzer;
    private List<CoordinatingMonster> coordinatingMonsters = new List<CoordinatingMonster>();
    private EncirclementType currentFormationType = EncirclementType.None;
    private float lastPositionUpdate;
    private Vector3 lastPlayerPosition;
    private bool isCoordinating = false;

    // 포위 전술 상태
    private float encirclementProgress = 0f; // 포위 진행도 (0-1)
    private float tacticalEffectiveness = 0f; // 전술 효과도 (0-1)

    private void Start()
    {
        ownerMonster = GetComponent<MonsterBase>();
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        behaviorAnalyzer = PlayerBehaviorAnalyzer.Instance;
        
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! EncirclementAI requires a GameObject with 'Player' tag.");
            enabled = false;
            return;
        }
        
        lastPlayerPosition = playerTransform.position;
    }

    private void Update()
    {
        if (playerTransform == null || ownerMonster == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // 포위 전술 활성화 조건 확인
        if (distanceToPlayer <= encirclementActivationDistance)
        {
            if (!isCoordinating)
            {
                StartCoordination();
            }
            
            // 주기적으로 전술 업데이트
            if (Time.time - lastPositionUpdate >= positionUpdateInterval)
            {
                UpdateEncirclementTactics();
                lastPositionUpdate = Time.time;
            }
        }
        else
        {
            if (isCoordinating)
            {
                StopCoordination();
            }
        }
        
        // 포위 진행도 계산
        UpdateEncirclementProgress();
    }

    /// <summary>
    /// 협력 시작
    /// </summary>
    private void StartCoordination()
    {
        isCoordinating = true;
        FindCoordinatingMonsters();
        DetermineOptimalFormation();
        AssignRolesAndPositions();
        
        Debug.Log($"{ownerMonster.name} started encirclement coordination with {coordinatingMonsters.Count} monsters");
    }

    /// <summary>
    /// 협력 중단
    /// </summary>
    private void StopCoordination()
    {
        isCoordinating = false;
        coordinatingMonsters.Clear();
        currentFormationType = EncirclementType.None;
        encirclementProgress = 0f;
        
        Debug.Log($"{ownerMonster.name} stopped encirclement coordination");
    }

    /// <summary>
    /// 협력할 몬스터들 찾기
    /// </summary>
    private void FindCoordinatingMonsters()
    {
        coordinatingMonsters.Clear();
        
        MonsterBase[] allMonsters = FindObjectsOfType<MonsterBase>();
        List<MonsterBase> nearbyMonsters = new List<MonsterBase>();
        
        foreach (var monster in allMonsters)
        {
            if (monster == ownerMonster) continue;
            
            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance <= coordinationRadius)
            {
                nearbyMonsters.Add(monster);
            }
        }
        
        // 거리순으로 정렬하고 최대 개수만큼 선택
        nearbyMonsters = nearbyMonsters
            .OrderBy(m => Vector3.Distance(transform.position, m.transform.position))
            .Take(maxCoordinatingMonsters)
            .ToList();
        
        // CoordinatingMonster 구조체로 변환
        foreach (var monster in nearbyMonsters)
        {
            TacticalPosition defaultPos = new TacticalPosition(
                monster.transform.position, 
                MonsterRole.Support, 
                0.5f, 
                0.5f, 
                false, 
                false
            );
            
            coordinatingMonsters.Add(new CoordinatingMonster(monster, MonsterRole.Support, defaultPos));
        }
    }

    /// <summary>
    /// 최적의 포위 형태 결정
    /// </summary>
    private void DetermineOptimalFormation()
    {
        if (behaviorAnalyzer == null)
        {
            currentFormationType = EncirclementType.Circular;
            return;
        }
        
        var playerAnalysis = behaviorAnalyzer.GetBehaviorAnalysis();
        int monsterCount = coordinatingMonsters.Count + 1; // 자신 포함
        
        // 플레이어 행동 패턴에 따른 최적 형태 선택
        if (playerAnalysis.escapeFrequency > 0.7f) // 자주 도망치는 플레이어
        {
            if (monsterCount >= 4 && useAmbushFormation)
                currentFormationType = EncirclementType.Ambush;
            else if (useCircularFormation)
                currentFormationType = EncirclementType.Circular;
        }
        else if (playerAnalysis.aggressionLevel > 0.6f) // 공격적인 플레이어
        {
            if (monsterCount >= 3 && useTriangularFormation)
                currentFormationType = EncirclementType.Triangular;
            else if (useLineFormation)
                currentFormationType = EncirclementType.Line;
        }
        else if (playerAnalysis.predictability > 0.8f) // 예측 가능한 플레이어
        {
            currentFormationType = EncirclementType.Adaptive;
        }
        else // 기본값
        {
            if (useCircularFormation)
                currentFormationType = EncirclementType.Circular;
        }
        
        Debug.Log($"Selected formation: {currentFormationType} for {monsterCount} monsters");
    }

    /// <summary>
    /// 역할과 위치 할당
    /// </summary>
    private void AssignRolesAndPositions()
    {
        if (coordinatingMonsters.Count == 0) return;
        
        Vector3 playerPos = playerTransform.position;
        Vector3 playerVelocity = behaviorAnalyzer?.GetCurrentVelocity() ?? Vector3.zero;
        
        // 리더 선정 (플레이어에게 가장 가까운 몬스터)
        var sortedMonsters = coordinatingMonsters
            .OrderBy(m => Vector3.Distance(m.monster.transform.position, playerPos))
            .ToList();
        
        for (int i = 0; i < sortedMonsters.Count; i++)
        {
            var monster = sortedMonsters[i];
            MonsterRole newRole;
            TacticalPosition newPosition;
            
            if (i == 0)
            {
                newRole = MonsterRole.Leader;
                newPosition = CalculateLeaderPosition(playerPos, playerVelocity);
            }
            else
            {
                newRole = AssignOptimalRole(monster.monster, i, sortedMonsters.Count);
                newPosition = CalculateRolePosition(newRole, playerPos, playerVelocity, i);
            }
            
            // 기존 몬스터 정보 업데이트
            for (int j = 0; j < coordinatingMonsters.Count; j++)
            {
                if (coordinatingMonsters[j].monster == monster.monster)
                {
                    var updatedMonster = coordinatingMonsters[j];
                    updatedMonster.role = newRole;
                    updatedMonster.assignedPosition = newPosition;
                    updatedMonster.lastUpdateTime = Time.time;
                    coordinatingMonsters[j] = updatedMonster;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 최적의 역할 할당
    /// </summary>
    private MonsterRole AssignOptimalRole(MonsterBase monster, int index, int totalCount)
    {
        Vector3 monsterPos = monster.transform.position;
        Vector3 playerPos = playerTransform.position;
        Vector3 toPlayer = (playerPos - monsterPos).normalized;
        
        switch (currentFormationType)
        {
            case EncirclementType.Circular:
                return index % 2 == 0 ? MonsterRole.Flanker : MonsterRole.Blocker;
                
            case EncirclementType.Triangular:
                if (index == 1) return MonsterRole.Flanker;
                if (index == 2) return MonsterRole.Flanker;
                return MonsterRole.Blocker;
                
            case EncirclementType.Line:
                return MonsterRole.Blocker;
                
            case EncirclementType.Ambush:
                return index < totalCount / 2 ? MonsterRole.Ambusher : MonsterRole.Flanker;
                
            case EncirclementType.Adaptive:
                return ChooseAdaptiveRole(monster, playerPos);
                
            default:
                return MonsterRole.Support;
        }
    }

    /// <summary>
    /// 적응형 역할 선택
    /// </summary>
    private MonsterRole ChooseAdaptiveRole(MonsterBase monster, Vector3 playerPos)
    {
        if (behaviorAnalyzer == null) return MonsterRole.Support;
        
        var predictions = behaviorAnalyzer.PredictFuturePositions(1.5f);
        if (predictions.Count == 0) return MonsterRole.Support;
        
        Vector3 predictedPos = predictions[0].position;
        Vector3 monsterPos = monster.transform.position;
        
        // 예측 위치와 현재 위치 관계 분석
        float distanceToCurrent = Vector3.Distance(monsterPos, playerPos);
        float distanceToPredicted = Vector3.Distance(monsterPos, predictedPos);
        
        if (distanceToPredicted < distanceToCurrent)
        {
            return MonsterRole.Ambusher; // 예측 위치에 더 가까우면 매복
        }
        else
        {
            return MonsterRole.Flanker; // 측면 공격
        }
    }

    /// <summary>
    /// 리더 위치 계산
    /// </summary>
    private TacticalPosition CalculateLeaderPosition(Vector3 playerPos, Vector3 playerVelocity)
    {
        // 리더는 플레이어를 직접 추적
        Vector3 targetPos = playerPos;
        
        // 플레이어의 이동을 고려한 선제적 위치
        if (playerVelocity.magnitude > 0.5f)
        {
            targetPos += playerVelocity.normalized * 2f;
        }
        
        return new TacticalPosition(
            targetPos,
            MonsterRole.Leader,
            1f,
            0.9f,
            false,
            false
        );
    }

    /// <summary>
    /// 역할별 위치 계산
    /// </summary>
    private TacticalPosition CalculateRolePosition(MonsterRole role, Vector3 playerPos, Vector3 playerVelocity, int index)
    {
        Vector3 targetPos = playerPos;
        bool isBlocking = false;
        bool isFlanking = false;
        float priority = 0.5f;
        float confidence = 0.7f;
        
        switch (role)
        {
            case MonsterRole.Flanker:
                targetPos = CalculateFlankingPosition(playerPos, playerVelocity, index);
                isFlanking = true;
                priority = 0.8f;
                break;
                
            case MonsterRole.Blocker:
                targetPos = CalculateBlockingPosition(playerPos, playerVelocity, index);
                isBlocking = true;
                priority = 0.9f;
                break;
                
            case MonsterRole.Ambusher:
                targetPos = CalculateAmbushPosition(playerPos, playerVelocity, index);
                priority = 0.7f;
                confidence = 0.6f;
                break;
                
            case MonsterRole.Support:
                targetPos = CalculateSupportPosition(playerPos, index);
                priority = 0.4f;
                break;
        }
        
        return new TacticalPosition(targetPos, role, priority, confidence, isBlocking, isFlanking);
    }

    /// <summary>
    /// 측면 공격 위치 계산
    /// </summary>
    private Vector3 CalculateFlankingPosition(Vector3 playerPos, Vector3 playerVelocity, int index)
    {
        float angle = (index * 90f) + 45f; // 45도, 135도, 225도, 315도
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * optimalEncircleDistance;
        
        return playerPos + offset;
    }

    /// <summary>
    /// 차단 위치 계산
    /// </summary>
    private Vector3 CalculateBlockingPosition(Vector3 playerPos, Vector3 playerVelocity, int index)
    {
        if (behaviorAnalyzer == null)
        {
            return playerPos + Vector3.forward * optimalEncircleDistance;
        }
        
        // 플레이어의 예측 경로를 차단
        var predictions = behaviorAnalyzer.PredictFuturePositions(2f);
        if (predictions.Count > 0)
        {
            var escapeRoutes = predictions.Where(p => p.isEscapeRoute).ToList();
            if (escapeRoutes.Count > 0)
            {
                int routeIndex = index % escapeRoutes.Count;
                return escapeRoutes[routeIndex].position;
            }
        }
        
        // 기본 차단 위치
        Vector3 escapeDirection = playerVelocity.magnitude > 0.1f ? playerVelocity.normalized : Vector3.forward;
        return playerPos + escapeDirection * optimalEncircleDistance;
    }

    /// <summary>
    /// 매복 위치 계산
    /// </summary>
    private Vector3 CalculateAmbushPosition(Vector3 playerPos, Vector3 playerVelocity, int index)
    {
        if (behaviorAnalyzer == null)
        {
            return playerPos + Vector3.back * optimalEncircleDistance;
        }
        
        // 플레이어가 갈 가능성이 높은 위치에 미리 대기
        var predictions = behaviorAnalyzer.PredictFuturePositions(3f);
        if (predictions.Count > index)
        {
            return predictions[index].position;
        }
        
        // 플레이어의 선호 방향 반대편에 위치
        var analysis = behaviorAnalyzer.GetBehaviorAnalysis();
        Vector3 oppositeDirection = -analysis.preferredDirection;
        return playerPos + oppositeDirection * (optimalEncircleDistance * 1.5f);
    }

    /// <summary>
    /// 지원 위치 계산
    /// </summary>
    private Vector3 CalculateSupportPosition(Vector3 playerPos, int index)
    {
        // 다른 몬스터들 사이의 빈 공간 채우기
        float angle = (index * 60f); // 60도 간격
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * (optimalEncircleDistance * 0.8f);
        
        return playerPos + offset;
    }

    /// <summary>
    /// 포위 전술 업데이트
    /// </summary>
    private void UpdateEncirclementTactics()
    {
        if (!isCoordinating || coordinatingMonsters.Count == 0) return;
        
        // 플레이어 위치가 크게 변했는지 확인
        float playerMovement = Vector3.Distance(playerTransform.position, lastPlayerPosition);
        if (playerMovement > 3f)
        {
            // 전술 재계산
            DetermineOptimalFormation();
            AssignRolesAndPositions();
        }
        
        lastPlayerPosition = playerTransform.position;
        
        // 각 몬스터의 위치 상태 확인
        UpdateMonsterPositions();
    }

    /// <summary>
    /// 몬스터 위치 상태 업데이트
    /// </summary>
    private void UpdateMonsterPositions()
    {
        for (int i = 0; i < coordinatingMonsters.Count; i++)
        {
            var monster = coordinatingMonsters[i];
            if (monster.monster == null) continue;
            
            float distanceToTarget = Vector3.Distance(
                monster.monster.transform.position, 
                monster.assignedPosition.targetPosition
            );
            
            bool wasInPosition = monster.isInPosition;
            monster.isInPosition = distanceToTarget <= 2f;
            
            // 위치 도달 시 로그
            if (!wasInPosition && monster.isInPosition)
            {
                Debug.Log($"{monster.monster.name} reached {monster.role} position");
            }
            
            coordinatingMonsters[i] = monster;
        }
    }

    /// <summary>
    /// 포위 진행도 업데이트
    /// </summary>
    private void UpdateEncirclementProgress()
    {
        if (!isCoordinating || coordinatingMonsters.Count == 0)
        {
            encirclementProgress = 0f;
            return;
        }
        
        int monstersInPosition = coordinatingMonsters.Count(m => m.isInPosition);
        encirclementProgress = (float)monstersInPosition / coordinatingMonsters.Count;
        
        // 전술 효과도 계산
        CalculateTacticalEffectiveness();
    }

    /// <summary>
    /// 전술 효과도 계산
    /// </summary>
    private void CalculateTacticalEffectiveness()
    {
        if (behaviorAnalyzer == null)
        {
            tacticalEffectiveness = encirclementProgress * 0.5f;
            return;
        }
        
        var analysis = behaviorAnalyzer.GetBehaviorAnalysis();
        
        // 플레이어의 스트레스 수준이 높을수록 전술이 효과적
        float stressBonus = analysis.stressLevel * 0.3f;
        
        // 포위 완성도
        float completionBonus = encirclementProgress * 0.5f;
        
        // 예측 정확도 (플레이어가 예측된 위치에 있는지)
        float predictionAccuracy = 0f;
        var predictions = behaviorAnalyzer.PredictFuturePositions(1f);
        if (predictions.Count > 0)
        {
            float distanceToPrediction = Vector3.Distance(playerTransform.position, predictions[0].position);
            predictionAccuracy = Mathf.Clamp01(1f - (distanceToPrediction / 5f)) * 0.2f;
        }
        
        tacticalEffectiveness = Mathf.Clamp01(stressBonus + completionBonus + predictionAccuracy);
    }

    /// <summary>
    /// 현재 할당된 전술 위치 반환 (다른 AI에서 사용)
    /// </summary>
    public Vector3 GetAssignedTacticalPosition()
    {
        if (!isCoordinating) return transform.position;
        
        // 자신이 리더인지 확인
        var nearestToPlayer = coordinatingMonsters
            .OrderBy(m => Vector3.Distance(m.monster.transform.position, playerTransform.position))
            .FirstOrDefault();
        
        if (nearestToPlayer.monster == null || 
            Vector3.Distance(transform.position, playerTransform.position) <= 
            Vector3.Distance(nearestToPlayer.monster.transform.position, playerTransform.position))
        {
            // 자신이 리더
            return CalculateLeaderPosition(playerTransform.position, behaviorAnalyzer?.GetCurrentVelocity() ?? Vector3.zero).targetPosition;
        }
        
        // 다른 역할
        return transform.position; // 기본값
    }

    /// <summary>
    /// 포위 전술이 활성화되었는지 확인
    /// </summary>
    public bool IsEncirclementActive()
    {
        return isCoordinating;
    }

    /// <summary>
    /// 현재 포위 진행도 반환
    /// </summary>
    public float GetEncirclementProgress()
    {
        return encirclementProgress;
    }

    /// <summary>
    /// 전술 효과도 반환
    /// </summary>
    public float GetTacticalEffectiveness()
    {
        return tacticalEffectiveness;
    }

    /// <summary>
    /// 현재 포위 형태 반환
    /// </summary>
    public EncirclementType GetCurrentFormationType()
    {
        return currentFormationType;
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        if (!isCoordinating || coordinatingMonsters.Count == 0) return;
        
        // 협력 반경 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, coordinationRadius);
        
        // 포위 거리 표시
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, optimalEncircleDistance);
        }
        
        // 각 몬스터의 할당된 위치 표시
        foreach (var monster in coordinatingMonsters)
        {
            if (monster.monster == null) continue;
            
            Color roleColor = GetRoleColor(monster.role);
            Gizmos.color = roleColor;
            
            // 현재 위치에서 목표 위치로 선 그리기
            Gizmos.DrawLine(monster.monster.transform.position, monster.assignedPosition.targetPosition);
            
            // 목표 위치 표시
            if (monster.isInPosition)
            {
                Gizmos.DrawSphere(monster.assignedPosition.targetPosition, 0.3f);
            }
            else
            {
                Gizmos.DrawWireSphere(monster.assignedPosition.targetPosition, 0.5f);
            }
            
            // 역할 표시를 위한 아이콘
            Vector3 iconPos = monster.assignedPosition.targetPosition + Vector3.up * 2f;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(iconPos, Vector3.one * 0.2f);
        }
        
        // 포위 진행도 표시
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector3 progressPos = transform.position + Vector3.up * 3f;
            Gizmos.DrawWireSphere(progressPos, encirclementProgress);
        }
    }

    private Color GetRoleColor(MonsterRole role)
    {
        switch (role)
        {
            case MonsterRole.Leader: return Color.red;
            case MonsterRole.Flanker: return Color.blue;
            case MonsterRole.Blocker: return Color.yellow;
            case MonsterRole.Ambusher: return Color.magenta;
            case MonsterRole.Support: return Color.green;
            default: return Color.white;
        }
    }
}
