# 적응형 몬스터 적극성 시스템 (Adaptive Monster Aggression System)

## 개요
이 시스템은 플레이어와 몬스터 간의 거리에 따라 몬스터의 적극성이 동적으로 변화하는 고급 AI 시스템입니다. 3번 **예측적 포위 전술**과 8번 **동적 난이도 조절 연동 시스템**을 결합하여 구현되었습니다.

## 주요 특징

### 1. 플레이어 행동 패턴 학습 (PlayerBehaviorAnalyzer)
- 플레이어의 이동 패턴, 도망 빈도, 공격성 수준을 실시간으로 분석
- 미래 위치를 예측하여 몬스터가 선제적으로 대응할 수 있도록 지원
- 플레이어의 스트레스 수준과 반응 시간을 측정하여 게임 경험 최적화

### 2. 예측적 포위 전술 (EncirclementAI)
- 여러 몬스터가 협력하여 플레이어를 포위하는 전술적 AI
- 원형, 삼각형, 일렬, 매복 등 다양한 포위 형태 지원
- 플레이어 행동 패턴에 따라 최적의 포위 전술을 자동 선택
- 각 몬스터에게 리더, 측면공격자, 차단자, 매복자 등의 역할 할당

### 3. 동적 난이도 조절 (DynamicDifficultyAdjuster)
- 플레이어의 실력과 게임 상황을 실시간으로 분석
- 체력, 스트레스 수준, 공격성 등을 종합하여 적절한 도전 수준 유지
- 머신러닝 기반 패턴 학습으로 개인화된 게임 경험 제공
- 몬스터의 속도, 적극성, 협력도를 동적으로 조절

### 4. 적응형 적극성 시스템 (AdaptiveAggressionSystem)
- 거리 기반 적극성 증가와 고급 AI 기능들을 통합
- 시각적 피드백 (색상 변화, 파티클 효과)
- 음향 피드백 (적극성 상태 변화 시 사운드)
- 예측 이동과 전술적 위치 선정

## 설치 및 사용법

### 1. 자동 설치 (권장)
```csharp
// 씬에 AggressionSystemManager 프리팹을 배치하거나
// 빈 GameObject에 AggressionSystemManager 스크립트를 추가하세요.
// 자동 초기화가 활성화되어 있으면 모든 시스템이 자동으로 설정됩니다.
```

### 2. 수동 설치
```csharp
// 1. PlayerBehaviorAnalyzer 추가
GameObject analyzerObject = new GameObject("PlayerBehaviorAnalyzer");
analyzerObject.AddComponent<PlayerBehaviorAnalyzer>();

// 2. DynamicDifficultyAdjuster 추가
GameObject adjusterObject = new GameObject("DynamicDifficultyAdjuster");
adjusterObject.AddComponent<DynamicDifficultyAdjuster>();

// 3. 각 몬스터에 AdaptiveAggressionSystem 추가
MonsterBase monster = GetComponent<MonsterBase>();
monster.gameObject.AddComponent<AdaptiveAggressionSystem>();
monster.gameObject.AddComponent<EncirclementAI>();
```

### 3. MonsterBase 설정
기존 MonsterBase를 사용하는 몬스터들은 자동으로 적극성 시스템을 지원합니다:
- `useAdaptiveAggression` 체크박스를 활성화하여 시스템 사용
- 기존 거리 기반 속도 조절 시스템과 자동으로 통합

## API 사용법

### 몬스터 적극성 확인
```csharp
MonsterBase monster = GetComponent<MonsterBase>();

// 현재 적극성 수준 (0-1)
float aggressionLevel = monster.GetCurrentAggressionLevel();

// 적극적 상태인지 확인
bool isAggressive = monster.IsCurrentlyAggressive();

// 적극성 시스템 참조
AdaptiveAggressionSystem aggressionSystem = monster.GetAggressionSystem();
```

### 플레이어 행동 분석
```csharp
PlayerBehaviorAnalyzer analyzer = PlayerBehaviorAnalyzer.Instance;

// 미래 위치 예측
List<PredictedPosition> predictions = analyzer.PredictFuturePositions(2.0f);

// 행동 패턴 분석
BehaviorAnalysis analysis = analyzer.GetBehaviorAnalysis();
Debug.Log($"공격성: {analysis.aggressionLevel}, 도망 빈도: {analysis.escapeFrequency}");
```

### 동적 난이도 조절
```csharp
DynamicDifficultyAdjuster adjuster = DynamicDifficultyAdjuster.Instance;

// 현재 난이도 배수들
float aggressionMult = adjuster.GetAggressionMultiplier();
float speedMult = adjuster.GetSpeedMultiplier();
float distanceThreshold = adjuster.GetDistanceThreshold();

// 데미지 기록 (시스템 학습용)
adjuster.RecordDamageDealt(50f);
adjuster.RecordDamageTaken(25f);
```

