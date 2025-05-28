using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenadier : RangedMonster
{
    [Header("Grenadier Settings")]
    [SerializeField] private Transform throwPoint; // 투척 시작 위치
    [SerializeField] private GameObject throwablePrefab; // 투척체 프리팹
    [SerializeField] private float throwForce = 15f; // 투척 힘 (증가)
    [SerializeField] private float arcHeight = 5f; // 포물선의 최고점 높이
    [SerializeField] private float maxThrowDistance = 20f; // 최대 투척 거리
    [SerializeField] private bool useHighArc = true; // 높은 궤도 vs 낮은 궤도

    [Header("Prediction Settings")]
    [SerializeField] private bool predictPlayerMovement = true; // 플레이어 움직임 예측
    [SerializeField] private float predictionTime = 0.5f; // 예측 시간
    [SerializeField] private LayerMask obstacleLayer = -1; // 장애물 레이어

    [Header("Debug")]
    [SerializeField] private bool showTrajectory = true; // 궤적 시각화
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 30;

    protected override void Start()
    {
        base.Start();
        aimTime = attackCooldown * 0.3f; // 조준 시간
        attackTime = attackCooldown * 0.7f; // 공격 시간

        // 궤적 시각화 컴포넌트 설정
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
        }

        if (trajectoryLine != null && showTrajectory)
        {
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.enabled = false;
        }
    }

    public override void FireEvent()
    {
        if (throwablePrefab != null && target != null)
        {
            Vector3 targetPosition = GetPredictedTargetPosition();

            // 투척 가능한 거리인지 확인
            float distance = Vector3.Distance(throwPoint.position, targetPosition);
            if (distance > maxThrowDistance)
            {
                // 최대 거리로 제한
                Vector3 direction = (targetPosition - throwPoint.position).normalized;
                targetPosition = throwPoint.position + direction * maxThrowDistance;
            }

            // 궤적 시각화 숨기기
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }

            // 투척 무기 사용
            ThrowGrenade(targetPosition);
        }
    }

    // 플레이어 움직임을 예측한 타겟 위치 계산
    private Vector3 GetPredictedTargetPosition()
    {
        if (!predictPlayerMovement || target == null)
            return target.position;

        // 플레이어의 현재 속도 가져오기
        Rigidbody playerRb = target.GetComponent<Rigidbody>();
        Vector3 playerVelocity = Vector3.zero;

        if (playerRb != null)
        {
            playerVelocity = playerRb.velocity;
        }
        else
        {
            // Rigidbody가 없다면 CharacterController 확인
            CharacterController playerCC = target.GetComponent<CharacterController>();
            if (playerCC != null)
            {
                // CharacterController의 경우 직접 속도를 구하기 어려우므로 근사치 사용
                playerVelocity = (target.position - target.position) / Time.deltaTime;
            }
        }

        // 예측된 위치 계산
        Vector3 predictedPosition = target.position + playerVelocity * predictionTime;

        // 지면에 맞추기 (Y축 보정)
        RaycastHit hit;
        if (Physics.Raycast(predictedPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            predictedPosition.y = hit.point.y;
        }

        return predictedPosition;
    }

    private void ThrowGrenade(Vector3 targetPosition)
    {
        // 최소 높이 보정 (너무 낮으면 던지는 사람 발밑에 떨어짐)
        if (targetPosition.y < throwPoint.position.y)
        {
            targetPosition.y = throwPoint.position.y;
        }

        // 투척체 생성
        GameObject grenade = Instantiate(throwablePrefab, throwPoint.position, Quaternion.identity);

        // 투척체에 데미지 설정 (있다면)
        EnemyThrowableWeapon explosive = grenade.GetComponent<EnemyThrowableWeapon>();
        if (explosive != null)
        {
            explosive.SetExplosionDamage(monsterStatus.GetAttackDamage());
        }

        // 투척체 방향 계산 및 적용
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 간단한 포물선 계산 방법 사용
            Vector3 throwVelocity = CalculateThrowVelocity(throwPoint.position, targetPosition);
            rb.velocity = throwVelocity;

            // 회전 효과 추가 (더 자연스러운 움직임)
            rb.angularVelocity = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(-3f, 3f),
                Random.Range(-3f, 3f)
            );

            Debug.Log($"Grenade thrown with velocity: {throwVelocity}, magnitude: {throwVelocity.magnitude}");
        }
    }

    private Vector3 CalculateThrowVelocity(Vector3 startPos, Vector3 targetPos)
    {
        Vector3 displacement = targetPos - startPos;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;
        float verticalDistance = displacement.y;

        // 안전한 최소/최대 각도 설정
        float throwAngle = useHighArc ? 60f : 30f; // 도 단위
        throwAngle = Mathf.Clamp(throwAngle, 15f, 75f); // 15~75도 사이로 제한

        float angleRad = throwAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y); // 중력은 항상 양수로

        // 필요한 초기 속도 계산
        float velocityMagnitude = CalculateRequiredVelocity(horizontalDistance, verticalDistance, angleRad, gravity);

        // 수평 방향 계산
        Vector3 horizontalDirection = horizontalDisplacement.normalized;

        // 속도 벡터 조합
        Vector3 velocity = horizontalDirection * velocityMagnitude * Mathf.Cos(angleRad) +
                          Vector3.up * velocityMagnitude * Mathf.Sin(angleRad);

        Debug.Log($"Throwing at angle: {throwAngle}°, velocity magnitude: {velocityMagnitude}, horizontal distance: {horizontalDistance}");

        return velocity;
    }

    private float CalculateRequiredVelocity(float horizontalDist, float verticalDist, float angle, float gravity)
    {
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);
        float tanAngle = Mathf.Tan(angle);

        // v² = (g * x²) / (2 * cos²(θ) * (x * tan(θ) - y))
        float denominator = 2f * cosAngle * cosAngle * (horizontalDist * tanAngle - verticalDist);

        if (denominator <= 0)
        {
            // 계산이 불가능한 경우 기본값 사용
            Debug.LogWarning("Invalid trajectory calculation, using default throw force");
            return throwForce;
        }

        float velocitySquared = (gravity * horizontalDist * horizontalDist) / denominator;

        if (velocitySquared <= 0)
        {
            return throwForce;
        }

        float calculatedVelocity = Mathf.Sqrt(velocitySquared);

        // 속도 제한 (너무 빠르거나 느리지 않게)
        return Mathf.Clamp(calculatedVelocity, 5f, 30f);
    }


    // 특정 시간에서의 위치 계산
    private Vector3 CalculatePositionAtTime(Vector3 startPos, Vector3 initialVelocity, float time)
    {
        Vector3 gravity = Physics.gravity;
        return startPos + initialVelocity * time + 0.5f * gravity * time * time;
    }

    // 조준 시 궤적 미리보기 (디버그용)
    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        if (showTrajectory && trajectoryLine != null && target != null)
        {
            if (attackTimer > aimTime * 0.5f && attackTimer < attackTime)
            {
                ShowTrajectoryPreview();
            }
        }
    }

    private void ShowTrajectoryPreview()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        Vector3 targetPos = GetPredictedTargetPosition();
        Vector3 velocity = CalculateThrowVelocity(throwPoint.position, targetPos);

        Vector3[] points = new Vector3[trajectoryPoints];
        float maxTime = 3f; // 최대 3초까지 궤적 표시

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = (float)i / (trajectoryPoints - 1) * maxTime;
            points[i] = CalculatePositionAtTime(throwPoint.position, velocity, t);

            // 지면에 닿으면 궤적 종료
            if (points[i].y <= targetPos.y)
            {
                points[i].y = targetPos.y;
                // 나머지 점들은 마지막 점으로 설정
                for (int j = i + 1; j < trajectoryPoints; j++)
                {
                    points[j] = points[i];
                }
                break;
            }
        }

        trajectoryLine.SetPositions(points);
    }

    // 디버그 시각화
    private void OnDrawGizmosSelected()
    {
        if (throwPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(throwPoint.position, 0.2f);

            if (target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(throwPoint.position, target.position);

                // 최대 투척 거리 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(throwPoint.position, maxThrowDistance);
            }
        }
    }
}
