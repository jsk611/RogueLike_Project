using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSkillLockController : MonoBehaviour
{
    [Header("스킬락 설정")]
    [SerializeField] private float defaultLockDuration = 60f;  // 기본 잠금 지속 시간(초)
    [SerializeField] private float skillChangeDuration = 10f;  // 스킬 변경 주기(초)

    [Header("잠금 패턴 설정")]
    [SerializeField] private LockPatternType lockPatternType = LockPatternType.RandomRotation;
    [Range(1, 3)]
    [SerializeField] private int maxSimultaneousLockedSkills = 2;  // 동시에 잠글 수 있는 최대 스킬 수

    [Header("잠금 가능한 스킬 설정")]
    [SerializeField] private List<SkillType> lockableSkills = new List<SkillType>();

    [Header("효과 설정")]
    [SerializeField] private AudioClip lockSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip glitchSound;

    // 내부 변수
    private ISkillLockable targetObject;
    private AudioSource audioSource;
    private bool isLockActive = false;
    private float lockTimer = 0f;
    private float nextSkillChangeTime = 0f;
    private List<SkillType> currentlyLockedSkills = new List<SkillType>();

    // 이벤트 정의
    public delegate void SkillLockEvent(string eventType, List<SkillType> lockedSkills, float remainingTime);
    public static event SkillLockEvent OnSkillLockEvent;

    // 스킬락 패턴 타입
    public enum LockPatternType
    {
        Fixed,              // 고정된 스킬만 잠금
        RandomRotation,     // 순차적으로 랜덤 스킬 잠금
        PulsatingLock,      // 모든 스킬 한번씩 잠금 후 해제
        ProgressivelyWorse, // 시간이 지날수록 더 많은 스킬 잠금
        CompletelyRandom    // 완전 랜덤
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 기본 잠금 가능 스킬 설정 (설정되지 않은 경우)
        if (lockableSkills.Count == 0)
        {
            lockableSkills.Add(SkillType.Running);
            lockableSkills.Add(SkillType.Jumping);
            lockableSkills.Add(SkillType.Dash);
        }
    }

    /// <summary>
    /// 스킬락 대상 설정
    /// </summary>
    /// <param name="target">ISkillLockable 인터페이스 구현 대상</param>
    public void SetTarget(ISkillLockable target)
    {
        targetObject = target;
    }
    public void SetLockPattern(LockPatternType pattern)
    {
        lockPatternType = pattern;
    }

    /// <summary>
    /// 스킬락 시작
    /// </summary>
    /// <param name="duration">잠금 지속 시간 (초). 기본값 사용 시 0 또는 음수 입력</param>
    public void StartLock(float duration = 0)
    {
        if (isLockActive || targetObject == null)
            return;

        // 유효한 지속 시간 설정
        float lockDuration = duration > 0 ? duration : defaultLockDuration;

        // 스킬락 코루틴 시작
        StartCoroutine(SkillLockCoroutine(lockDuration));
    }


    /// <summary>
    /// 스킬락 강제 종료
    /// </summary>
    public void StopLock()
    {
        if (!isLockActive || targetObject == null)
            return;

        StopAllCoroutines();
        EndLock();
    }

    /// <summary>
    /// 스킬락 코루틴
    /// </summary>
    private IEnumerator SkillLockCoroutine(float duration)
    {
        isLockActive = true;
        lockTimer = 0f;
        nextSkillChangeTime = 0f;

        // 초기 잠금 적용
        ApplyLockPattern();

        // 스킬락 시작 이벤트 발생
        TriggerLockEvent("LockStarted", currentlyLockedSkills, duration);

        // 잠금 사운드 재생
        PlaySound(lockSound);

        Debug.Log($"스킬락 시작: {duration}초 동안");

        while (lockTimer < duration)
        {
            lockTimer += Time.deltaTime;
            float remainingTime = duration - lockTimer;

            // 스킬 변경 시간이 되면 새로운 스킬 잠금 패턴 적용
            if (lockTimer >= nextSkillChangeTime)
            {
                ApplyLockPattern();
                nextSkillChangeTime = lockTimer + skillChangeDuration;

                // 스킬 변경 이벤트 발생
                TriggerLockEvent("SkillsChanged", currentlyLockedSkills, remainingTime);

                // 글리치 효과 재생
                PlaySound(glitchSound);
            }

            yield return null;
        }

        // 스킬락 종료
        EndLock();
    }

    /// <summary>
    /// 스킬락 종료 처리
    /// </summary>
    private void EndLock()
    {
        if (targetObject != null)
        {
            // 모든 스킬 잠금 해제
            targetObject.UnlockAllSkills();
        }

        isLockActive = false;
        currentlyLockedSkills.Clear();

        // 잠금 해제 사운드 재생
        PlaySound(unlockSound);

        // 스킬락 종료 이벤트 발생
        TriggerLockEvent("LockEnded", new List<SkillType>(), 0);

        Debug.Log("스킬락 종료됨");
    }

    /// <summary>
    /// 설정된 패턴에 따라 스킬 잠금 적용
    /// </summary>
    private void ApplyLockPattern()
    {
        if (targetObject == null || lockableSkills.Count == 0)
            return;

        // 이전에 잠긴 모든 스킬 해제
        UnlockAllSkills();
        currentlyLockedSkills.Clear();

        switch (lockPatternType)
        {
            case LockPatternType.Fixed:
                ApplyFixedPattern();
                break;

            case LockPatternType.RandomRotation:
                ApplyRandomRotationPattern();
                break;

            case LockPatternType.PulsatingLock:
                ApplyPulsatingPattern();
                break;

            case LockPatternType.ProgressivelyWorse:
                ApplyProgressivePattern();
                break;

            case LockPatternType.CompletelyRandom:
                ApplyRandomPattern();
                break;
        }

        // 현재 잠긴 스킬 로그 출력
        LogLockedSkills();
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
            LockSkill(lockableSkills[i]);
        }
    }

    /// <summary>
    /// 랜덤 순환 패턴 - 매번 다른 스킬을 순환하며 잠금
    /// </summary>
    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        // 순환 위치 계산 (순환 주기는 lockableSkills 개수)
        int cycleIndex = Mathf.FloorToInt(lockTimer / skillChangeDuration) % lockableSkills.Count;

        // 해당 인덱스의 스킬 잠금
        LockSkill(lockableSkills[cycleIndex]);
    }

    /// <summary>
    /// 맥박 패턴 - 모든 스킬 잠금 후 해제를 반복
    /// </summary>
    private void ApplyPulsatingPattern()
    {
        // 짝수 주기에는 잠금, 홀수 주기에는 아무것도 잠그지 않음
        bool shouldLock = Mathf.FloorToInt(lockTimer / skillChangeDuration) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxSimultaneousLockedSkills);

            for (int i = 0; i < count; i++)
            {
                LockSkill(lockableSkills[i]);
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
        float progress = lockTimer / defaultLockDuration;
        int skillsToLock = Mathf.CeilToInt(progress * maxSimultaneousLockedSkills);
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxSimultaneousLockedSkills, lockableSkills.Count));

        // 복사본 생성 후 섞기
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // 계산된 수만큼 스킬 잠금
        for (int i = 0; i < skillsToLock; i++)
        {
            LockSkill(shuffledSkills[i]);
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
        int skillsToLock = UnityEngine.Random.Range(1, Mathf.Min(maxSimultaneousLockedSkills, shuffledSkills.Count) + 1);

        // 선택된 수만큼 스킬 잠금
        for (int i = 0; i < skillsToLock; i++)
        {
            LockSkill(shuffledSkills[i]);
        }
    }


    private void LockSkill(SkillType skillType)
    {
        if (targetObject == null) return;

        targetObject.SetSkillEnabled(skillType, false);
        currentlyLockedSkills.Add(skillType);
    }

 
    private void UnlockAllSkills()
    {
        if (targetObject == null) return;

        foreach (SkillType skill in lockableSkills)
        {
            targetObject.SetSkillEnabled(skill, true);
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

    /// <summary>
    /// 리스트 섞기 (Fisher-Yates 알고리즘)
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + UnityEngine.Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private void TriggerLockEvent(string eventType, List<SkillType> skills, float remainingTime)
    {
        OnSkillLockEvent?.Invoke(eventType, new List<SkillType>(skills), remainingTime);
    }
}