### 포위 전술 제어
```csharp
EncirclementAI encirclement = GetComponent<EncirclementAI>();

// 포위 전술 상태 확인
bool isEncircling = encirclement.IsEncirclementActive();
float progress = encirclement.GetEncirclementProgress();
float effectiveness = encirclement.GetTacticalEffectiveness();
```

## 설정 가능한 매개변수

### AdaptiveAggressionSystem
- `baseAggressionThreshold`: 기본 적극성 활성화 거리 (기본값: 8)
- `maxAggressionDistance`: 최대 적극성 거리 (기본값: 15)
- `aggressionBuildupSpeed`: 적극성 축적 속도 (기본값: 2)
- `maxSpeedMultiplier`: 최대 속도 배수 (기본값: 2.5)

### PlayerBehaviorAnalyzer
- `positionSampleInterval`: 위치 샘플링 간격 (기본값: 0.2초)
- `predictionTimeWindow`: 예측 시간 윈도우 (기본값: 2초)
- `escapeDetectionThreshold`: 도망 패턴 감지 임계값 (기본값: 3)

### DynamicDifficultyAdjuster
- `targetChallengeLevel`: 목표 도전 수준 (기본값: 0.7)
- `difficultyAdjustmentSpeed`: 난이도 조절 속도 (기본값: 0.5)
- `enableMachineLearning`: 머신러닝 활성화 (기본값: true)

### EncirclementAI
- `coordinationRadius`: 협력 반경 (기본값: 15)
- `optimalEncircleDistance`: 최적 포위 거리 (기본값: 8)
- `maxCoordinatingMonsters`: 최대 협력 몬스터 수 (기본값: 6)

## 디버그 기능

### 시각적 디버그
- Scene View에서 Gizmos를 통해 예측 위치, 포위 형태, 적극성 수준 등을 시각적으로 확인
- Game View에서 실시간 적극성 수준 표시

### GUI 디버그 패널
- `AggressionSystemManager`의 `enableDebugUI`를 활성화하면 실시간 시스템 상태 확인 가능
- `DynamicDifficultyAdjuster`에서 현재 성능 지표와 난이도 조절 상태 표시

### 로그 출력
```csharp
// 상세 디버그 로그 활성화
Debug.Log($"{monster.name} - Speed: {chaseSpeed:F1}, Aggression: {aggressionLevel:F2}");
```

## 성능 최적화

### 업데이트 빈도 조절
- 적극성 시스템: 10FPS (0.1초 간격)
- 행동 분석: 5FPS (0.2초 간격)  
- 난이도 조절: 0.5FPS (2초 간격)
- 포위 전술: 2FPS (0.5초 간격)

### 메모리 관리
- 성능 히스토리 크기 제한 (기본값: 30개)
- 학습된 패턴 자동 정리
- 불필요한 계산 캐싱

## 확장 가능성

### 새로운 포위 전술 추가
```csharp
// EncirclementAI의 EncirclementType에 새로운 타입 추가
public enum EncirclementType
{
    // ... 기존 타입들
    YourCustomFormation
}
```

### 커스텀 행동 분석 지표
```csharp
// PlayerBehaviorAnalyzer의 PerformanceMetrics 구조체 확장
// 새로운 분석 지표 추가 가능
```

### 개인화된 난이도 프로파일
```csharp
// DynamicDifficultyAdjuster의 LearnedPattern을 활용하여
// 플레이어별 맞춤형 난이도 프로파일 생성 가능
```

## 문제 해결

### 일반적인 문제들

1. **몬스터가 적극성을 보이지 않음**
   - `useAdaptiveAggression`이 체크되어 있는지 확인
   - `baseAggressionThreshold` 값이 너무 높지 않은지 확인
   - Player 태그가 올바르게 설정되어 있는지 확인

2. **포위 전술이 작동하지 않음**
   - 몬스터가 최소 2마리 이상 있는지 확인
   - `coordinationRadius` 내에 다른 몬스터들이 있는지 확인
   - NavMeshAgent가 올바르게 설정되어 있는지 확인

3. **성능 문제**
   - 디버그 UI를 비활성화
   - 업데이트 간격을 늘려서 계산 빈도 감소
   - 불필요한 Gizmos 그리기 비활성화

### 디버그 명령어
```csharp
// 시스템 상태 확인
AggressionSystemManager.Instance.GetSystemStatus();

// 모든 시스템 리셋
AggressionSystemManager.Instance.ResetAllSystems();

// 시스템 활성화/비활성화
AggressionSystemManager.Instance.SetSystemsEnabled(false);
```

## 라이센스 및 크레딧

이 시스템은 **천재적 통찰 도출 공식**과 **다차원적 분석 프레임워크**를 바탕으로 설계되었습니다.

- 예측적 포위 전술: 플레이어 행동 패턴 학습 + 협력적 AI 전술
- 동적 난이도 조절: 실시간 성능 분석 + 머신러닝 기반 적응

---

**버전**: 1.0  
**최종 수정일**: 2024년  
**호환성**: Unity 2021.3 이상
