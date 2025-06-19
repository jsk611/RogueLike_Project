using System.Collections;
using UnityEngine;

/// <summary>
/// 진짜 오로라 공간 효과 - Plane 메시로 지속적인 오로라 커튼 생성
/// </summary>
public class AuroraSpaceEffect : MonoBehaviour
{
    [Header("Aurora Planes")]
    [SerializeField] private GameObject[] auroraPlanes;     // 오로라 평면들
    [SerializeField] private Material auroraMaterial;       // 오로라 쉐이더 머티리얼
    [SerializeField] private int planeCount = 8;            // 오로라 평면 개수
    
    [Header("Aurora Space Settings")]
    [SerializeField] private Vector3 spaceSize = new Vector3(8f, 10f, 8f);
    [SerializeField] private float auroraHeight = 12f;      // 오로라 높이
    [SerializeField] private bool createCylindrical = true;  // 원통형 오로라 공간
    
    [Header("Animation Settings")]
    [SerializeField] private float waveSpeed = 1f;          // 파도 속도
    [SerializeField] private float fadeInDuration = 2f;     // 페이드인 시간
    [SerializeField] private float sustainDuration = 3f;    // 지속 시간
    [SerializeField] private float fadeOutDuration = 2f;    // 페이드아웃 시간
    
    [Header("Aurora Colors")]
    [SerializeField] private Color[] auroraColors = {
        new Color(0.2f, 1f, 0.8f, 0.7f),    // 청록색
        new Color(0.8f, 0.2f, 1f, 0.7f),    // 보라색
        new Color(0.2f, 0.8f, 1f, 0.7f),    // 하늘색
        new Color(1f, 0.4f, 0.8f, 0.7f),    // 분홍색
        new Color(0.6f, 1f, 0.2f, 0.7f)     // 연두색
    };
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip auroraAppearSound;
    [SerializeField] private AudioClip auroraDisappearSound;
    
    private bool isActive = false;
    private Coroutine auroraCoroutine;
    private Material[] planeMaterials;
    
    void Start()
    {
        CreateAuroraPlanes();
        DeactivateAllPlanes();
    }
    
    /// <summary>
    /// 오로라 평면들 생성
    /// </summary>
    private void CreateAuroraPlanes()
    {
        if (auroraPlanes != null && auroraPlanes.Length > 0)
        {
            planeCount = auroraPlanes.Length;
            SetupExistingPlanes();
            return;
        }
        
        auroraPlanes = new GameObject[planeCount];
        planeMaterials = new Material[planeCount];
        
        for (int i = 0; i < planeCount; i++)
        {
            CreateAuroraPlane(i);
        }
        
        Debug.Log($"[Aurora] {planeCount}개의 오로라 평면 생성 완료");
    }
    
    /// <summary>
    /// 개별 오로라 평면 생성
    /// </summary>
    private void CreateAuroraPlane(int index)
    {
        GameObject plane = new GameObject($"AuroraPlane_{index}");
        plane.transform.SetParent(transform);
        
        MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();
        
        // Quad 메시 생성 (세로로 긴 형태)
        meshFilter.mesh = CreateAuroraPlaneMesh();
        
        // 위치 및 회전 설정
        if (createCylindrical)
        {
            // 원통형 배치
            float angle = (360f / planeCount) * index;
            float radian = angle * Mathf.Deg2Rad;
            float radius = spaceSize.x * 0.4f;
            
            Vector3 position = new Vector3(
                Mathf.Sin(radian) * radius,
                spaceSize.y * 0.5f,
                Mathf.Cos(radian) * radius
            );
            
            plane.transform.localPosition = position;
            plane.transform.localRotation = Quaternion.Euler(0, angle, 0);
            plane.transform.localScale = new Vector3(spaceSize.x * 0.3f, auroraHeight, 1f);
        }
        else
        {
            // 직선형 배치
            float spacing = spaceSize.z / (planeCount + 1);
            Vector3 position = new Vector3(0, spaceSize.y * 0.5f, -spaceSize.z * 0.5f + spacing * (index + 1));
            plane.transform.localPosition = position;
            plane.transform.localScale = new Vector3(spaceSize.x, auroraHeight, 1f);
        }
        
        // 머티리얼 설정
        Material planeMaterial = CreateAuroraMaterial(index);
        meshRenderer.material = planeMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        
        auroraPlanes[index] = plane;
        planeMaterials[index] = planeMaterial;
    }
    
