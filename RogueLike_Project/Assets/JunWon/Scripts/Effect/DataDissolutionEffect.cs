using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Renderer))]
public class DataDissolutionEffect : MonoBehaviour
{
    // 분해 효과 설정
    [Header("분해 효과 설정")]
    [SerializeField] private float dissolutionDuration = 1.0f;  // 분해 진행 시간
    [SerializeField] private float glitchIncreaseDuration = 0.4f; // 글리치 증가 시간
    [SerializeField] private float delayBeforeDissolve = 0.2f;  // 분해 시작 전 지연 시간
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float yOffset = 1.0f;             // 분해 후 Y축 이동
    [SerializeField] private Vector3 scatterForce = new Vector3(1f, 2f, 1f); // 분해 시 파편 흩어짐 힘
    [SerializeField] private Vector3 voxelRotationSpeed = new Vector3(1f, 1f, 1f); // 복셀 회전 속도

    // 시각 효과 설정
    [Header("시각 효과 설정")]
    [SerializeField] private Color glowColor = new Color(0f, 0.7f, 1f); // 발광 색상
    [SerializeField] private float emissionStrength = 3.0f;    // 발광 강도
    [SerializeField] private float finalVoxelSize = 0.05f;     // 최종 복셀 크기
    [SerializeField] private float voxelHollowAmount = 0.3f;   // 복셀 중공화 정도

    // 효과 컴포넌트
    [Header("추가 효과")]
    [SerializeField] private GameObject glitchParticlePrefab;  // 글리치 파티클 이펙트
    [SerializeField] private AudioClip dissolutionSound;        // 분해 사운드
    [SerializeField] private AudioClip glitchSound;             // 글리치 사운드

    // 내부 변수
    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private List<Material> originalMaterials = new List<Material>();
    private List<Material> dissolveMaterials = new List<Material>();
    private float dissolveAmount = 0f;
    private float glitchIntensity = 0f;
    private bool isDissolving = false;
    private Coroutine dissolutionCoroutine;
    private Rigidbody rigidBody;
    private Collider mainCollider;
    private NavMeshAgent navAgent;

    // 콜백 이벤트
    public System.Action OnDissolutionStarted;
    public System.Action OnDissolutionCompleted;

