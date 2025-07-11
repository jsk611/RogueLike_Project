using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



/// <summary>
/// UnknownVirus 몬스터들의 멋진 디지털 디졸브 사라짐 효과
/// </summary>
public class VirusDissolveEffect : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public Material dissolveMaterial;           // 디졸브 머티리얼
    public float colorTransitionTime = 1.5f;    // 색상 변화 시간
    public float dissolveTime = 3f;             // 디졸브 시간  
    public AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Cybernetic Color Transition")]
    public Color cyberColor1 = new Color(0f, 1f, 1f, 1f);    // 사이안
    public Color cyberColor2 = new Color(1f, 0f, 1f, 1f);    // 마젠타  
    public Color cyberColor3 = new Color(0f, 0.5f, 1f, 1f);  // 전기 블루
    public bool enableColorPulse = true;        // 색상 펄스 효과
    public float pulseSpeed = 3f;               // 펄스 속도
    
    [Header("Material Blending")]
    public bool useMaterialBlending = true;     // 머티리얼 블렌딩 사용
    public float materialBlendSpeed = 2f;       // 블렌딩 속도
    
    [Header("Digital Effects")]
    public bool enableGlitchEffect = true;      // 글리치 효과
    public bool enableParticleEffect = true;    // 파티클 효과
    public bool enableLightEffect = true;       // 라이트 효과
    
    [Header("Audio")]
    public AudioClip dissolveSound;             // 디졸브 사운드
    
    // 내부 변수들
    [SerializeField] private Renderer[] originalRenderers;
    [SerializeField] private Material[] originalMaterials;
    private bool isDissolving = false;
    
    // 효과 컴포넌트들
    private ParticleSystem dissolveParticles;
    private Light dissolveLight;
    private AudioSource audioSource;

    void Start()
    {
        // 원본 렌더러와 머티리얼 저장
        CacheOriginalMaterials();
        
        // 효과 컴포넌트들 초기화
        InitializeEffectComponents();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartDissolve();
        }
    }


    /// <summary>
    /// 원본 머티리얼들 캐싱
    /// </summary>
    private void CacheOriginalMaterials()
    {
        originalRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[originalRenderers.Length];
        
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            originalMaterials[i] = originalRenderers[i].material;
        }
        
        // 🎯 몬스터의 기존 텍스처 정보 로그
        Debug.Log($"[VirusDissolveEffect] {gameObject.name} - 기존 렌더러 텍스처 자동 감지 (총 {originalRenderers.Length}개)");
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                Texture mainTex = originalMaterials[i].GetTexture("_MainTex");
                if (mainTex != null)
                {
                    Debug.Log($"  └ 렌더러 {i}: {mainTex.name}");
                }
                else
                {
                    Debug.Log($"  └ 렌더러 {i}: 텍스처 없음");
                }
            }
        }
    }
    
    /// <summary>
    /// 효과 컴포넌트들 초기화
    /// </summary>
    private void InitializeEffectComponents()
    {
        // AudioSource 생성
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D 사운드
        
        // 디졸브 파티클 생성
        if (enableParticleEffect)
        {
            CreateDissolveParticles();
        }
        
        // 디졸브 라이트 생성
        if (enableLightEffect)
        {
            CreateDissolveLight();
        }
    }
    
    /// <summary>
    /// 디졸브 파티클 시스템 생성
    /// </summary>
    private void CreateDissolveParticles()
    {
        GameObject particleGO = new GameObject("DissolveParticles");
        particleGO.transform.SetParent(transform);
        particleGO.transform.localPosition = Vector3.zero;
        
        dissolveParticles = particleGO.AddComponent<ParticleSystem>();
        
        var main = dissolveParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 3f;
        main.startSize = 0.1f;
        main.startColor = Color.cyan;
        main.maxParticles = 100;
        
        var emission = dissolveParticles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 50)
        });
        
        var shape = dissolveParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = Vector3.one;
        
        var velocityOverLifetime = dissolveParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        var colorOverLifetime = dissolveParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.cyan, 0f), 
                new GradientColorKey(Color.blue, 0.5f),
                new GradientColorKey(Color.black, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
    }
    
    /// <summary>
    /// 디졸브 라이트 생성
    /// </summary>
    private void CreateDissolveLight()
    {
        GameObject lightGO = new GameObject("DissolveLight");
        lightGO.transform.SetParent(transform);
        lightGO.transform.localPosition = Vector3.zero;
        
        dissolveLight = lightGO.AddComponent<Light>();
        dissolveLight.type = LightType.Point;
        dissolveLight.color = Color.cyan;
        dissolveLight.intensity = 0f;
        dissolveLight.range = 10f;
    }
    
    /// <summary>
    /// 디졸브 효과 시작 (메인 함수)
    /// </summary>
    public void StartDissolve()
    {
        if (isDissolving) return;
        
        Debug.Log($"[VirusDissolveEffect] {gameObject.name} 디졸브 시작");
        StartCoroutine(DissolveSequence());
    }
    
    /// <summary>
    /// 완전한 디졸브 시퀀스 (자연스러운 색상 변화 포함)
    /// </summary>
    private IEnumerator DissolveSequence()
    {
        isDissolving = true;
        
        // 🎨 1단계: 원본 머티리얼 색상을 사이버틱하게 변화
        yield return StartCoroutine(CyberneticColorTransition());
        
        // 🔧 2단계: 디졸브 머티리얼 적용
        ApplyDissolveMaterials();
        
        // 🔊 3단계: 사운드 재생
        if (dissolveSound != null && audioSource != null)
        {
            audioSource.clip = dissolveSound;
            audioSource.Play();
        }
        
        // 💥 5단계: 메인 디졸브 애니메이션
        yield return StartCoroutine(AnimateDissolve());
        
        // ✅ 6단계: 완료 후 정리
        OnDissolveComplete();
    }
    
    /// <summary>
    /// 사이버틱 머티리얼 변화 (기존 머티리얼과 디졸브 머티리얼 블렌딩)
    /// </summary>
    private IEnumerator CyberneticColorTransition()
    {
        Debug.Log($"[VirusDissolveEffect] {gameObject.name} 사이버틱 머티리얼 블렌딩 시작");
        
        if (!useMaterialBlending || dissolveMaterial == null)
        {
            Debug.Log("[VirusDissolveEffect] 머티리얼 블렌딩이 비활성화되었거나 디졸브 머티리얼이 없습니다.");
            yield break;
        }
        
        // 🎨 각 렌더러에 대해 블렌딩된 머티리얼 인스턴스 생성
        Material[] blendedMaterials = new Material[originalRenderers.Length];
        
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] != null && originalMaterials[i] != null)
            {
                // 블렌딩용 머티리얼 인스턴스 생성 (원본 기반)
                blendedMaterials[i] = new Material(originalMaterials[i]);
            }
        }
        
        float elapsed = 0f;
        
        while (elapsed < colorTransitionTime)
        {
            float progress = elapsed / colorTransitionTime;
            float blendAmount = Mathf.SmoothStep(0f, 1f, progress); // 부드러운 블렌딩
            
            // 🔄 모든 렌더러에 머티리얼 블렌딩 적용
            for (int i = 0; i < originalRenderers.Length; i++)
            {
                if (originalRenderers[i] != null && originalMaterials[i] != null && blendedMaterials[i] != null)
                {
                    // Material.Lerp로 두 머티리얼 블렌딩
                    blendedMaterials[i].Lerp(originalMaterials[i], dissolveMaterial, blendAmount);
                    
                    // 🎯 기존 텍스처는 보존 (덮어쓰지 않음)
                    Texture originalTexture = originalMaterials[i].GetTexture("_MainTex");
                    if (originalTexture != null)
                    {
                        blendedMaterials[i].SetTexture("_MainTex", originalTexture);
                        blendedMaterials[i].SetTextureScale("_MainTex", originalMaterials[i].GetTextureScale("_MainTex"));
                        blendedMaterials[i].SetTextureOffset("_MainTex", originalMaterials[i].GetTextureOffset("_MainTex"));
                    }
                    
                    // 사이버틱 색상 오버레이 (추가 효과)
                    Color currentColor = Color.white;
                    if (blendedMaterials[i].HasProperty("_Color"))
                        currentColor = blendedMaterials[i].GetColor("_Color");
                    else if (blendedMaterials[i].HasProperty("_MainColor"))
                        currentColor = blendedMaterials[i].GetColor("_MainColor");
                    
                    // 진행도에 따른 사이버틱 색상 틴트
                    Color cyberTint;
                    if (progress < 0.5f)
                        cyberTint = Color.Lerp(Color.white, cyberColor1, progress * 2f);
                    else
                        cyberTint = Color.Lerp(cyberColor1, cyberColor3, (progress - 0.5f) * 2f);
                    
                    // 펄스 효과
                    if (enableColorPulse)
                    {
                        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                        cyberTint = Color.Lerp(cyberTint, cyberTint * 1.3f, pulse * 0.2f);
                    }
                    
                    // 최종 색상 적용 (원본 색상 + 사이버틱 틴트)
                    Color finalColor = Color.Lerp(currentColor, currentColor * cyberTint, blendAmount * 0.6f);
                    
                    if (blendedMaterials[i].HasProperty("_Color"))
                        blendedMaterials[i].SetColor("_Color", finalColor);
                    else if (blendedMaterials[i].HasProperty("_MainColor"))
                        blendedMaterials[i].SetColor("_MainColor", finalColor);
                    
                    // 블렌딩된 머티리얼 적용
                    originalRenderers[i].material = blendedMaterials[i];
                }
            }
            
            // 라이트 효과
            if (enableLightEffect && dissolveLight != null)
            {
                Color lightColor = Color.Lerp(Color.white, cyberColor2, blendAmount);
                dissolveLight.color = lightColor;
                dissolveLight.intensity = blendAmount * 2f;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log($"[VirusDissolveEffect] {gameObject.name} 사이버틱 머티리얼 블렌딩 완료");
    }
    
    /// <summary>
    /// 디졸브 머티리얼들 적용 (기존 텍스처와 설정 보존)
    /// </summary>
    private void ApplyDissolveMaterials()
    {
        if (dissolveMaterial == null) return;
        
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] != null && originalMaterials[i] != null)
            {
                // 새로운 머티리얼 인스턴스 생성
                Material newMaterial = new Material(dissolveMaterial);
                
                // 🎯 현재 몬스터에 적용된 머티리얼의 텍스처 보존
                Material originalMat = originalMaterials[i];
                
                // 🎨 모든 텍스처 프로퍼티 자동 보존
                string[] textureProperties = { "_MainTex", "_BumpMap", "_EmissionMap", "_MetallicGlossMap", "_OcclusionMap" };
                
                foreach (string texProp in textureProperties)
                {
                    if (originalMat.HasProperty(texProp))
                    {
                        Texture tex = originalMat.GetTexture(texProp);
                        if (tex != null && newMaterial.HasProperty(texProp))
                        {
                            newMaterial.SetTexture(texProp, tex);
                            newMaterial.SetTextureScale(texProp, originalMat.GetTextureScale(texProp));
                            newMaterial.SetTextureOffset(texProp, originalMat.GetTextureOffset(texProp));
                        }
                    }
                }
                
                // 메인 텍스처 정보 (로그용)
                Texture mainTex = originalMat.GetTexture("_MainTex");
                
                // 현재 색상 보존 (사이버틱 변화 후의 색상)
                Color currentColor = Color.white;
                if (originalRenderers[i].material.HasProperty("_Color"))
                {
                    currentColor = originalRenderers[i].material.GetColor("_Color");
                }
                else if (originalRenderers[i].material.HasProperty("_MainColor"))
                {
                    currentColor = originalRenderers[i].material.GetColor("_MainColor");
                }
                else if (originalMat.HasProperty("_Color"))
                {
                    currentColor = originalMat.GetColor("_Color");
                }
                else if (originalMat.HasProperty("_MainColor"))
                {
                    currentColor = originalMat.GetColor("_MainColor");
                }
                
                newMaterial.SetColor("_MainColor", currentColor);
                
                // 🌟 디졸브 설정 초기화
                newMaterial.SetFloat("_DissolveAmount", 0f);
                
                // 바이러스 테마에 맞는 기본 설정 적용
                if (newMaterial.HasProperty("_EdgeColor"))
                {
                    newMaterial.SetColor("_EdgeColor", new Color(0f, 1f, 1f, 1f)); // 사이안
                }
                if (newMaterial.HasProperty("_EdgeIntensity"))
                {
                    newMaterial.SetFloat("_EdgeIntensity", 3f);
                }
                if (newMaterial.HasProperty("_GlitchIntensity"))
                {
                    newMaterial.SetFloat("_GlitchIntensity", 0.1f);
                }
                
                // 📦 노이즈 텍스처 설정 (기본 흰색 노이즈 사용)
                if (newMaterial.HasProperty("_DissolveTexture") && newMaterial.GetTexture("_DissolveTexture") == null)
                {
                    // 기본 노이즈 텍스처가 없으면 흰색 텍스처 사용 (자체 노이즈 생성)
                    newMaterial.SetTexture("_DissolveTexture", Texture2D.whiteTexture);
                }
                
                // 머티리얼 적용
                originalRenderers[i].material = newMaterial;
                
                string textureName = mainTex != null ? mainTex.name : "없음";
                Debug.Log($"[VirusDissolveEffect] {originalRenderers[i].gameObject.name}의 머티리얼 교체 완료 - 기존 텍스처: {textureName}");
            }
        }
    }
    
    /// <summary>
    /// 메인 디졸브 애니메이션
    /// </summary>
    private IEnumerator AnimateDissolve()
    {
        float elapsed = 0f;
        
        // 파티클 효과 시작
        if (enableParticleEffect && dissolveParticles != null)
        {
            dissolveParticles.Play();
        }
        
        while (elapsed < dissolveTime)
        {
            float progress = elapsed / dissolveTime;
            float curveValue = dissolveCurve.Evaluate(progress);
            
            // 모든 머티리얼의 디졸브 값 업데이트
            UpdateDissolveValue(curveValue);
            
            // 라이트 효과 업데이트
            if (enableLightEffect && dissolveLight != null)
            {
                // 디졸브 진행에 따라 라이트 강도 변화
                float lightIntensity = Mathf.Sin(progress * Mathf.PI) * 3f;
                dissolveLight.intensity = lightIntensity;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 최종 디졸브 완료
        UpdateDissolveValue(1f);
        
        // 라이트 페이드 아웃
        if (dissolveLight != null)
        {
            dissolveLight.DOIntensity(0f, 0.5f);
        }
    }
    
    /// <summary>
    /// 디졸브 값 업데이트
    /// </summary>
    private void UpdateDissolveValue(float dissolveAmount)
    {
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] != null && originalRenderers[i].material != null)
            {
                originalRenderers[i].material.SetFloat("_DissolveAmount", dissolveAmount);
            }
        }
    }
    
    /// <summary>
    /// 디졸브 완료 후 처리
    /// </summary>
    private void OnDissolveComplete()
    {
        Debug.Log($"[VirusDissolveEffect] {gameObject.name} 디졸브 완료");
        
        // 렌더러 비활성화 (완전히 사라짐)
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] != null)
            {
                originalRenderers[i].enabled = false;
            }
        }
        
        // 콜라이더 비활성화
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // 이벤트 발생 (필요시)
        SendMessage("OnDissolveFinished", SendMessageOptions.DontRequireReceiver);
    }
    
    /// <summary>
    /// 디졸브 리셋 (다시 나타나게)
    /// </summary>
    public void ResetDissolve()
    {
        isDissolving = false;
        
        // 원본 머티리얼로 복구
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            if (originalRenderers[i] != null)
            {
                originalRenderers[i].material = originalMaterials[i];
                originalRenderers[i].enabled = true;
            }
        }
        
        // 콜라이더 복구
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
        
        // 라이트 끄기
        if (dissolveLight != null)
        {
            dissolveLight.intensity = 0f;
        }
        
        Debug.Log($"[VirusDissolveEffect] {gameObject.name} 디졸브 리셋 완료 - 기존 머티리얼로 복구");
    }
    
} 