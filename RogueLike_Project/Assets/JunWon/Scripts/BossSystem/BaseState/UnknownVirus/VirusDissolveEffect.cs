using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 다중 Material을 가진 오브젝트의 바이러스 디졸브 효과
/// </summary>
public class VirusDissolveEffect : MonoBehaviour
{
    [Header("Material 설정")]
    [SerializeField] private Material virusDissolveaterial;
    [SerializeField] private Material defaultMaterial;  // DefaultTile Material
    [SerializeField] private Renderer targetRenderer;

    [Header("연출 설정")]
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float preSpawnDelay = 0.5f;
    [SerializeField] private float postDissolveDelay = 0.3f;
    [SerializeField] private float preDissolveDelay = 0.3f;

    [Header("스케일 연출")]
    [SerializeField] private float spawnStartScale = 0.1f;
    [SerializeField] private float spawnOvershoot = 1.2f;
    [SerializeField] private float dissolveEndScale = 0.8f;

    // 셰이더 프로퍼티
    private const string DISSOLVE_AMOUNT = "_DissolveAmount";
    private const string EFFECT_TIME = "_EffectTime";
    private const string GLITCH_INTENSITY = "_GlitchIntensity";
    private const string EDGE_INTENSITY = "_EdgeIntensity";
    private const string MAIN_COLOR = "_MainColor";
    private const string EDGE_COLOR = "_EdgeColor";

    // 상태 관리
    private Vector3 originalScale;
    private Material[] originalMaterials;
    private Material[] dissolveMaterials;
    private Color originalMainColor;
    private Color originalEdgeColor;
    private bool isVisible = false;
    private bool isEffectRunning = false;

