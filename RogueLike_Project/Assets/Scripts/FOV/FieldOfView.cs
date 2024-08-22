using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] float viewRadius;
    [SerializeField] [Range(0, 360)] float viewAngle;

    public LayerMask targetMask, obstacleMask, playerMask;

    public bool upClear;
    public bool downClear;
    public bool leftClear;
    public bool rightClear;

    // Target mask에 ray hit된 transform을 보관하는 리스트
    public List<Transform> visibleTargets = new List<Transform>();

    void Start()
    {
        upClear = true;
        downClear = true;
        leftClear = true;
        rightClear = true;

        // 0.2초 간격으로 코루틴 호출
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        // viewRadius를 반지름으로 한 원 영역 내 targetMask 레이어인 콜라이더를 모두 가져옴
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        if (targetsInViewRadius.Length > 0)
        {
            Transform target = targetsInViewRadius[0].transform;
            CharacterController targetCollider = target.GetComponent<CharacterController>();

            if (targetCollider != null)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                // 플레이어와 forward와 target이 이루는 각이 설정한 각도 내라면
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float dstToTarget = Vector3.Distance(transform.position, target.transform.position);
                    if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit hit, dstToTarget, obstacleMask))
                    {
                        Vector3 capsuleCenter = targetCollider.bounds.center;
                        float capsuleHeight = targetCollider.height / 2;
                        float capsuleRadius = targetCollider.radius;

                        Vector3 upEdge = capsuleCenter + Vector3.up * capsuleHeight;
                        Vector3 downEdge = capsuleCenter - Vector3.up * capsuleHeight;
                        Vector3 rightEdge = capsuleCenter + target.right * capsuleRadius;
                        Vector3 leftEdge = capsuleCenter - target.right * capsuleRadius;


                        upClear = !Physics.Raycast(transform.position, (upEdge - transform.position).normalized, dstToTarget, obstacleMask);
                        downClear = !Physics.Raycast(transform.position, (downEdge - transform.position).normalized, dstToTarget, obstacleMask);
                        rightClear = !Physics.Raycast(transform.position, (rightEdge - transform.position).normalized, dstToTarget, obstacleMask);
                        leftClear = !Physics.Raycast(transform.position, (leftEdge - transform.position).normalized, dstToTarget, obstacleMask);


                        // 각 가장자리 점에 대해 Raycasting 수행
                        if (upClear || downClear || rightClear || leftClear)
                        {
                            visibleTargets.Add(target);
                        }

                    }
                    else
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }
       
    }

    // y축 오일러 각을 3차원 방향 벡터로 변환한다.
    // 원본과 구현이 살짝 다름에 주의. 결과는 같다.
    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad));
    }

    public float GetRadius() { return viewRadius; }

    public float GetAngle() { return viewAngle; }

}
