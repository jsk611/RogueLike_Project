using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenadier : RangedMonster
{
    [Header("Grenadier Settings")]
    [SerializeField] private Transform throwPoint; // 투척 시작 위치
    [SerializeField] private GameObject throwablePrefab; // 투척체 프리팹
    [SerializeField] private float throwForce = 5f; // 투척 힘
    [SerializeField] private float arcHeight = 3f; // 포물선의 최고점 높이

    protected override void Start()
    {
        base.Start();
        aimTime = attackCooldown * 0.2f;
        aimTime = attackCooldown * 0.4f;
    }
    public override void FireEvent()
    {
        if (throwablePrefab != null && target != null)
        {
            Vector3 targetPosition = target.position;

            // 투척 무기 사용
            Throw(targetPosition);
            //Debug.Log($"Grenade thrown at {targetPosition}");
        }
    }

    private void Throw(Vector3 targetPosition)
    {
        // 투척체 생성
        GameObject throwable = Instantiate(throwablePrefab, throwPoint.position, Quaternion.identity);

        // 투척체 방향 계산
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDirection = CalculateThrowDirection(throwPoint.position, targetPosition, arcHeight);
            rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
    }

    private Vector3 CalculateThrowDirection(Vector3 start, Vector3 target, float height)
    {
        // 포물선 궤적 계산
        Vector3 direction = target - start;
        direction.y = 0; // 수평 방향
        float distance = direction.magnitude;
        direction.Normalize();

        float verticalVelocity = Mathf.Sqrt(2 * Physics.gravity.magnitude * height);
        float time = Mathf.Sqrt(2 * height / Physics.gravity.magnitude) + Mathf.Sqrt(2 * (height + target.y - start.y) / Physics.gravity.magnitude);

        Vector3 throwDirection = direction * (distance / time) + Vector3.up * verticalVelocity;
        return throwDirection;
    }
}