    /// <summary>
    /// 오로라 평면 메시 생성
    /// </summary>
    private Mesh CreateAuroraPlaneMesh()
    {
        Mesh mesh = new Mesh();
        
        // 간단한 Quad 메시
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),  // 왼쪽 아래
            new Vector3(0.5f, -0.5f, 0),   // 오른쪽 아래
            new Vector3(-0.5f, 0.5f, 0),   // 왼쪽 위
            new Vector3(0.5f, 0.5f, 0)     // 오른쪽 위
        };
        
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// 오로라 머티리얼 생성
    /// </summary>
    private Material CreateAuroraMaterial(int index)
    {
        Material material;
        
        if (auroraMaterial != null)
        {
            material = new Material(auroraMaterial);
        }
        else
        {
            // 기본 투명 머티리얼 생성
            material = new Material(Shader.Find("Standard"));
            SetupTransparentMaterial(material);
        }
        
        // 색상 설정
        Color auroraColor = auroraColors[index % auroraColors.Length];
        material.color = auroraColor;
        
        return material;
    }
    
    /// <summary>
    /// 투명 머티리얼 설정
    /// </summary>
    private void SetupTransparentMaterial(Material material)
    {
        material.SetFloat("_Mode", 3); // Transparent 모드
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
    
    /// <summary>
    /// 기존 평면들 설정
    /// </summary>
    private void SetupExistingPlanes()
    {
        planeMaterials = new Material[auroraPlanes.Length];
        
        for (int i = 0; i < auroraPlanes.Length; i++)
        {
            if (auroraPlanes[i] != null)
            {
                MeshRenderer renderer = auroraPlanes[i].GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    planeMaterials[i] = renderer.material;
                }
            }
        }
    }
    
    /// <summary>
    /// 모든 평면 비활성화
    /// </summary>
    private void DeactivateAllPlanes()
    {
        foreach (var plane in auroraPlanes)
        {
            if (plane != null) plane.SetActive(false);
        }
        isActive = false;
    }
    
    #region Public API
    
    /// <summary>
    /// 오로라 공간 활성화
    /// </summary>
    public void ActivateAuroraSpace(System.Action onTransformCallback = null)
    {
        if (isActive) return;
        
        isActive = true;
        auroraCoroutine = StartCoroutine(AuroraSpaceSequence(onTransformCallback));
    }
    
    /// <summary>
    /// 오로라 공간 비활성화
    /// </summary>
    public void DeactivateAuroraSpace()
    {
        if (!isActive) return;
        
        if (auroraCoroutine != null)
        {
            StopCoroutine(auroraCoroutine);
        }
        
        StartCoroutine(QuickFadeOut());
    }
    
    /// <summary>
    /// 공간 크기 설정
    /// </summary>
    public void SetSpaceSize(Vector3 newSize)
    {
        spaceSize = newSize;
        
        if (auroraPlanes != null)
        {
            for (int i = 0; i < auroraPlanes.Length; i++)
            {
                if (auroraPlanes[i] != null)
                {
                    if (createCylindrical)
                    {
                        float angle = (360f / planeCount) * i;
                        float radian = angle * Mathf.Deg2Rad;
                        float radius = spaceSize.x * 0.4f;
                        
                        Vector3 position = new Vector3(
                            Mathf.Sin(radian) * radius,
                            spaceSize.y * 0.5f,
                            Mathf.Cos(radian) * radius
                        );
                        
                        auroraPlanes[i].transform.localPosition = position;
                        auroraPlanes[i].transform.localScale = new Vector3(spaceSize.x * 0.3f, auroraHeight, 1f);
                    }
                }
            }
        }
    }
    
    #endregion
    
    #region Aurora Sequence
    
    /// <summary>
    /// 오로라 공간 시퀀스
    /// </summary>
    private IEnumerator AuroraSpaceSequence(System.Action onTransformCallback)
    {
        Debug.Log("[Aurora] 오로라 공간 활성화 시작");
        
        if (audioSource != null && auroraAppearSound != null)
        {
            audioSource.PlayOneShot(auroraAppearSound);
        }
        
        // 1단계: 페이드인
        yield return StartCoroutine(FadeInAurora());
        
        // 2단계: 지속 (변신 실행)
        yield return StartCoroutine(SustainAurora(onTransformCallback));
        
        // 3단계: 페이드아웃
        yield return StartCoroutine(FadeOutAurora());
        
        isActive = false;
        Debug.Log("[Aurora] 오로라 공간 비활성화 완료");
    }
    
    /// <summary>
    /// 오로라 페이드인
    /// </summary>
    private IEnumerator FadeInAurora()
    {
        Debug.Log("[Aurora] 페이드인 시작");
        
        // 모든 평면 활성화
        foreach (var plane in auroraPlanes)
        {
            if (plane != null) plane.SetActive(true);
        }
        
        float timer = 0f;
        
        while (timer < fadeInDuration)
        {
            float progress = timer / fadeInDuration;
            float alpha = Mathf.Lerp(0f, 1f, progress);
            
            // 각 평면의 알파값 조정
            for (int i = 0; i < planeMaterials.Length; i++)
            {
                if (planeMaterials[i] != null)
                {
                    Color color = auroraColors[i % auroraColors.Length];
                    color.a = alpha * color.a;
                    planeMaterials[i].color = color;
                    
                    // 오로라 웨이브 애니메이션
                    UpdateAuroraWave(planeMaterials[i], timer, i);
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("[Aurora] 페이드인 완료");
    }
    
    /// <summary>
    /// 오로라 지속
    /// </summary>
    private IEnumerator SustainAurora(System.Action onTransformCallback)
    {
        Debug.Log("[Aurora] 오로라 지속 단계");
        
        float transformTime = sustainDuration * 0.5f;
        float timer = 0f;
        bool transformExecuted = false;
        
        while (timer < sustainDuration)
        {
            // 중간에 변신 실행
            if (!transformExecuted && timer >= transformTime)
            {
                onTransformCallback?.Invoke();
                transformExecuted = true;
                Debug.Log("[Aurora] 변신 실행!");
            }
            
            // 오로라 애니메이션 업데이트
            for (int i = 0; i < planeMaterials.Length; i++)
            {
                if (planeMaterials[i] != null)
                {
                    UpdateAuroraWave(planeMaterials[i], timer, i);
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("[Aurora] 오로라 지속 완료");
    }
    
    /// <summary>
    /// 오로라 페이드아웃
    /// </summary>
    private IEnumerator FadeOutAurora()
    {
        Debug.Log("[Aurora] 페이드아웃 시작");
        
        if (audioSource != null && auroraDisappearSound != null)
        {
            audioSource.PlayOneShot(auroraDisappearSound);
        }
        
        float timer = 0f;
        Color[] originalColors = new Color[planeMaterials.Length];
        
        for (int i = 0; i < planeMaterials.Length; i++)
        {
            if (planeMaterials[i] != null)
            {
                originalColors[i] = auroraColors[i % auroraColors.Length];
            }
        }
        
        while (timer < fadeOutDuration)
        {
            float progress = timer / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0f, progress);
            
            for (int i = 0; i < planeMaterials.Length; i++)
            {
                if (planeMaterials[i] != null)
                {
                    Color color = originalColors[i];
                    color.a = alpha * originalColors[i].a;
                    planeMaterials[i].color = color;
                    
                    UpdateAuroraWave(planeMaterials[i], timer, i);
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        DeactivateAllPlanes();
        Debug.Log("[Aurora] 페이드아웃 완료");
    }
    
    /// <summary>
    /// 빠른 페이드아웃
    /// </summary>
    private IEnumerator QuickFadeOut()
    {
        float duration = 0.5f;
        float timer = 0f;
        
        while (timer < duration)
        {
            float progress = timer / duration;
            float alpha = Mathf.Lerp(1f, 0f, progress);
            
            for (int i = 0; i < planeMaterials.Length; i++)
            {
                if (planeMaterials[i] != null)
                {
                    Color color = planeMaterials[i].color;
                    color.a = alpha * auroraColors[i % auroraColors.Length].a;
                    planeMaterials[i].color = color;
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        DeactivateAllPlanes();
        isActive = false;
    }
    
    #endregion
    
    /// <summary>
    /// 오로라 웨이브 애니메이션 업데이트
    /// </summary>
    private void UpdateAuroraWave(Material material, float time, int planeIndex)
    {
        if (material == null) return;
        
        // 오로라 흐름 효과 (UV 오프셋)
        if (material.HasProperty("_MainTex"))
        {
            float offset = (time * waveSpeed + planeIndex * 0.5f) % 1f;
            material.SetTextureOffset("_MainTex", new Vector2(0, offset));
        }
        
        // 머티리얼의 기본 속성들로 웨이브 시뮬레이션
        float wave = Mathf.Sin(time * waveSpeed + planeIndex * 0.8f) * 0.3f + 0.7f;
        
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", wave * 0.3f);
        }
        
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", wave * 0.8f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 오로라 공간 영역 시각화
        Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.2f);
        Gizmos.DrawCube(transform.position + Vector3.up * spaceSize.y * 0.5f, spaceSize);
        
        Gizmos.color = new Color(0.8f, 0.2f, 1f, 1f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * spaceSize.y * 0.5f, spaceSize);
        
        // 평면 위치 미리보기
        if (createCylindrical && Application.isPlaying == false)
        {
            for (int i = 0; i < planeCount; i++)
            {
                float angle = (360f / planeCount) * i;
                float radian = angle * Mathf.Deg2Rad;
                float radius = spaceSize.x * 0.4f;
                
                Vector3 position = transform.position + new Vector3(
                    Mathf.Sin(radian) * radius,
                    spaceSize.y * 0.5f,
                    Mathf.Cos(radian) * radius
                );
                
                Gizmos.color = auroraColors[i % auroraColors.Length];
                Gizmos.DrawLine(position + Vector3.down * auroraHeight * 0.5f, 
                              position + Vector3.up * auroraHeight * 0.5f);
            }
        }
    }
} 