using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusLaser : MonoBehaviour
{
    [Header("레이저 설정")]
    [SerializeField] private float damage = 10f;          // 레이저가 입히는 데미지
    [SerializeField] private float lifeTime = 1.5f;       // 레이저 지속 시간
    [SerializeField] private bool applyKnockback = true;  // 넉백 적용 여부
    [SerializeField] private float knockbackForce = 5f;   // 넉백 힘

    [Header("시각 효과")]
    [SerializeField] private float startWidth = 0.1f;     // 초기 레이저 너비
    [SerializeField] private float maxWidth = 0.5f;       // 최대 레이저 너비
    [SerializeField] private Color startColor = new Color(1f, 0.2f, 0.2f, 0.8f);  // 시작 색상
    [SerializeField] private Color endColor = new Color(1f, 0f, 0f, 0f);          // 종료 색상
    [SerializeField] private float heightOffset = 5f;     // 시작 높이 오프셋

    [Header("Trigger 설정")]
    [SerializeField] private Vector3 triggerSize = new Vector3(1f, 0.5f, 1f); // 트리거 콜라이더 크기

    // 컴포넌트 참조
    private LineRenderer lineRenderer;
    private BoxCollider triggerCollider;
    private bool hasDamaged = false;  // 이미 데미지를 입혔는지 여부
    private bool isImpactReady = false; // 임팩트 효과가 준비되었는지 여부

    // 피격 이벤트 델리게이트
    public delegate void LaserHitEvent(GameObject target, float damage);
    public static event LaserHitEvent OnLaserHit;

    private void Awake()
    {
        // 라인 렌더러 컴포넌트 가져오기
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            SetupLineRenderer();
        }

        // 트리거 콜라이더 설정
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }

        // 콜라이더 설정
        triggerCollider.isTrigger = true;
        triggerCollider.size = triggerSize;
        triggerCollider.center = Vector3.zero;

        // 초기에는 콜라이더 비활성화 (레이저 임팩트 시점에 활성화)
        triggerCollider.enabled = false;

        // 자동 제거 타이머 설정
        Destroy(gameObject, lifeTime);
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = startWidth * 0.8f;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // 머티리얼 설정
        Material laserMaterial = new Material(Shader.Find("Sprites/Default"));
        laserMaterial.color = startColor;
        lineRenderer.material = laserMaterial;

        // 발광 효과를 위한 설정
        laserMaterial.EnableKeyword("_EMISSION");
        laserMaterial.SetColor("_EmissionColor", startColor * 2f);
    }

    private void Start()
    {
        // 초기 위치 설정: 하늘에서 타겟으로 내려오는 형태
        Vector3 targetPosition = transform.position;
        Vector3 startPosition = targetPosition + Vector3.up * heightOffset;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition);

        // 레이저 애니메이션 시작
        StartCoroutine(AnimateLaser(startPosition, targetPosition));
    }

    private IEnumerator AnimateLaser(Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        float impactTime = 0.3f; // 타겟에 도달하는 시간

        // 1. 하늘에서 타겟으로 빠르게 내려오는 단계
        while (elapsed < impactTime)
        {
            float t = elapsed / impactTime;
            Vector3 currentEndPos = Vector3.Lerp(startPos, endPos, t);

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, currentEndPos);

            // 너비 점점 증가
            float currentWidth = Mathf.Lerp(startWidth, maxWidth, t);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth * 0.8f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 타겟에 도달 후 효과
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // 임팩트 효과 (흰색 플래시)
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        // 임팩트 위치 설정 및 트리거 활성화
        transform.position = endPos;
        isImpactReady = true;
        triggerCollider.enabled = true;

        yield return new WaitForSeconds(0.1f);

        // 3. 페이드 아웃
        elapsed = 0f;
        float fadeDuration = lifeTime - impactTime - 0.1f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;

            // 색상 페이드 아웃
            lineRenderer.startColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.endColor = Color.Lerp(startColor, endColor, t);

            // 너비 감소
            float currentWidth = Mathf.Lerp(maxWidth, startWidth * 0.5f, t);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth * 0.5f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 페이드 아웃 후에는 충돌 비활성화
        triggerCollider.enabled = false;
    }

    // OnTriggerEnter로 충돌 처리
    private void OnTriggerEnter(Collider other)
    {
        // 임팩트 준비가 안된 상태거나 이미 데미지를 입힌 경우 무시
        if (!isImpactReady || hasDamaged) return;

        // 플레이어 레이어 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어 데미지 처리
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // 데미지 적용
                playerStatus.DecreaseHealth(damage);
                hasDamaged = true;

                // 이벤트 발생
                OnLaserHit?.Invoke(other.gameObject, damage);

                // 넉백 효과
                if (applyKnockback)
                {
                    Rigidbody rb = other.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // 약간 위쪽으로 넉백 (점프 효과)
                        Vector3 knockbackDir = Vector3.up + (other.transform.position - transform.position).normalized;
                        knockbackDir.Normalize();
                        rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                    }
                }

                // 히트 이펙트 생성
                CreateHitEffect(transform.position);
            }
        }
    }

    // 히트 이펙트 생성
    private void CreateHitEffect(Vector3 position)
    {
        // 여기에 히트 이펙트를 추가할 수 있습니다 (파티클 시스템 등)
        /*
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        */
    }

    // 디버그용 기즈모 표시 (트리거 영역)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (triggerCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, triggerSize);
        }
    }

    // 레이저 데미지 설정 (외부에서 호출 가능)
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}