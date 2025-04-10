using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillLockUI : MonoBehaviour
{
    [Header("Lock UI Elements")]
    [SerializeField] private GameObject lockUIContainer;
    [SerializeField] private Transform lockIconsParent;
    [SerializeField] private GameObject lockIconPrefab;
    [SerializeField] private TextMeshProUGUI lockCountdownText;

    [Header("Animation Settings")]
    [SerializeField] private float appearDuration = 0.3f;
    [SerializeField] private float disappearDuration = 0.3f;
    [SerializeField] private AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float pulseFrequency = 1.5f;
    [SerializeField] private float pulseAmount = 0.2f;

    [Header("Glitch Effect")]
    [SerializeField] private CanvasGroup glitchOverlay;
    [SerializeField] private float glitchDuration = 0.5f;
    [SerializeField] private float glitchIntensity = 0.7f;

    private Dictionary<SkillType, GameObject> activeLockIcons = new Dictionary<SkillType, GameObject>();
    private Dictionary<SkillType, Sprite> skillIconSprites = new Dictionary<SkillType, Sprite>();
    private Coroutine countdownCoroutine;
    private float remainingLockTime;
    private bool isVisible = false;

    private void Awake()
    {
        if (lockUIContainer != null)
            lockUIContainer.SetActive(false);

        if (glitchOverlay != null)
            glitchOverlay.alpha = 0;

        LoadSkillIcons();
    }

    private void OnEnable()
    {
        Phase2_RansomLock_State.OnRansomLockEvent += HandleSkillLockEvent;
    }

    private void OnDisable()
    {
        Phase2_RansomLock_State.OnRansomLockEvent -= HandleSkillLockEvent;
    }

    private void LoadSkillIcons()
    {
        skillIconSprites[SkillType.Running] = Resources.Load<Sprite>("UI/Icons/RunningIcon");
        skillIconSprites[SkillType.Jumping] = Resources.Load<Sprite>("UI/Icons/JumpingIcon");
        skillIconSprites[SkillType.Dash] = Resources.Load<Sprite>("UI/Icons/DashIcon");
        skillIconSprites[SkillType.Movement] = Resources.Load<Sprite>("UI/Icons/MovementIcon");
        skillIconSprites[SkillType.Shooting] = Resources.Load<Sprite>("UI/Icons/ShootingIcon");
        skillIconSprites[SkillType.WeaponSwitch] = Resources.Load<Sprite>("UI/Icons/WeaponSwitchIcon");
        skillIconSprites[SkillType.Interaction] = Resources.Load<Sprite>("UI/Icons/InteractionIcon");
    }

    public void HandleSkillLockEvent(string eventType, List<SkillType> lockedSkills, float remainingTime)
    {
        switch (eventType)
        {
            case "LockStarted":
                ShowLockUI(lockedSkills, remainingTime);
                PlayGlitchEffect();
                break;

            case "SkillsChanged":
                UpdateLockedSkills(lockedSkills, remainingTime);
                PlayGlitchEffect();
                break;

            case "LockEnded":
                HideLockUI();
                break;
        }
    }

    public void ShowLockUI(List<SkillType> lockedSkills, float duration)
    {
        if (lockUIContainer == null) return;

        // Clear any existing lock icons
        ClearLockIcons();

        // Create new lock icons for each locked skill
        foreach (SkillType skill in lockedSkills)
        {
            CreateLockIcon(skill);
        }

        // Set the countdown timer
        remainingLockTime = duration;

        // Stop any existing countdown coroutine
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        // Start a new countdown coroutine
        countdownCoroutine = StartCoroutine(CountdownRoutine(duration));

        // Show the lock UI with animation
        if (!isVisible)
        {
            isVisible = true;
            lockUIContainer.SetActive(true);
            StartCoroutine(AnimateContainer(true));
        }
    }

    public void UpdateLockedSkills(List<SkillType> lockedSkills, float remainingTime)
    {
        // Clear existing icons
        ClearLockIcons();

        // Create new icons for the updated list
        foreach (SkillType skill in lockedSkills)
        {
            CreateLockIcon(skill);
        }

        // Update the countdown timer
        remainingLockTime = remainingTime;

        // Play a quick pulse animation on the container
        StartCoroutine(PulseContainer());
    }

    public void HideLockUI()
    {
        if (!isVisible) return;

        // Hide with animation
        StartCoroutine(AnimateContainer(false));

        // Stop the countdown
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // Clear any lock icons
        ClearLockIcons();

        isVisible = false;
    }

    private void CreateLockIcon(SkillType skill)
    {
        if (lockIconPrefab == null || lockIconsParent == null) return;

        // Create a new lock icon
        GameObject newIcon = Instantiate(lockIconPrefab, lockIconsParent);

        // Set the skill icon if we have one
        if (skillIconSprites.TryGetValue(skill, out Sprite skillSprite))
        {
            Image skillIconImage = newIcon.transform.Find("SkillIcon")?.GetComponent<Image>();
            if (skillIconImage != null)
                skillIconImage.sprite = skillSprite;
        }

        // Set the skill name text
        TextMeshProUGUI skillNameText = newIcon.transform.Find("SkillName")?.GetComponent<TextMeshProUGUI>();
        if (skillNameText != null)
            skillNameText.text = skill.ToString();

        // Store the icon in our dictionary
        activeLockIcons[skill] = newIcon;

        // Start a pulse animation on the new icon
        StartCoroutine(PulseIcon(newIcon.transform));
    }

    private void ClearLockIcons()
    {
        foreach (GameObject icon in activeLockIcons.Values)
        {
            Destroy(icon);
        }

        activeLockIcons.Clear();
    }

    public void PlayGlitchEffect()
    {
        if (glitchOverlay == null) return;

        StartCoroutine(GlitchEffectRoutine());
    }

    private IEnumerator CountdownRoutine(float duration)
    {
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            // Update the remaining time
            remainingLockTime = duration - (Time.time - startTime);

            // Update the countdown text
            if (lockCountdownText != null)
            {
                lockCountdownText.text = remainingLockTime.ToString("F1") + "s";
            }

            yield return null;
        }

        // Ensure the countdown text shows zero at the end
        if (lockCountdownText != null)
        {
            lockCountdownText.text = "0.0s";
        }
    }

    private IEnumerator AnimateContainer(bool appearing)
    {
        float duration = appearing ? appearDuration : disappearDuration;
        float startTime = Time.time;
        CanvasGroup canvasGroup = lockUIContainer.GetComponent<CanvasGroup>();

        // Make sure we have a CanvasGroup
        if (canvasGroup == null)
        {
            canvasGroup = lockUIContainer.AddComponent<CanvasGroup>();
        }

        // Set initial state
        canvasGroup.alpha = appearing ? 0 : 1;

        // Animate
        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float curveValue = appearCurve.Evaluate(appearing ? t : 1 - t);

            canvasGroup.alpha = curveValue;

            yield return null;
        }

        // Set final state
        canvasGroup.alpha = appearing ? 1 : 0;

        // If disappearing, deactivate the container
        if (!appearing)
        {
            lockUIContainer.SetActive(false);
        }
    }

    private IEnumerator PulseContainer()
    {
        RectTransform rect = lockUIContainer.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

        // Quick grow
        float growDuration = 0.1f;
        float startTime = Time.time;

        while (Time.time - startTime < growDuration)
        {
            float t = (Time.time - startTime) / growDuration;
            float scale = 1 + (pulseAmount * appearCurve.Evaluate(t));

            rect.localScale = originalScale * scale;

            yield return null;
        }

        // Quick shrink back
        startTime = Time.time;

        while (Time.time - startTime < growDuration)
        {
            float t = (Time.time - startTime) / growDuration;
            float scale = 1 + (pulseAmount * (1 - appearCurve.Evaluate(t)));

            rect.localScale = originalScale * scale;

            yield return null;
        }

        // Reset to original scale
        rect.localScale = originalScale;
    }

    private IEnumerator PulseIcon(Transform iconTransform)
    {
        Vector3 originalScale = iconTransform.localScale;

        while (iconTransform != null && iconTransform.gameObject.activeInHierarchy)
        {
            // Calculate the pulse based on sin wave
            float pulse = 1 + (Mathf.Sin(Time.time * pulseFrequency) * pulseAmount * 0.5f);

            // Apply scale
            iconTransform.localScale = originalScale * pulse;

            yield return null;
        }
    }

    private IEnumerator GlitchEffectRoutine()
    {
        float startTime = Time.time;

        // Fade in
        while (Time.time - startTime < glitchDuration * 0.3f)
        {
            float t = (Time.time - startTime) / (glitchDuration * 0.3f);
            glitchOverlay.alpha = glitchIntensity * t;
            yield return null;
        }

        // Hold
        glitchOverlay.alpha = glitchIntensity;
        yield return new WaitForSeconds(glitchDuration * 0.4f);

        // Fade out
        startTime = Time.time;
        while (Time.time - startTime < glitchDuration * 0.3f)
        {
            float t = (Time.time - startTime) / (glitchDuration * 0.3f);
            glitchOverlay.alpha = glitchIntensity * (1 - t);
            yield return null;
        }

        // Ensure it's fully faded out
        glitchOverlay.alpha = 0;
    }
}