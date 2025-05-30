using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalStripeDissolveEffect : MonoBehaviour
{
    [Header("디졸브 설정")]
    [SerializeField] private Material stripeDissolveShader; // 개선된 DigitalStripeDissolve 셰이더 참조
    [SerializeField] private float dissolveDuration = 5.0f; // 디졸브 효과 지속 시간

    [Header("스트라이프 효과 설정")]
    [SerializeField] private float stripeWidthStart = 10f;
    [SerializeField] private float stripeWidthEnd = 30f;
    [SerializeField] private float stripeSpeedStart = 2f;
    [SerializeField] private float stripeSpeedEnd = 4f;

    [Header("글리치 효과 설정")]
    [SerializeField] private float glitchDelayTime = 0.3f; // 글리치 효과 시작 지연 시간
    [SerializeField] private float glitchIntensityMax = 0.8f;

    [Header("디졸브 방향 설정")]
    [SerializeField] private Vector3 dissolveDirection = Vector3.up; // 기본 방향: 위쪽

    [Header("색상 설정")]
    [SerializeField] private Color dissolveColor = new Color(0f, 0.7f, 1f);

    [Header("사운드 효과")]
    [SerializeField] private AudioClip dissolveSound;
    [SerializeField] private AudioClip glitchSound;

    private AudioSource audioSource;
    [SerializeField] private List<Material> dissolveMaterials = new List<Material>();
    [SerializeField] private Dictionary<SkinnedMeshRenderer, Material[]> originalSkinnedMaterials = new Dictionary<SkinnedMeshRenderer, Material[]>();
    [SerializeField] private Dictionary<Renderer, Material[]> originalRendererMaterials = new Dictionary<Renderer, Material[]>();

    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] private Renderer[] regularRenderers;
    private MonsterBase monster;
    private int randomSeed;
    private Vector3 objectCenterPosition;
    private float objectHeight;

    // 몬스터 사망 효과 적용 정적 메서드
    public static void ApplyDeathEffect(MonsterBase targetMonster)
    {
        //Debug.Log("죽음 연출 적용 시작");
        // 이미 효과가 적용되었는지 확인
        if (targetMonster.GetComponent<DigitalStripeDissolveEffect>() == null)
            return;

        Debug.Log(" 계속 진행");

        // 효과 컴포넌트 추가
       DigitalStripeDissolveEffect effect = targetMonster.GetComponent<DigitalStripeDissolveEffect>();
        effect.Initialize();
        effect.StartDeathSequence();
    }

    private void Initialize()
    {
        // 오디오 소스 추가
        audioSource = gameObject.AddComponent<AudioSource>();

        // 랜덤 시드 생성 (모든 하위 객체에 동일한 랜덤 패턴 적용을 위함)
        randomSeed = Random.Range(1, 10000);

        // SkinnedMeshRenderer와 일반 Renderer 분리 수집
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // SkinnedMeshRenderer가 아닌 일반 Renderer들 수집
        List<Renderer> regularRenderersList = new List<Renderer>();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            if (!(renderer is SkinnedMeshRenderer))
            {
                regularRenderersList.Add(renderer);
            }
        }
        regularRenderers = regularRenderersList.ToArray();

        // 몬스터 오브젝트의 바운딩 박스 정보 구하기
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;

        // 모든 렌더러를 포함하는 바운딩 박스 계산
        foreach (Renderer renderer in allRenderers)
        {
            if (!boundsInitialized)
            {
                combinedBounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        // 오브젝트 중심점과 높이 계산
        objectCenterPosition = combinedBounds.center;
        objectHeight = combinedBounds.size.y;

        // 디졸브 셰이더 인스턴스화
        if (stripeDissolveShader == null)
        {
            stripeDissolveShader = new Material(Shader.Find("Custom/DigitalStripeDissolve"));
        }
        else
        {
        }

        // 몬스터 유형에 맞는 색상 설정
        AdjustColorForMonsterType();

        // SkinnedMeshRenderer 처리
        ProcessSkinnedMeshRenderers();

        // 일반 Renderer 처리
        ProcessRegularRenderers();
    }

    private void ProcessSkinnedMeshRenderers()
    {
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            // 원본 재질 저장
            Material[] originalMaterials = renderer.materials;
            originalSkinnedMaterials[renderer] = originalMaterials;

            // 새 재질 생성
            Material[] newMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material dissolveMat = new Material(stripeDissolveShader);

                // 원본 텍스처와 색상 복사
                if (originalMaterials[i].HasProperty("_MainTex"))
                {
                    dissolveMat.SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
                }

                if (originalMaterials[i].HasProperty("_Color"))
                {
                    dissolveMat.SetColor("_Color", originalMaterials[i].GetColor("_Color"));
                }

                // 공통 셰이더 속성 설정
                SetShaderCommonProperties(dissolveMat);

                // SkinnedMeshRenderer에 특화된 설정
                ConfigureShaderForSkinnedMesh(dissolveMat, renderer);

                newMaterials[i] = dissolveMat;
                dissolveMaterials.Add(dissolveMat);
            }

            // 이 시점에서는 재질을 변경하지 않고 저장만 함
        }
    }

    private void ProcessRegularRenderers()
    {
        foreach (Renderer renderer in regularRenderers)
        {
            // 원본 재질 저장
            Material[] originalMaterials = renderer.materials;
            originalRendererMaterials[renderer] = originalMaterials;

            // 새 재질 생성
            Material[] newMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material dissolveMat = new Material(stripeDissolveShader);

                // 원본 텍스처와 색상 복사
                if (originalMaterials[i].HasProperty("_MainTex"))
                {
                    dissolveMat.SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
                }

                if (originalMaterials[i].HasProperty("_Color"))
                {
                    dissolveMat.SetColor("_Color", originalMaterials[i].GetColor("_Color"));
                }

                // 공통 셰이더 속성 설정
                SetShaderCommonProperties(dissolveMat);

                // 일반 렌더러에 특화된 설정
                ConfigureShaderForRenderer(dissolveMat, renderer);

                newMaterials[i] = dissolveMat;
                dissolveMaterials.Add(dissolveMat);
            }

            // 이 시점에서는 재질을 변경하지 않고 저장만 함
        }
    }

    private void SetShaderCommonProperties(Material material)
    {
        // 기본 셰이더 속성 설정
        material.SetFloat("_DissolveAmount", 0f);
        material.SetFloat("_StripeWidth", stripeWidthStart);
        material.SetFloat("_StripeSpeed", stripeSpeedStart);
        material.SetFloat("_GlitchIntensity", 0.3f);
        material.SetFloat("_StripeIntensity", 0.7f);
        material.SetColor("_GlowColor", dissolveColor);

        // 추가된 속성 설정
        material.SetFloat("_RandomSeed", randomSeed); // 공통 랜덤 시드 설정
        material.SetFloat("_UseWorldCoords", 1f); // 월드 좌표 사용

        // 디졸브 방향 정규화하여 설정
        material.SetVector("_DissolveDirection", dissolveDirection.normalized);
    }

    private void ConfigureShaderForSkinnedMesh(Material material, SkinnedMeshRenderer renderer)
    {
        // SkinnedMeshRenderer의 상대적 위치에 따른 설정
        float relativeHeight = (renderer.bounds.center.y - (objectCenterPosition.y - objectHeight / 2)) / objectHeight;

        // 월드 Y 오프셋 설정 (모든 하위 객체가 동시에 디졸브되도록)
        material.SetFloat("_WorldYOffset", relativeHeight);
    }

    private void ConfigureShaderForRenderer(Material material, Renderer renderer)
    {
        // 일반 렌더러의 상대적 위치에 따른 설정
        float relativeHeight = (renderer.bounds.center.y - (objectCenterPosition.y - objectHeight / 2)) / objectHeight;

        // 월드 Y 오프셋 설정 (모든 하위 객체가 동시에 디졸브되도록)
        material.SetFloat("_WorldYOffset", relativeHeight);
    }

    private void AdjustColorForMonsterType()
    {

        dissolveColor = new Color(0.2f, 0.7f, 1f); // 파란색
        
    }

    // 디졸브 방향 설정 메서드 (외부에서 호출 가능)
    public void SetDissolveDirection(Vector3 direction)
    {
        dissolveDirection = direction.normalized;

        // 이미 생성된 재질들에도 적용
        foreach (Material mat in dissolveMaterials)
        {
            mat.SetVector("_DissolveDirection", dissolveDirection);
        }
    }

    // 디졸브 지속시간 설정 메서드 (외부에서 호출 가능)
    public void SetDissolveDuration(float duration)
    {
        if (duration > 0f)
        {
            dissolveDuration = duration;
        }
    }

    public void StartDeathSequence()
    {
        StartCoroutine(DissolveRoutine());

        // 사운드 효과 재생
        PlayDissolveSound();
    }


    private IEnumerator DissolveRoutine()
    {
        // 모든 SkinnedMeshRenderer에 디졸브 셰이더 적용
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            int materialCount = renderer.materials.Length;
            Material[] newMaterials = new Material[materialCount];

            for (int i = 0; i < materialCount; i++)
            {
                // dissolveMaterials 리스트에서 인덱스 계산
                int materialIndex = dissolveMaterials.FindIndex(m =>
                    m.GetTexture("_MainTex") == renderer.materials[i].GetTexture("_MainTex") &&
                    m.GetColor("_Color") == renderer.materials[i].GetColor("_Color"));

                if (materialIndex >= 0)
                {
                    newMaterials[i] = dissolveMaterials[materialIndex];
                }
                else
                {
                    // 찾지 못했을 경우 기본 디졸브 재질 사용
                    newMaterials[i] = new Material(stripeDissolveShader);
                    SetShaderCommonProperties(newMaterials[i]);
                    dissolveMaterials.Add(newMaterials[i]);
                }
            }

            renderer.materials = newMaterials;
        }

        // 모든 일반 Renderer에 디졸브 셰이더 적용
        foreach (Renderer renderer in regularRenderers)
        {
            int materialCount = renderer.materials.Length;
            Material[] newMaterials = new Material[materialCount];

            for (int i = 0; i < materialCount; i++)
            {
                // dissolveMaterials 리스트에서 인덱스 계산
                int materialIndex = 0;
                // 인덱스 찾기 로직...

                if (materialIndex < dissolveMaterials.Count)
                {
                    newMaterials[i] = dissolveMaterials[materialIndex];
                    materialIndex++;
                }
                else
                {
                    // 기본 디졸브 재질 사용
                    newMaterials[i] = new Material(stripeDissolveShader);
                    SetShaderCommonProperties(newMaterials[i]);
                    dissolveMaterials.Add(newMaterials[i]);
                }
            }

            renderer.materials = newMaterials;
        }

        float elapsed = 0f;
        bool glitchStarted = false;

        // 디졸브 애니메이션
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / dissolveDuration;

            // 디졸브 진행도 업데이트
            float dissolveAmount = Mathf.Lerp(0f, 1f, progress);

            // 스트라이프 효과 점진적 변화
            float stripeWidth = Mathf.Lerp(stripeWidthStart, stripeWidthEnd, progress);
            float stripeSpeed = Mathf.Lerp(stripeSpeedStart, stripeSpeedEnd, progress);

            // 글리치 효과는 일정 시간 후 시작
            float glitchIntensity = 0f;
            if (elapsed >= glitchDelayTime)
            {
                if (!glitchStarted)
                {
                    glitchStarted = true;
                    PlayGlitchSound();
                }

                // 글리치 강도 점진적 증가
                glitchIntensity = Mathf.Lerp(0f, glitchIntensityMax, (elapsed - glitchDelayTime) / (dissolveDuration - glitchDelayTime));

                // 디졸브 진행 중에 글리치 강도 맥박 효과
                if (progress > 0.3f && progress < 0.8f)
                {
                    float pulse = (Mathf.Sin(Time.time * 10f) * 0.3f + 0.7f);
                    glitchIntensity *= pulse;
                }
            }

            // 모든 디졸브 재질 동시에 업데이트
            foreach (Material mat in dissolveMaterials)
            {
                mat.SetFloat("_DissolveAmount", dissolveAmount);
                mat.SetFloat("_StripeWidth", stripeWidth);
                mat.SetFloat("_StripeSpeed", stripeSpeed);
                mat.SetFloat("_GlitchIntensity", glitchIntensity);
                mat.SetFloat("_StripeIntensity", 0.5f + progress * 0.5f);
            }

            yield return null;
        }

        // 효과 완료 후 오브젝트 제거
        Destroy(gameObject);
    }

    private void PlayDissolveSound()
    {
        if (audioSource != null && dissolveSound != null)
        {
            audioSource.clip = dissolveSound;
            audioSource.loop = true;
            audioSource.volume = 0.7f;
            audioSource.Play();
        }
    }

    private void PlayGlitchSound()
    {
        if (audioSource != null && glitchSound != null)
        {
            audioSource.PlayOneShot(glitchSound, 0.5f);
        }
    }

    private void OnDestroy()
    {
        // 모든 재질 정리
        foreach (Material mat in dissolveMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
    }
}