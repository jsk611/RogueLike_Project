using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    public float maxRadius = 10f; // 충격파 최대 반경
    public float expansionSpeed = 5f; // 충격파 확장 속도
    public float damage = 20f; // 데미지
    public LayerMask targetLayer; // 영향을 받을 레이어
    public float duration = 2f; // 충격파 지속 시간

    private HashSet<Collider> hitTargets = new HashSet<Collider>(); // 데미지를 받은 대상
    private float innerRadius = 0f; // 충격파 내부 반경
    private float outerRadius = 0f; // 충격파 외부 반경
    private Transform shockwaveVisual;

    private void Start()
    {
        damage = GetComponentInParent<MonsterStatus>().GetAttackDamage();
        shockwaveVisual = transform.GetChild(0); // 자식 오브젝트로 시각적 효과 연결
        shockwaveVisual.localScale = Vector3.zero; // 초기 크기
    }

    private void Update()
    {
        // 충격파 확장
        innerRadius = outerRadius; // 이전 외부 반경을 내부 반경으로 업데이트
        outerRadius = Mathf.Min(outerRadius + expansionSpeed * Time.deltaTime, maxRadius);

        shockwaveVisual.localScale = Vector3.one * outerRadius * 2f; // 시각적 효과 업데이트

        // 충격파 경로에 새로 포함된 대상 처리
        ApplyShockwaveEffect();

        // 최대 반경에 도달하면 제거
        if (outerRadius >= maxRadius)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyShockwaveEffect()
    {
        // 현재 반경 범위 안에 있는 모든 충돌체 검색
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, outerRadius, targetLayer);

        foreach (var collider in hitColliders)
        {
            // 충격파의 "새로 포함된" 영역에 있는 대상만 처리
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < innerRadius || hitTargets.Contains(collider)) continue;

            // 대상 데미지 처리
            PlayerStatus player = collider.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.DecreaseHealth(damage);
            }

            // 데미지 판정 목록에 추가
            hitTargets.Add(collider);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 디버그용 충격파 내부 및 외부 반경 시각화
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }
}
