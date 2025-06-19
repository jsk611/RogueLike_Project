using System.Collections;
using UnityEngine;

/// <summary>
/// 오로라 변신 효과 사용 예시 - 몬스터 변신 시 오로라로 가리기
/// </summary>
public class AuroraTransformationExample : MonoBehaviour
{
    [Header("Aurora Transformation Effect")]
    [SerializeField] private AuroraTransformationEffect auroraEffect;
    
    [Header("Objects")]
    [SerializeField] private GameObject[] originalObjects;    // 원래 오브젝트들
    [SerializeField] private GameObject[] transformedObjects; // 변신 후 오브젝트들
    [SerializeField] private bool autoSetupArea = true;       // 자동으로 효과 영역 설정
    
    [Header("Timing")]
    [SerializeField] private float preAuroraDelay = 0.3f;     // 오로라 시작 전 대기
    [SerializeField] private bool addShineEffect = true;      // 추가 빛나는 효과
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip preparationSound;     // 변신 준비 소리
    
    private bool isTransforming = false;
    
    void Start()
    {
        // 변신 후 오브젝트들은 초기에 비활성화
        foreach (var obj in transformedObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        // 자동으로 효과 영역 설정
        if (autoSetupArea && auroraEffect != null)
        {
            Vector3 area = CalculateTransformArea();
            auroraEffect.SetEffectArea(area);
        }
    }
    
    /// <summary>
    /// 오로라 변신 시작 (외부에서 호출)
    /// </summary>
    public void StartAuroraTransformation()
    {
        if (isTransforming) return;
        
        StartCoroutine(AuroraTransformationProcess());
    }
    
    /// <summary>
    /// 즉시 변신 (효과 없이)
    /// </summary>
    public void InstantTransformation()
    {
        if (isTransforming) return;
        
        ExecuteObjectSwap();
    }
    
    /// <summary>
    /// 역변신 (원래 상태로)
    /// </summary>
    public void ReverseTransformation()
    {
        if (isTransforming) return;
        
        StartCoroutine(ReverseAuroraTransformation());
    }
    
    /// <summary>
    /// 전체 오로라 변신 프로세스
    /// </summary>
    private IEnumerator AuroraTransformationProcess()
    {
        isTransforming = true;
        Debug.Log("[AuroraExample] 오로라 변신 프로세스 시작");
        
        // 준비 사운드 재생
        if (audioSource != null && preparationSound != null)
        {
            audioSource.PlayOneShot(preparationSound);
        }
        
        // 변신 전 대기 (신비로운 분위기 조성)
        yield return new WaitForSeconds(preAuroraDelay);
        
        // 오로라 빛나는 효과 (선택적)
        if (addShineEffect)
        {
            yield return StartCoroutine(PreAuroraShineEffect());
        }
        
        // 오로라 효과 시작 (콜백으로 실제 변신 실행)
        if (auroraEffect != null)
        {
            auroraEffect.StartAuroraTransformation(() => {
                ExecuteObjectSwap();
            });
            
            // 효과가 완료될 때까지 대기
            yield return new WaitForSeconds(1f);
        }
        else
        {
            // 이펙트가 없으면 바로 변신
            ExecuteObjectSwap();
            yield return new WaitForSeconds(2f);
        }
        
        isTransforming = false;
        Debug.Log("[AuroraExample] 오로라 변신 프로세스 완료");
    }
    
    /// <summary>
    /// 역변신 프로세스
    /// </summary>
    private IEnumerator ReverseAuroraTransformation()
    {
        isTransforming = true;
        Debug.Log("[AuroraExample] 오로라 역변신 프로세스 시작");
        
        if (audioSource != null && preparationSound != null)
        {
            audioSource.PlayOneShot(preparationSound);
        }
        
        yield return new WaitForSeconds(preAuroraDelay);
        
        if (auroraEffect != null)
        {
            auroraEffect.StartAuroraTransformation(() => {
                ExecuteReverseSwap();
            });
            
            yield return new WaitForSeconds(1f);
        }
        else
        {
            ExecuteReverseSwap();
            yield return new WaitForSeconds(2f);
        }
        
        isTransforming = false;
        Debug.Log("[AuroraExample] 오로라 역변신 프로세스 완료");
    }
    
    /// <summary>
    /// 오로라 전 빛나는 효과
    /// </summary>
    private IEnumerator PreAuroraShineEffect()
    {
        Debug.Log("[AuroraExample] 사전 빛나는 효과");
        
        // 모든 원본 오브젝트에 빛나는 효과 적용
        var renderers = new System.Collections.Generic.List<Renderer>();
        foreach (var obj in originalObjects)
        {
            if (obj != null)
                renderers.AddRange(obj.GetComponentsInChildren<Renderer>());
        }
        
        float shineTime = 0.8f;
        float timer = 0f;
        
        while (timer < shineTime)
        {
            float progress = timer / shineTime;
            float brightness = Mathf.Sin(progress * Mathf.PI * 3) * 0.5f + 1f; // 3번 깜빡임
            
            // 각 렌더러의 emission 조정 (간소화)
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.material.HasProperty("_Color"))
                {
                    Color originalColor = renderer.material.color;
                    Color brightColor = originalColor * brightness;
                    renderer.material.color = brightColor;
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // 색상 리셋
        foreach (var renderer in renderers)
        {
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = Color.white;
            }
        }
        
        Debug.Log("[AuroraExample] 사전 빛나는 효과 완료");
    }
    
    /// <summary>
    /// 실제 오브젝트 교체 실행
    /// </summary>
    private void ExecuteObjectSwap()
    {
        Debug.Log("[AuroraExample] 오브젝트 교체 실행");
        
        // 원본 오브젝트들 비활성화
        foreach (var obj in originalObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        // 변신 후 오브젝트들 활성화
        foreach (var obj in transformedObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        Debug.Log($"[AuroraExample] 변신 완료: {originalObjects.Length}개 → {transformedObjects.Length}개");
    }
    
    /// <summary>
    /// 역변신 오브젝트 교체
    /// </summary>
    private void ExecuteReverseSwap()
    {
        Debug.Log("[AuroraExample] 역변신 오브젝트 교체 실행");
        
        // 변신 후 오브젝트들 비활성화
        foreach (var obj in transformedObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        // 원본 오브젝트들 활성화
        foreach (var obj in originalObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        Debug.Log($"[AuroraExample] 역변신 완료: {transformedObjects.Length}개 → {originalObjects.Length}개");
    }
    
    /// <summary>
    /// 변신 영역 크기 자동 계산
    /// </summary>
    private Vector3 CalculateTransformArea()
    {
        if (originalObjects == null || originalObjects.Length == 0)
            return new Vector3(5f, 6f, 5f);
        
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool boundsInitialized = false;
        
        // 원본 오브젝트들의 바운딩 박스 계산
        foreach (var obj in originalObjects)
        {
            if (obj == null) continue;
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        
        // 변신 후 오브젝트들도 포함
        foreach (var obj in transformedObjects)
        {
            if (obj == null) continue;
            
            // 임시로 활성화해서 바운딩 박스 계산
            bool wasActive = obj.activeSelf;
            obj.SetActive(true);
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            
            obj.SetActive(wasActive);
        }
        
        // 최소 크기 보장 및 여유 공간 추가 (오로라는 넓게)
        Vector3 size = bounds.size;
        size.x = Mathf.Max(size.x, 3f) + 4f;  // 오로라 여유 공간
        size.y = Mathf.Max(size.y, 4f) + 3f;  // 높이 여유
        size.z = Mathf.Max(size.z, 3f) + 4f;
        
        Debug.Log($"[AuroraExample] 계산된 오로라 영역: {size}");
        return size;
    }
    
    /// <summary>
    /// 테스트용 키보드 입력
    /// </summary>
    void Update()
    {
        // A키로 오로라 변신 테스트
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartAuroraTransformation();
        }
        
        // R키로 역변신 테스트
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReverseTransformation();
        }
        
        // I키로 즉시 변신 테스트
        if (Input.GetKeyDown(KeyCode.I))
        {
            InstantTransformation();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 원본 오브젝트들 영역 시각화
        if (originalObjects != null && originalObjects.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var obj in originalObjects)
            {
                if (obj != null && obj.activeSelf)
                {
                    Gizmos.DrawWireCube(obj.transform.position, obj.transform.localScale);
                }
            }
        }
        
        // 변신 후 오브젝트들 영역 시각화
        if (transformedObjects != null && transformedObjects.Length > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var obj in transformedObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawWireCube(obj.transform.position, obj.transform.localScale);
                }
            }
        }
        
        // 오로라 영역 시각화
        if (autoSetupArea)
        {
            Vector3 area = CalculateTransformArea();
            Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.2f);
            Gizmos.DrawCube(transform.position, area);
        }
    }
} 