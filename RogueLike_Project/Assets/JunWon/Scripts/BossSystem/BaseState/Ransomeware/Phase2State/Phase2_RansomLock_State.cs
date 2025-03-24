using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class Phase2_RansomLock_State : State<Ransomware>
{
    private float skillChangeInterval = 10f;  // 스킬 변경 간격
    private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;
    private int maxLockedSkills = 2;         // 동시에 잠글 수 있는 최대 스킬 수
    private float duration = 5f;

    private float glitchEffectDuration = 1.0f;   // 글리치 효과 지속 시간
    private float glitchEffectInterval = 15.0f;  // 글리치 효과 간격

    private float nextGlitchTime = 0f;
    private float nextSkillChangeTime = 0f;
    private bool isActive = false;
    private bool isAttackFinished = false;

    private UIManager uiManager;
    private PlayerControl playerControl;
    private PlayerStatus playerStatus;
    private PostProcessingManager postProcessingManager;

    private List<SkillType> lockableSkills = new List<SkillType>();

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
        nextGlitchTime = glitchEffectInterval;
        nextSkillChangeTime = skillChangeInterval;
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

        isActive = false;
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RansomLock");
    }

    public override void Update()
    {
        if (!isActive) return;
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

    // 애니메이션 이벤트에서 호출
    public void OnApplySkillLock()
    {
        Debug.Log("[Phase2_RansomLock_State] OnApplySkillLock");

        // 실제 스킬 잠금 적용
        ApplyLockPattern();
    }

    // 애니메이션 이벤트에서 호출
    public void OnGlitchEffect()
    {
        Debug.Log("[Phase2_RansomLock_State] OnGlitchEffect");
        PlayGlitchEffect();
    }

    private void InitializeReferences()
    {
        uiManager = GameObject.FindObjectOfType<UIManager>();
        playerControl = GameObject.FindObjectOfType<PlayerControl>();
        playerStatus = GameObject.FindObjectOfType<PlayerStatus>();
        postProcessingManager = GameObject.FindObjectOfType<PostProcessingManager>();
    }

    private void InitializeLockableSkills()
    {
        lockableSkills.Clear();

        lockableSkills.Add(SkillType.Running);
        lockableSkills.Add(SkillType.Jumping);
        lockableSkills.Add(SkillType.Dash);
        lockableSkills.Add(SkillType.Movement);
    }

    private void SetupUI()
    {
        if (uiManager == null) return;

        // UI에 랜섬웨어 잠금 표시
        // uiManager.ShowRansomLockUI(stateDuration);
    }

    private void CleanupUI()
    {
        if (uiManager == null) return;

        // UI에서 랜섬웨어 잠금 표시 제거
        // uiManager.HideRansomLockUI();
    }

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

    private void ApplyFixedPattern()
    {
        int count = Mathf.Min(lockableSkills.Count, 2);

        for (int i = 0; i < count; i++)
        {
            owner.LockPlayerSkill(lockableSkills[i], duration);
        }
    }

    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        int cycleIndex = Mathf.FloorToInt(Time.time / skillChangeInterval) % lockableSkills.Count;
        owner.LockPlayerSkill(lockableSkills[cycleIndex], duration);
    }

    private void ApplyPulsatingPattern()
    {
        bool shouldLock = Mathf.FloorToInt(Time.time / skillChangeInterval) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxLockedSkills);

            for (int i = 0; i < count; i++)
            {
                owner.LockPlayerSkill(lockableSkills[i], duration);
            }
        }
    }

    private void ApplyProgressivePattern()
    {
        if (lockableSkills.Count == 0) return;

        int skillsToLock = 0;
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxLockedSkills, lockableSkills.Count));

        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    private void ApplyRandomPattern()
    {
        if (lockableSkills.Count == 0) return;

        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        int skillsToLock = Random.Range(1, Mathf.Min(maxLockedSkills, shuffledSkills.Count) + 1);

        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    private void UpdateGlitchEffects()
    {
        if (Time.time >= nextGlitchTime)
        {
            PlayGlitchEffect();
            nextGlitchTime = Time.time + Random.Range(glitchEffectInterval * 0.8f, glitchEffectInterval * 1.2f);
        }
    }

    private void PlayGlitchEffect()
    {
        if (uiManager != null)
        {
            // uiManager.PlayGlitchEffect(glitchEffectDuration);
        }
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