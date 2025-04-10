using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class Phase2_RansomLock_State : State<Ransomware>
{
    [Header("스킬 잠금 설정")]
    [SerializeField] private float skillChangeInterval = 10f;  // 스킬 변경 간격
    [SerializeField] private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;
    [SerializeField] private int maxLockedSkills = 2;         // 동시에 잠글 수 있는 최대 스킬 수
    [SerializeField] private float lockDuration = 15f;        // 스킬 잠금 지속 시간
    [SerializeField] private float stateMaxDuration = 30f;    // 전체 스킬 잠금 공격 지속 시간

    [Header("글리치 효과 설정")]
    [SerializeField] private float glitchEffectDuration = 1.0f;   // 글리치 효과 지속 시간
    [SerializeField] private float glitchEffectInterval = 15.0f;  // 글리치 효과 간격

    // 내부 변수
    private float nextGlitchTime = 0f;
    private float nextSkillChangeTime = 0f;
    private float stateTimeRemaining = 0f;
    private bool isActive = false;
    private bool isAttackFinished = false;

    // 컴포넌트 참조
    private UIManager uiManager;
    private PlayerControl playerControl;
    private PlayerStatus playerStatus;
    private PostProcessingManager postProcessingManager;
    private SkillLockUI skillLockUI;
    public static event RandomSkillLockController.SkillLockEvent OnRansomLockEvent;


    // 스킬 관련 리스트
    private List<SkillType> lockableSkills = new List<SkillType>();
    private List<SkillType> currentlyLockedSkills = new List<SkillType>();

    public Phase2_RansomLock_State(Ransomware owner) : base(owner)
    {
        owner.SetLockState(this);
    }


    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_RansomLock_State] Enter");
        owner.NmAgent.isStopped = true;

        // 필요한 컴포넌트 참조 찾기
        InitializeReferences();

        // 잠글 수 있는 스킬 목록 초기화
        InitializeLockableSkills();

        // 상태 초기화
        stateTimeRemaining = stateMaxDuration;
        nextGlitchTime = Time.time + glitchEffectInterval * 0.5f;
        nextSkillChangeTime = Time.time + skillChangeInterval;
        isActive = true;

        // UI 초기화
        SetupUI();

        if (CanExecuteAttack())
        {
            // 애니메이션 트리거 설정
            owner.Animator.SetTrigger("RansomLock");
            if (owner.AbilityManager.UseAbility("Lock"))
            {
                // 애니메이션이 알아서 진행됨
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("[Phase2_RansomLock_State] Exit");

        // UI 정리
        CleanupUI();

        // 모든 플레이어 스킬 잠금 해제
        if (playerControl != null)
        {
            playerControl.UnlockAllSkills();
        }

        isActive = false;
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RansomLock");
    }

    public override void Update()
    {
        if (!isActive || isInterrupted) return;

        // 스킬 잠금 공격 전체 지속 시간 체크
        stateTimeRemaining -= Time.deltaTime;
        if (stateTimeRemaining <= 0)
        {
            isAttackFinished = true;
            return;
        }

        // 정기적인 글리치 효과 체크
        if (Time.time >= nextGlitchTime)
        {
            PlayGlitchEffect();
            nextGlitchTime = Time.time + Random.Range(glitchEffectInterval * 0.8f, glitchEffectInterval * 1.2f);
        }

        // 정기적인 스킬 변경 체크
        if (Time.time >= nextSkillChangeTime)
        {
            ApplyLockPattern();
            nextSkillChangeTime = Time.time + skillChangeInterval;
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;

    // 애니메이션 이벤트에서 호출
    public void OnStartLockEffect()
    {
        Debug.Log("[Phase2_RansomLock_State] OnStartLockEffect");

        // 첫 스킬락 적용
        ApplyLockPattern();

        // 글리치 효과 재생
        PlayGlitchEffect();
    }

    private void InitializeReferences()
    {
        uiManager = GameObject.FindObjectOfType<UIManager>();
        playerControl = GameObject.FindObjectOfType<PlayerControl>();
        playerStatus = GameObject.FindObjectOfType<PlayerStatus>();
        postProcessingManager = GameObject.FindObjectOfType<PostProcessingManager>();
        skillLockUI = GameObject.FindObjectOfType<SkillLockUI>();
    }

    private void InitializeLockableSkills()
    {
        lockableSkills.Clear();
        currentlyLockedSkills.Clear();

        // 잠글 수 있는 스킬 목록 설정
        lockableSkills.Add(SkillType.Running);
        lockableSkills.Add(SkillType.Jumping);
        lockableSkills.Add(SkillType.Dash);
        lockableSkills.Add(SkillType.Movement);

        // 게임 플레이에 큰 영향을 주지 않도록 다음 스킬들은 제외
        // lockableSkills.Add(SkillType.Shooting);
        // lockableSkills.Add(SkillType.WeaponSwitch);
        // lockableSkills.Add(SkillType.Interaction);
    }

    private void SetupUI()
    {
        if (uiManager == null) return;

        // 필요한 UI 컴포넌트 활성화
        if (postProcessingManager != null)
        {
            postProcessingManager.EnableGlitchEffect(0.2f);
        }
    }

    private void CleanupUI()
    {
        // 글리치 효과 비활성화
        if (postProcessingManager != null)
        {
            postProcessingManager.DisableGlitchEffect();
        }

        // 스킬 잠금 UI 숨기기
        if (skillLockUI != null)
        {
            TriggerLockEvent("LockEnded", new List<SkillType>(), 0);
        }
    }

    private void ApplyLockPattern()
    {
        if (playerControl == null || lockableSkills.Count == 0) return;

        // 이전에 잠긴 모든 스킬 해제
        UnlockAllSkills();
        currentlyLockedSkills.Clear();

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

        // 현재 잠긴 스킬 로그 출력
        LogLockedSkills();

        // UI 업데이트를 위한 이벤트 발생
        TriggerLockEvent("SkillsChanged", currentlyLockedSkills, lockDuration);
    }

    private void ApplyFixedPattern()
    {
        int count = Mathf.Min(lockableSkills.Count, 2);

        for (int i = 0; i < count; i++)
        {
            LockSkill(lockableSkills[i]);
        }
    }

    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        int cycleIndex = Mathf.FloorToInt(Time.time / skillChangeInterval) % lockableSkills.Count;
        LockSkill(lockableSkills[cycleIndex]);

        // 두 번째 스킬도 랜덤하게 선택 (첫 번째와 다른 스킬)
        if (lockableSkills.Count > 1 && Random.value > 0.5f)
        {
            int secondIndex = (cycleIndex + 1 + Random.Range(0, lockableSkills.Count - 1)) % lockableSkills.Count;
            LockSkill(lockableSkills[secondIndex]);
        }
    }

    private void ApplyPulsatingPattern()
    {
        bool shouldLock = Mathf.FloorToInt(Time.time / skillChangeInterval) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxLockedSkills);

            for (int i = 0; i < count; i++)
            {
                LockSkill(lockableSkills[i]);
            }
        }
    }

    private void ApplyProgressivePattern()
    {
        if (lockableSkills.Count == 0) return;

        // 스테이트 진행도에 따라 잠글 스킬 수 결정 (시간이 갈수록 더 많은 스킬 잠금)
        float progress = 1.0f - (stateTimeRemaining / stateMaxDuration);
        int skillsToLock = Mathf.CeilToInt(progress * maxLockedSkills);
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxLockedSkills, lockableSkills.Count));

        // 복사본 생성 후 섞기
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // 계산된 수만큼 스킬 잠금
        for (int i = 0; i < skillsToLock; i++)
        {
            LockSkill(shuffledSkills[i]);
        }
    }

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
            LockSkill(shuffledSkills[i]);
        }
    }

    private void LockSkill(SkillType skillType)
    {
        if (playerControl == null) return;

        // 플레이어 스킬 비활성화
        playerControl.SetSkillEnabled(skillType, false);
        currentlyLockedSkills.Add(skillType);

        // 실제 랜섬웨어 보스에서는 지속 시간으로 스킬을 잠금
        owner.LockPlayerSkill(skillType, lockDuration);
    }

    private void UnlockAllSkills()
    {
        if (playerControl == null) return;

        foreach (SkillType skill in currentlyLockedSkills)
        {
            playerControl.SetSkillEnabled(skill, true);
        }
    }

    private void LogLockedSkills()
    {
        if (currentlyLockedSkills.Count == 0)
        {
            Debug.Log("현재 잠긴 스킬 없음");
            return;
        }

        string skills = "";
        foreach (SkillType skill in currentlyLockedSkills)
        {
            skills += skill.ToString() + ", ";
        }

        if (skills.Length > 2)
        {
            skills = skills.Substring(0, skills.Length - 2);
        }

        Debug.Log($"현재 잠긴 스킬: {skills}");
    }

    private void PlayGlitchEffect()
    {
        // 포스트 프로세싱 글리치 효과 재생
        if (postProcessingManager != null)
        {
            postProcessingManager.TriggerGlitchEffect(glitchEffectDuration);
        }

        // UI 글리치 효과 재생
        if (skillLockUI != null)
        {
            skillLockUI.PlayGlitchEffect();
        }
    }

    private void TriggerLockEvent(string eventType, List<SkillType> skills, float remainingTime)
    {
        // RandomSkillLockController에서 정의된 이벤트 형식을 직접 호출
        OnRansomLockEvent?.Invoke(eventType, new List<SkillType>(skills), remainingTime);
    }

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