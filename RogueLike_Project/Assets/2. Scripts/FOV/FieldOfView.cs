using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("FOV")]
    [SerializeField, Min(0.1f)] private float viewRadius = 15f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 90f;
    [SerializeField] private Transform eyePoint; // 시야 원점(없으면 this.transform)

    [Header("Layers")]
    [SerializeField] private LayerMask targetMask;   // 보려는 대상
    [SerializeField] private LayerMask occluderMask; // 시야를 가리는 레이어

    [Header("Scan")]
    [SerializeField, Min(0f)] private float pollInterval = 0.2f;
    [SerializeField, Range(1, 128)] private int maxVisibleTargets = 32;

    // 결과: 보이는 타깃들 (읽기 전용 노출)
    public IReadOnlyList<Transform> VisibleTargets => _visibleTargets;

    // 디버그: 가장 최근 검사한 타깃의 부분 가림 결과
    public bool upClear { get; private set; }
    public bool downClear { get; private set; }
    public bool leftClear { get; private set; }
    public bool rightClear { get; private set; }

    private readonly List<Transform> _visibleTargets = new List<Transform>(32);
    private Collider[] _overlapBuffer = new Collider[64];     // NonAlloc 버퍼
    private RaycastHit[] _raycastBuffer = new RaycastHit[1];  // NonAlloc 버퍼(최대 1개만 필요)
    private float _cosHalfFov;
    private Coroutine _scanRoutine;
    private WaitForSeconds _waitObj;

    private void Awake()
    {
        if (eyePoint == null) eyePoint = transform;
        RecomputeCosHalfFov();
        _waitObj = pollInterval > 0f ? new WaitForSeconds(pollInterval) : null;
    }

    private void OnValidate()
    {
        RecomputeCosHalfFov();
        if (pollInterval < 0f) pollInterval = 0f;
        if (eyePoint == null) eyePoint = transform;
    }

    private void OnEnable()
    {
        _scanRoutine = StartCoroutine(ScanLoop());
    }

    private void OnDisable()
    {
        if (_scanRoutine != null) StopCoroutine(_scanRoutine);
        _scanRoutine = null;
    }

    private IEnumerator ScanLoop()
    {
        while (true)
        {
            Scan();
            if (_waitObj != null) yield return _waitObj;
            else yield return null; // 매 프레임
        }
    }

    private void Scan()
    {
        _visibleTargets.Clear();
        upClear = downClear = leftClear = rightClear = false;

        Vector3 eye = eyePoint.position;

        int count = Physics.OverlapSphereNonAlloc(
            eye, viewRadius, _overlapBuffer, targetMask, QueryTriggerInteraction.Ignore);

        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (col == null) continue;

            Transform t = col.transform;
            if (t == transform) continue;

            Vector3 center = col.bounds.center;
            Vector3 to = center - eye;
            float dist = to.magnitude;
            if (dist <= 0.0001f) { _visibleTargets.Add(t); continue; }
            Vector3 dir = to / dist;

            // FOV 도트 체크
            if (Vector3.Dot(eyePoint.forward, dir) < _cosHalfFov) continue;

            // 센터 가림 체크
            bool centerBlocked = IsBlocked(eye, center);
            bool anyVisible = !centerBlocked;

            // 부분 가림 허용: head/feet/left/right 샘플
            if (!anyVisible)
            {
                SamplePoints(col, eye, dir, out var head, out var feet, out var left, out var right);

                upClear    = !IsBlocked(eye, head);
                downClear  = !IsBlocked(eye, feet);
                leftClear  = !IsBlocked(eye, left);
                rightClear = !IsBlocked(eye, right);

                anyVisible = upClear || downClear || leftClear || rightClear;
            }

            if (anyVisible)
            {
                _visibleTargets.Add(t);
                if (_visibleTargets.Count >= maxVisibleTargets) break;
            }
        }
    }

    private bool IsBlocked(Vector3 from, Vector3 to)
    {
        Vector3 d = to - from;
        float len = d.magnitude;
        if (len <= 0f) return false;

        // 가림체만 검사 (타깃 콜라이더는 targetMask로만 포함되므로 여기선 occluderMask만)
        int hitCount = Physics.RaycastNonAlloc(
            from, d / len, _raycastBuffer, len, occluderMask, QueryTriggerInteraction.Ignore);

        return hitCount > 0;
    }

    // 대상 콜라이더의 대략적인 4개 샘플 포인트(머리/발/좌/우)
    private void SamplePoints(Collider col, Vector3 eye, Vector3 dirToTarget,
                              out Vector3 head, out Vector3 feet, out Vector3 left, out Vector3 right)
    {
        var b = col.bounds;
        Vector3 center = b.center;

        float halfY = b.extents.y;
        float r = Mathf.Min(b.extents.x, b.extents.z) * 0.9f;
        Vector3 lateral = Vector3.Cross(Vector3.up, dirToTarget).normalized;

        head  = center + Vector3.up * halfY;
        feet  = center - Vector3.up * halfY;
        right = center + lateral * r;
        left  = center - lateral * r;
    }

    private void RecomputeCosHalfFov()
    {
        _cosHalfFov = Mathf.Cos(0.5f * viewAngle * Mathf.Deg2Rad);
    }

    // 시각화/에디터 디버깅용
    private void OnDrawGizmosSelected()
    {
        var eye = eyePoint ? eyePoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, viewRadius);

        Vector3 a = DirFromAngle(-viewAngle * 0.5f) * viewRadius;
        Vector3 b = DirFromAngle(+viewAngle * 0.5f) * viewRadius;
        Gizmos.DrawLine(eye, eye + a);
        Gizmos.DrawLine(eye, eye + b);
    }

    private Vector3 DirFromAngle(float angle)
    {
        return Quaternion.Euler(0f, angle, 0f) * (eyePoint ? eyePoint.forward : transform.forward);
    }
}
