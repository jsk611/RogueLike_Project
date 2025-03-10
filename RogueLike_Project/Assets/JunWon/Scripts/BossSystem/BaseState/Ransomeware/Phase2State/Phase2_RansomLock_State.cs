using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase2_RansomLock_State : State<Ransomware>
{
    [Header("스킬락 설정")]
    [SerializeField] private float lockDuration = 60f;      // 잠금 지속 시간(초)
    [SerializeField] private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;

    [Header("시각 효과 설정")]
    [SerializeField] private float glitchEffectDuration = 1.0f;    // 글리치 효과 지속 시간
    [SerializeField] private float glitchEffectInterval = 15.0f;   // 글리치 효과 간격

    // 내부 상태
    private float stateTimer = 0f;
    private float nextGlitchTime = 0f;
    private bool isActive = false;

    // 참조
    private RandomSkillLockController skillLockController;
    private UIManager uiManager;
    private PlayerControl playerControl;

    // 이벤트 구독 상태
    private bool isSubscribed = false;

    public Phase2_RansomLock_State(Ransomware owner) : base(owner)
    {
    }

    public override void Enter()
    {
        Debug.Log("Phase2_RansomLock_State: 상태 진입");

        // 필요한 컴포넌트 참조 찾기
        InitializeReferences();

        // 상태 초기화
        stateTimer = 0f;
        nextGlitchTime = glitchEffectInterval;
        isActive = true;

        // 이벤트 구독
        SubscribeToEvents();

        // UI 초기화
        SetupUI();

        // 스킬락 시작
        StartSkillLock();
    }

    public override void Exit()
    {
        Debug.Log("Phase2_RansomLock_State: 상태 종료");

        // 스킬락 정리
        CleanupSkillLock();

        // 이벤트 구독 해제
        UnsubscribeFromEvents();

        // UI 정리
        CleanupUI();

        isActive = false;
    }

    public override void Update()
    {
        if (!isActive) return;

        // 타이머 업데이트
        stateTimer += Time.deltaTime;

        // 글리치 효과 업데이트
        UpdateGlitchEffects();

        // 상태 만료 확인
        if (stateTimer >= lockDuration)
        {
            CompleteState();
        }
    }

    /// <summary>
    /// 참조 초기화
    /// </summary>
    private void InitializeReferences()
    {
        // 스킬락 컨트롤러 찾거나 생성
        skillLockController = owner.GetComponent<RandomSkillLockController>();
        if (skillLockController == null)
        {
            skillLockController = owner.gameObject.AddComponent<RandomSkillLockController>();
        }

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
    }

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribeToEvents()
    {
        if (isSubscribed) return;

        RandomSkillLockController.OnSkillLockEvent += HandleSkillLockEvent;
        isSubscribed = true;
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (!isSubscribed) return;

        RandomSkillLockController.OnSkillLockEvent -= HandleSkillLockEvent;
        isSubscribed = false;
    }

    /// <summary>
    /// UI 설정
    /// </summary>
    private void SetupUI()
    {
        if (uiManager == null) return;

        //uiManager.ShowRansomLockUI(lockDuration);
        //uiManager.ShowNotification("시스템이 감염되었습니다! 랜섬웨어에 의해 기능이 제한됩니다.");
    }

    /// <summary>
    /// UI 정리
    /// </summary>
    private void CleanupUI()
    {
        if (uiManager == null) return;

        //uiManager.HideRansomLockUI();
    }

    /// <summary>
    /// 스킬락 시작
    /// </summary>
    private void StartSkillLock()
    {
        if (skillLockController == null || playerControl == null) return;

        // 스킬락 컨트롤러 설정
        skillLockController.SetTarget(playerControl);

        skillLockController.SetLockPattern(patternType);
        // 패턴 설정

        // 스킬락 시작
        skillLockController.StartLock(lockDuration);

        // 글리치 효과 재생
        PlayGlitchEffect();
    }

    /// <summary>
    /// 스킬락 정리
    /// </summary>
    private void CleanupSkillLock()
    {
        if (skillLockController == null) return;

        skillLockController.StopLock();
    }

    /// <summary>
    /// 상태 완료 및 다음 상태로 전환
    /// </summary>
    private void CompleteState()
    {
        Debug.Log("랜섬웨어 잠금 상태 완료");

        // 다음 상태로 전환
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
        if (uiManager == null) return;

        //uiManager.PlayGlitchEffect(glitchEffectDuration);
    }

    /// <summary>
    /// 스킬락 이벤트 처리
    /// </summary>
    private void HandleSkillLockEvent(string eventType, List<SkillType> lockedSkills, float remainingTime)
    {
        if (uiManager == null) return;

        switch (eventType)
        {
            case "LockStarted":
                Debug.Log("스킬락 시작됨");
                break;

            case "SkillsChanged":
                // 잠긴 스킬 UI 업데이트
                //UpdateLockedSkillsUI(lockedSkills);

                // 변경 알림 표시
                string skillNames = GetSkillNamesString(lockedSkills);
                //uiManager.ShowNotification($"기능 제한 변경: {skillNames}");

                // 글리치 효과
                PlayGlitchEffect();
                break;

            case "LockEnded":
                Debug.Log("스킬락 종료됨");
                // UI에서 모든 잠금 아이콘 제거
                //UpdateLockedSkillsUI(new List<SkillType>());
                break;
        }

        // 타이머 업데이트
        //uiManager.UpdateLockTimer(remainingTime);
    }

    /// <summary>
    /// 잠긴 스킬 UI 업데이트
    /// </summary>
    //private void UpdateLockedSkillsUI(List<SkillType> lockedSkills)
    //{
    //    if (uiManager == null) return;

    //    // 모든 아이콘 초기화
    //    uiManager.ShowLockedFeatureIcon("running", false);
    //    uiManager.ShowLockedFeatureIcon("jump", false);
    //    uiManager.ShowLockedFeatureIcon("dash", false);
    //    uiManager.ShowLockedFeatureIcon("movement", false);

    //    // 잠긴 스킬만 아이콘 표시
    //    foreach (SkillType skill in lockedSkills)
    //    {
    //        switch (skill)
    //        {
    //            case SkillType.Running:
    //                uiManager.ShowLockedFeatureIcon("running", true);
    //                break;

    //            case SkillType.Jumping:
    //                uiManager.ShowLockedFeatureIcon("jump", true);
    //                break;

    //            case SkillType.Dash:
    //                uiManager.ShowLockedFeatureIcon("dash", true);
    //                break;

    //            case SkillType.Movement:
    //                uiManager.ShowLockedFeatureIcon("movement", true);
    //                break;
    //        }
    //    }
    //}

    /// <summary>
    /// 스킬 이름 목록 문자열 반환
    /// </summary>
    private string GetSkillNamesString(List<SkillType> skills)
    {
        if (skills == null || skills.Count == 0)
        {
            return "없음";
        }

        List<string> skillNames = new List<string>();

        foreach (SkillType skill in skills)
        {
            switch (skill)
            {
                case SkillType.Running:
                    skillNames.Add("달리기");
                    break;

                case SkillType.Jumping:
                    skillNames.Add("점프");
                    break;

                case SkillType.Dash:
                    skillNames.Add("대시");
                    break;

                case SkillType.Movement:
                    skillNames.Add("이동");
                    break;

                default:
                    skillNames.Add(skill.ToString());
                    break;
            }
        }

        return string.Join(", ", skillNames);
    }
}
