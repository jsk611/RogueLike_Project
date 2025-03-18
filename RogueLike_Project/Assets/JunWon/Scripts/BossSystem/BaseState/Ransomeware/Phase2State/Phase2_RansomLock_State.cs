using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class Phase2_RansomLock_State : State<Ransomware>
{
    [Header("스킬락 설정")]
    [SerializeField] private float skillChangeInterval = 10f;  // 스킬 변경 간격
    [SerializeField] private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;
    [SerializeField] private int maxLockedSkills = 2;         // 동시에 잠글 수 있는 최대 스킬 수
    [SerializeField] private float duration = 5f;

    [Header("시각 효과 설정")]
    [SerializeField] private float glitchEffectDuration = 1.0f;   // 글리치 효과 지속 시간
    [SerializeField] private float glitchEffectInterval = 15.0f;  // 글리치 효과 간격

    // 내부 상태
    private float nextGlitchTime = 0f;
    private float nextSkillChangeTime = 0f;
    private bool isActive = false;

    // 참조
    private UIManager uiManager;
    private PlayerControl playerControl;
    private PlayerStatus playerStatus;
    private PostProcessingManager postProcessingManager;

    // 잠글 수 있는 스킬 목록
    private List<SkillType> lockableSkills = new List<SkillType>();

    public Phase2_RansomLock_State(Ransomware owner) : base(owner)
    {
    }

    public override void Enter()
    {
        Debug.Log("Phase2_RansomLock_State: 상태 진입");

        // 필요한 컴포넌트 참조 찾기
        InitializeReferences();

        // 잠글 수 있는 스킬 목록 초기화
        InitializeLockableSkills();

        // 상태 초기화
        nextGlitchTime = glitchEffectInterval;
        nextSkillChangeTime = skillChangeInterval;

        // UI 초기화
        SetupUI();

        // 첫 스킬락 적용
        ApplyLockPattern();

        // 글리치 효과 재생
        PlayGlitchEffect();

        // 시작 알림
        Debug.Log("랜섬웨어 스킬락 시작: 패턴 " + patternType);
    }

    public override void Exit()
    {
        Debug.Log("Phase2_RansomLock_State: 상태 종료");

        // UI 정리
        CleanupUI();

        isActive = false;
    }

    public override void Update()
    {
        if (!isActive) return;

        // 타이머 업데이트

        // 스킬 변경 시간이 되면 패턴 적용
        ApplyLockPattern();

        // 글리치 효과 재생
        PlayGlitchEffect();

        // 주기적인 글리치 효과 업데이트
        UpdateGlitchEffects();

        CompleteState();
    }

    /// <summary>
    /// 참조 초기화
    /// </summary>
    private void InitializeReferences()
    {
        // UI 매니저 찾기
        uiManager = GameObject.FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager를 찾을 수 없습니다. UI 관련 기능이 작동하지 않을 수 있습니다.");
        }

        // 플레이어 컨트롤 찾기
        playerControl = GameObject.FindObjectOfType<PlayerControl>();
        if (playerControl == null)
        {
            Debug.LogError("PlayerControl을 찾을 수 없습니다!");
        }

        // 플레이어 상태 찾기
        playerStatus = GameObject.FindObjectOfType<PlayerStatus>();
        if (playerStatus == null)
        {
            Debug.LogWarning("PlayerStatus를 찾을 수 없습니다!");
        }

        // 포스트 프로세싱 매니저 찾기
        postProcessingManager = GameObject.FindObjectOfType<PostProcessingManager>();
        if (postProcessingManager == null)
        {
            Debug.LogWarning("PostProcessingManager를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 잠글 수 있는 스킬 목록 초기화
    /// </summary>
    private void InitializeLockableSkills()
    {
        lockableSkills.Clear();

        // 랜섬웨어에서 잠글 수 있는 핵심 스킬들
        lockableSkills.Add(SkillType.Running);
        lockableSkills.Add(SkillType.Jumping);
        lockableSkills.Add(SkillType.Dash);
        lockableSkills.Add(SkillType.Movement);

        // 필요하다면 공격 관련 스킬도 추가 가능
        // lockableSkills.Add(SkillType.Shooting);
    }

    /// <summary>
    /// UI 설정
    /// </summary>
    private void SetupUI()
    {
        if (uiManager == null) return;

        // UI에 랜섬웨어 잠금 표시
        // uiManager.ShowRansomLockUI(stateDuration);
        // uiManager.ShowNotification("시스템이 감염되었습니다! 랜섬웨어에 의해 기능이 제한됩니다.");

        // 화면 효과 시작
        if (postProcessingManager != null)
        {
            // 예: 랜섬웨어 감염 효과 (비네팅 색상 변경, 색수차 활성화 등)
            // postProcessingManager.ChangeVignetteColor(Color.red);
            // postProcessingManager.ChangeChromaticAberrationActive(true);
        }
    }

    /// <summary>
    /// UI 정리
    /// </summary>
    private void CleanupUI()
    {
        if (uiManager == null) return;

        // UI에서 랜섬웨어 잠금 표시 제거
        // uiManager.HideRansomLockUI();

        // 화면 효과 종료
        if (postProcessingManager != null)
        {
            // 예: 화면 효과 원복
            // postProcessingManager.ChangeVignetteColor(Color.white);
            // postProcessingManager.ChangeChromaticAberrationActive(false);
        }
    }

    /// <summary>
    /// 설정된 패턴에 따라 스킬 잠금 적용
    /// </summary>
    private void ApplyLockPattern()
    {
        if (playerControl == null || lockableSkills.Count == 0) return;

        // 패턴에 따라 다른 잠금 적용
        switch (patternType)
        {
            case RandomSkillLockController.LockPatternType.Fixed:
                ApplyFixedPattern();
                break;

            case RandomSkillLockController.LockPatternType.RandomRotation:
                ApplyRandomRotationPattern();
                break;

            case RandomSkillLockController.LockPatternType.PulsatingLock:
                ApplyPulsatingPattern();
                break;

            case RandomSkillLockController.LockPatternType.ProgressivelyWorse:
                ApplyProgressivePattern();
                break;

            case RandomSkillLockController.LockPatternType.CompletelyRandom:
                ApplyRandomPattern();
                break;
        }
    }

    /// <summary>
    /// 고정 패턴 적용 - 항상 같은 스킬 잠금
    /// </summary>
    private void ApplyFixedPattern()
    {
        // 첫 번째 두 개의 스킬만 잠금 (설정에 따라 조정 가능)
        int count = Mathf.Min(lockableSkills.Count, 2);

        for (int i = 0; i < count; i++)
        {
            owner.LockPlayerSkill(lockableSkills[i], duration);
        }
    }

    /// <summary>
    /// 랜덤 순환 패턴 - 매번 다른 스킬을 순환하며 잠금
    /// </summary>
    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        // 순환 위치 계산 (순환 주기는 lockableSkills 개수)
        int cycleIndex = Mathf.FloorToInt(Time.time / skillChangeInterval) % lockableSkills.Count;

        // 해당 인덱스의 스킬 잠금
        owner.LockPlayerSkill(lockableSkills[cycleIndex], duration);
    }

    /// <summary>
    /// 맥박 패턴 - 모든 스킬 잠금 후 해제를 반복
    /// </summary>
    private void ApplyPulsatingPattern()
    {
        // 짝수 주기에는 잠금, 홀수 주기에는 아무것도 잠그지 않음
        bool shouldLock = Mathf.FloorToInt(Time.time / skillChangeInterval) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxLockedSkills);

            for (int i = 0; i < count; i++)
            {
                owner.LockPlayerSkill(lockableSkills[i], duration);
            }
        }
        // shouldLock이 false면 모든 스킬을 해제 상태로 유지
    }

    /// <summary>
    /// 점진적 악화 패턴 - 시간이 지날수록 더 많은 스킬 잠금
    /// </summary>
    private void ApplyProgressivePattern()
    {
        if (lockableSkills.Count == 0) return;

        // 감염 진행도에 따라 잠글 스킬 수 결정
        int skillsToLock = 0;
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxLockedSkills, lockableSkills.Count));

        // 복사본 생성 후 섞기
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // 계산된 수만큼 스킬 잠금
        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    /// <summary>
    /// 완전 랜덤 패턴 - 매번 완전히 무작위 스킬 잠금
    /// </summary>
    private void ApplyRandomPattern()
    {
        if (lockableSkills.Count == 0) return;

        // 복사본 생성 후 섞기
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // 무작위로 잠글 스킬 수 결정
        int skillsToLock = Random.Range(1, Mathf.Min(maxLockedSkills, shuffledSkills.Count) + 1);

        // 선택된 수만큼 스킬 잠금
        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    /// <summary>
    /// 상태 완료 및 다음 상태로 전환
    /// </summary>
    private void CompleteState()
    {
        Debug.Log("랜섬웨어 잠금 상태 완료");

        // 다음 상태로 전환
        // owner.ChangeState(owner.GetNextState(this));
    }

    /// <summary>
    /// 글리치 효과 업데이트
    /// </summary>
    private void UpdateGlitchEffects()
    {
        if (Time.time >= nextGlitchTime)
        {
            PlayGlitchEffect();
            nextGlitchTime = Time.time + Random.Range(glitchEffectInterval * 0.8f, glitchEffectInterval * 1.2f);
        }
    }

    /// <summary>
    /// 글리치 효과 재생
    /// </summary>
    private void PlayGlitchEffect()
    {
        Debug.Log("글리치 효과 재생");

        // UI 글리치 효과
        if (uiManager != null)
        {
            // uiManager.PlayGlitchEffect(glitchEffectDuration);
        }

        // 화면 효과 (포스트 프로세싱)
        if (postProcessingManager != null)
        {
            // 예: 화면 깜빡임 효과
            // postProcessingManager.FlashEffect(Color.cyan, 0.2f);
        }
    }

    /// <summary>
    /// 리스트 섞기 (Fisher-Yates 알고리즘)
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
}