    private void Awake()
    {
        // 필요한 컴포넌트 참조 가져오기
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        rigidBody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        navAgent = GetComponent<NavMeshAgent>();

        // 원본 머티리얼 저장
        originalMaterials.AddRange(rend.materials);

        // 분해 셰이더 머티리얼 생성
        CreateDissolveMaterials();
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 자원 정리
        foreach (var mat in dissolveMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
    }

    // 분해 셰이더용 머티리얼 생성
    private void CreateDissolveMaterials()
    {
        Shader dissolveShader = Shader.Find("Custom/DataDissolution");
        if (dissolveShader == null)
        {
            Debug.LogError("DataDissolution 셰이더를 찾을 수 없습니다. 셰이더가 프로젝트에 추가되었는지 확인하세요.");
            return;
        }

        // 각 원본 머티리얼마다 분해 머티리얼 생성
        foreach (var originalMat in originalMaterials)
        {
            Material dissolveMat = new Material(dissolveShader);

            // 원본 머티리얼에서 텍스처와 색상 복사
            if (originalMat.HasProperty("_MainTex"))
            {
                dissolveMat.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));
            }

            if (originalMat.HasProperty("_Color"))
            {
                dissolveMat.SetColor("_Color", originalMat.GetColor("_Color"));
            }

            // 노이즈 텍스처 설정 (없으면 기본 노이즈 텍스처 사용)
            Texture2D noiseTex = Resources.Load<Texture2D>("Textures/NoiseTexture");
            if (noiseTex != null)
            {
                dissolveMat.SetTexture("_NoiseTex", noiseTex);
            }

            // 기본 속성 설정
            dissolveMat.SetColor("_GlowColor", glowColor);
            dissolveMat.SetFloat("_DissolveAmount", 0f);
            dissolveMat.SetFloat("_DissolveScale", 1f);
            dissolveMat.SetFloat("_GlitchIntensity", 0f);
            dissolveMat.SetFloat("_VoxelSize", 0.01f);
            dissolveMat.SetFloat("_VoxelHollow", 0f);
            dissolveMat.SetFloat("_EmissionStrength", emissionStrength);

            dissolveMaterials.Add(dissolveMat);
        }
    }

    // 분해 효과 시작
    public void StartDissolve()
    {
        if (isDissolving) return;

        // 원래 실행 중인 Coroutine이 있으면 중지
        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
        }

        dissolutionCoroutine = StartCoroutine(DissolutionRoutine());
    }

    // 분해 효과 중지 (및 원상 복구)
    public void StopDissolve(bool resetToOriginal = true)
    {
        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
            dissolutionCoroutine = null;
        }

        isDissolving = false;

        // 원래 머티리얼로 복원
        if (resetToOriginal)
        {
            rend.materials = originalMaterials.ToArray();
            dissolveAmount = 0f;
            glitchIntensity = 0f;
        }
    }

    // 분해 과정 코루틴
    private IEnumerator DissolutionRoutine()
    {
        isDissolving = true;
        OnDissolutionStarted?.Invoke();

        // 물리 처리 비활성화
        DisablePhysics();

        // 분해 셰이더 머티리얼로 변경
        rend.materials = dissolveMaterials.ToArray();

        // 지연 시간
        if (delayBeforeDissolve > 0)
        {
            yield return new WaitForSeconds(delayBeforeDissolve);
        }

        // 글리치 효과 증가
        float glitchTimer = 0f;
        while (glitchTimer < glitchIncreaseDuration)
        {
            glitchTimer += Time.deltaTime;
            glitchIntensity = Mathf.Lerp(0f, 0.5f, glitchTimer / glitchIncreaseDuration);

            // 셰이더 속성 업데이트
            UpdateShaderProperties();

            yield return null;
        }

        // 글리치 사운드 재생
        PlayGlitchSound();

        // 글리치 파티클 효과 생성
        SpawnGlitchParticles();

        // 분해 효과 실행
        float timer = 0f;
        while (timer < dissolutionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / dissolutionDuration;

            // 애니메이션 커브 적용
            dissolveAmount = dissolveCurve.Evaluate(t);

            // 글리치 효과는 분해가 진행됨에 따라 증가
            glitchIntensity = Mathf.Lerp(0.5f, 1.0f, t);

            // 복셀 크기는 점점 커짐
            float currentVoxelSize = Mathf.Lerp(0.01f, finalVoxelSize, t);

            // 복셀 중공화 정도 증가
            float currentHollow = Mathf.Lerp(0f, voxelHollowAmount, t);

            // 셰이더 속성 업데이트
            UpdateShaderProperties(currentVoxelSize, currentHollow);

            // 오브젝트 회전 (복셀 파편 회전 효과)
            if (t > 0.5f)
            {
                transform.Rotate(voxelRotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // 분해 완료 후 오브젝트 비활성화 또는 제거
        OnDissolutionCompleted?.Invoke();

        // 완전히 분해된 오브젝트 처리 (선택적)
        // gameObject.SetActive(false);

        isDissolving = false;
    }

    // 셰이더 속성 업데이트
    private void UpdateShaderProperties(float? voxelSize = null, float? hollow = null)
    {
        for (int i = 0; i < rend.materials.Length; i++)
        {
            if (i < dissolveMaterials.Count)
            {
                Material mat = rend.materials[i];

                // 기본 속성 설정
                mat.SetFloat("_DissolveAmount", dissolveAmount);
                mat.SetFloat("_GlitchIntensity", glitchIntensity);

                // 선택적 속성 설정
                if (voxelSize.HasValue)
                {
                    mat.SetFloat("_VoxelSize", voxelSize.Value);
                }

                if (hollow.HasValue)
                {
                    mat.SetFloat("_VoxelHollow", hollow.Value);
                }
            }
        }
    }

    // 글리치 파티클 효과 생성
    private void SpawnGlitchParticles()
    {
        if (glitchParticlePrefab != null)
        {
            GameObject particles = Instantiate(glitchParticlePrefab, transform.position, Quaternion.identity);

            // 파티클 시스템 설정 (색상, 크기 등)
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = glowColor;
            }

            // 일정 시간 후 파티클 제거
            Destroy(particles, 5f);
        }
    }

    // 사운드 효과 재생
    private void PlayGlitchSound()
    {
        if (glitchSound != null)
        {
            AudioSource.PlayClipAtPoint(glitchSound, transform.position);
        }
    }

    private void PlayDissolutionSound()
    {
        if (dissolutionSound != null)
        {
            AudioSource.PlayClipAtPoint(dissolutionSound, transform.position);
        }
    }

    // 물리 처리 비활성화
    private void DisablePhysics()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }
    }
}