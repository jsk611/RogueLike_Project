using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LockIconController : MonoBehaviour
{
    [SerializeField] private Image lockImage;
    [SerializeField] private Image skillIconImage;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Image backgroundImage;

    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 40f;
    [SerializeField] private float glitchInterval = 0.5f;
    [SerializeField] private float glitchDuration = 0.1f;

    private Color originalLockColor;
    private Color originalBackgroundColor;
    private Coroutine glitchCoroutine;

    private void Awake()
    {
        if (lockImage != null)
            originalLockColor = lockImage.color;

        if (backgroundImage != null)
            originalBackgroundColor = backgroundImage.color;

        // Start the glitch effect coroutine
        glitchCoroutine = StartCoroutine(GlitchRoutine());
    }

    private void Update()
    {
        // Rotate the lock icon
        if (lockImage != null)
        {
            lockImage.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (glitchCoroutine != null)
            StopCoroutine(glitchCoroutine);
    }

    public void SetSkillIcon(Sprite icon)
    {
        if (skillIconImage != null && icon != null)
        {
            skillIconImage.sprite = icon;
            skillIconImage.enabled = true;
        }
    }

    public void SetSkillName(string name)
    {
        if (skillNameText != null)
        {
            skillNameText.text = name;
        }
    }

    private IEnumerator GlitchRoutine()
    {
        while (true)
        {
            // Wait for random interval
            yield return new WaitForSeconds(Random.Range(glitchInterval * 0.7f, glitchInterval * 1.3f));

            // Apply glitch effect
            yield return StartCoroutine(ApplyGlitchEffect());
        }
    }

    private IEnumerator ApplyGlitchEffect()
    {
        // Randomly decide which elements to glitch
        bool glitchLock = Random.value > 0.3f;
        bool glitchBackground = Random.value > 0.6f;

        // Apply glitch color to selected elements
        if (glitchLock && lockImage != null)
        {
            lockImage.color = new Color(
                originalLockColor.r + Random.Range(-0.3f, 0.3f),
                originalLockColor.g + Random.Range(-0.3f, 0.3f),
                originalLockColor.b + Random.Range(-0.3f, 0.3f),
                originalLockColor.a
            );
        }

        if (glitchBackground && backgroundImage != null)
        {
            backgroundImage.color = new Color(
                originalBackgroundColor.r + Random.Range(-0.2f, 0.2f),
                originalBackgroundColor.g + Random.Range(-0.2f, 0.2f),
                originalBackgroundColor.b + Random.Range(-0.2f, 0.2f),
                originalBackgroundColor.a
            );
        }

        // Apply position glitch
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 originalPosition = rect.anchoredPosition;

        if (Random.value > 0.5f)
        {
            rect.anchoredPosition = new Vector2(
                originalPosition.x + Random.Range(-5f, 5f),
                originalPosition.y + Random.Range(-5f, 5f)
            );
        }

        // Wait for glitch duration
        yield return new WaitForSeconds(glitchDuration);

        // Restore original values
        if (glitchLock && lockImage != null)
            lockImage.color = originalLockColor;

        if (glitchBackground && backgroundImage != null)
            backgroundImage.color = originalBackgroundColor;

        rect.anchoredPosition = originalPosition;
    }
}