    void Awake()
    {
        originalScale = transform.localScale;

        // Renderer 자동 할당
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // 원본 Materials 백업
        if (targetRenderer != null)
        {
            originalMaterials = targetRenderer.materials;

            // 디졸브용 Materials 배열 생성
            dissolveMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i].name.Contains("VirusDissolve"))
                {
                    dissolveMaterials[i] = originalMaterials[i];
                    if (virusDissolveaterial == null)
                        virusDissolveaterial = originalMaterials[i];
                }
                else
                {
                    // DefaultTile 등 다른 Material은 VirusDissolve Material로 교체
                    dissolveMaterials[i] = virusDissolveaterial;
                    if (defaultMaterial == null)
                        defaultMaterial = originalMaterials[i];
                }
            }
        }

        BackupOriginalValues();
    }

    void Start()
    {
        // 시작할 때는 완전히 숨김
        SetToHiddenState();
    }

    private void BackupOriginalValues()
    {
        if (virusDissolveaterial == null) return;

        if (virusDissolveaterial.HasProperty(MAIN_COLOR))
            originalMainColor = virusDissolveaterial.GetColor(MAIN_COLOR);
        else
            originalMainColor = Color.white;

        if (virusDissolveaterial.HasProperty(EDGE_COLOR))
            originalEdgeColor = virusDissolveaterial.GetColor(EDGE_COLOR);
        else
            originalEdgeColor = Color.cyan;
    }

    /// <summary>완전히 숨김 상태</summary>
    public void SetToHiddenState()
    {
        if (targetRenderer != null)
            targetRenderer.enabled = false;

        transform.localScale = Vector3.zero;
        isVisible = false;
    }

    /// <summary>완전히 보임 상태 - 원본 Material 사용</summary>
    public void SetToVisibleState()
    {
        if (targetRenderer != null)
        {
            targetRenderer.enabled = true;
            targetRenderer.materials = originalMaterials;  // 원본 Materials 복구
        }

        transform.localScale = originalScale;
        isVisible = true;
    }

    /// <summary>디졸브 Material로 전환</summary>
    private void SetToDissolveMode()
    {
        if (targetRenderer != null)
        {
            targetRenderer.enabled = true;
            targetRenderer.materials = dissolveMaterials;  // 모든 Material을 디졸브로
        }
    }

    /// <summary>생성 연출 - 없던 것이 나타남</summary>
    public void StartSpawn(float duration = 2f)
    {
        if (isEffectRunning) return;
        StartCoroutine(SpawnSequence(duration));
    }

    /// <summary>소멸 연출 - 있던 것이 사라짐</summary>
    public void StartDissolve(float duration = 2f)
    {
        if (isEffectRunning || !isVisible) return;
        StartCoroutine(DissolveSequence(duration));
    }

    private IEnumerator SpawnSequence(float duration)
    {
        isEffectRunning = true;
        float effectStartTime = Time.time;

        // === 0단계: 완전히 숨김 상태에서 시작 ===
        SetToHiddenState();

        // === 1단계: 생성 예고 ===
        if (preSpawnDelay > 0)
        {
            yield return new WaitForSeconds(preSpawnDelay);
        }

        // === 2단계: 디졸브 모드로 전환 후 생성 연출 ===
        SetToDissolveMode();

        // 디졸브를 최대로 설정 (완전히 투명)
        SetMaterialFloat(DISSOLVE_AMOUNT, 1f);
        SetMaterialFloat(GLITCH_INTENSITY, 1f);
        SetMaterialFloat(EDGE_INTENSITY, 15f);
        SetMaterialColor(MAIN_COLOR, new Color(originalMainColor.r, originalMainColor.g, originalMainColor.b, 0f));
        SetMaterialColor(EDGE_COLOR, Color.cyan);

        float spawnTime = 0f;
        while (spawnTime < duration)
        {
            float progress = spawnTime / duration;
            float curveValue = spawnCurve.Evaluate(progress);
            float globalTime = Time.time - effectStartTime;

            // 디졸브 값 (1에서 0으로 - 투명에서 불투명으로)
            float dissolveAmount = Mathf.Lerp(1f, 0f, curveValue);

            // 글리치 강도 감소
            float glitchIntensity = Mathf.Lerp(1f, 0.1f, curveValue);

            // 엣지 강도 감소
            float edgeIntensity = Mathf.Lerp(15f, 3f, curveValue);

            // 색상 복구
            Color mainColor = Color.Lerp(
                new Color(originalMainColor.r, originalMainColor.g, originalMainColor.b, 0f),
                originalMainColor,
                curveValue
            );

            Color edgeColor = Color.Lerp(Color.cyan, originalEdgeColor, curveValue);

            // 스케일 애니메이션
            float scaleProgress = curveValue;
            float currentScale;
            if (scaleProgress < 0.7f)
            {
                currentScale = Mathf.Lerp(spawnStartScale, spawnOvershoot, scaleProgress / 0.7f);
            }
            else
            {
                currentScale = Mathf.Lerp(spawnOvershoot, 1f, (scaleProgress - 0.7f) / 0.3f);
            }

            transform.localScale = originalScale * currentScale;

            // Material 업데이트
            SetMaterialFloat(DISSOLVE_AMOUNT, dissolveAmount);
            SetMaterialFloat(EFFECT_TIME, globalTime);
            SetMaterialFloat(GLITCH_INTENSITY, glitchIntensity);
            SetMaterialFloat(EDGE_INTENSITY, edgeIntensity);
            SetMaterialColor(MAIN_COLOR, mainColor);
            SetMaterialColor(EDGE_COLOR, edgeColor);

            spawnTime += Time.deltaTime;
            yield return null;
        }

        // === 3단계: 완전히 정상 상태로 (원본 Material 복구) ===
        SetToVisibleState();

        isEffectRunning = false;
    }

    private IEnumerator DissolveSequence(float duration)
    {
        isEffectRunning = true;
        float t = 0f;

        // 0) “사라질 것”을 예고하는 딜레이·사운드
        if (preDissolveDelay > 0)
        {
            yield return new WaitForSeconds(preDissolveDelay);
        }

        SetToDissolveMode();

        // ─ 초기값 : Spawn 의 반대 ─
        SetMaterialFloat(DISSOLVE_AMOUNT, 0f);            // 완전 불투명
        SetMaterialFloat(GLITCH_INTENSITY, 0.1f);
        SetMaterialFloat(EDGE_INTENSITY, 3f);
        SetMaterialColor(MAIN_COLOR, originalMainColor);
        SetMaterialColor(EDGE_COLOR, originalEdgeColor);

        // 스케일 : 1 → overshootShrink(0.8) → 0.2
        float overshootShrink = 0.8f;

        while (t < duration)
        {
            float p = t / duration;                 // 0 → 1
            float cv = spawnCurve.Evaluate(p);       // Spawn 과 같은 커브

            // 1) 디졸브 (0→1)
            float dissolve = cv;
            float glitch = Mathf.Lerp(0.1f, 1f, cv);     // 같이 증가
            float edge = Mathf.Lerp(3f, 15f, cv);     // 같이 증가

            // 2) 알파는 60% 이후 급격히 감소
            float bodyFade = Mathf.InverseLerp(0.6f, 1f, p);
            Color main = originalMainColor;
            main.a = Mathf.Lerp(1f, 0f, bodyFade);

            Color edgeCol = Color.Lerp(originalEdgeColor, Color.magenta, cv * 0.7f);

            // 4) 셰이더 업데이트
            SetMaterialFloat(DISSOLVE_AMOUNT, dissolve);
            SetMaterialFloat(GLITCH_INTENSITY, glitch);
            SetMaterialFloat(EDGE_INTENSITY, edge);
            SetMaterialColor(MAIN_COLOR, main);
            SetMaterialColor(EDGE_COLOR, edgeCol);
            SetMaterialFloat(EFFECT_TIME, t);

            t += Time.deltaTime;
            yield return null;
        }

        // 3) 뒷정리
        if (postDissolveDelay > 0)
            yield return new WaitForSeconds(postDissolveDelay);

        SetToHiddenState();
        isEffectRunning = false;
    }


    /// <summary>즉시 토글</summary>
    public void ToggleVisibility()
    {
        if (isEffectRunning) return;

        if (isVisible)
            StartDissolve(1.5f);
        else
            StartSpawn(2f);
    }

    // Helper 메서드들
    private void SetMaterialFloat(string propertyName, float value)
    {
        if (virusDissolveaterial && virusDissolveaterial.HasProperty(propertyName))
            virusDissolveaterial.SetFloat(propertyName, value);
    }

    private void SetMaterialColor(string propertyName, Color value)
    {
        if (virusDissolveaterial && virusDissolveaterial.HasProperty(propertyName))
            virusDissolveaterial.SetColor(propertyName, value);
    }

    /// <summary>디버그 정보 출력</summary>
    [ContextMenu("Debug Materials")]
    public void DebugMaterials()
    {
        if (targetRenderer != null)
        {
            Debug.Log($"Original Materials: {originalMaterials?.Length}");
            Debug.Log($"Dissolve Materials: {dissolveMaterials?.Length}");
            Debug.Log($"Current Materials: {targetRenderer.materials?.Length}");

            for (int i = 0; i < targetRenderer.materials.Length; i++)
            {
                Debug.Log($"Material {i}: {targetRenderer.materials[i].name}");
            }
        }
    }

    /// <summary>테스트용</summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleVisibility();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            StartSpawn(2f);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            StartDissolve(2f);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            SetToHiddenState();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            SetToVisibleState();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            DebugMaterials();
        }
    }
}