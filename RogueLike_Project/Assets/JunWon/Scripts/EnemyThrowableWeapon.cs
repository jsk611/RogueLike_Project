using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrowableWeapon : MonoBehaviour
{
    [SerializeField] private GameObject fieldPrefab; // 생성할 장판 프리팹
    [SerializeField] private float explosionRadius = 3f; // 폭발 범위
    [SerializeField] private LayerMask targetLayer; // 충돌 대상 레이어

    private void OnCollisionEnter(Collision collision)
    {
        Explode(collision.contacts[0].point);
    }
    private void Explode(Vector3 position)
    {
        // 폭발 위치에 장판 생성
        if (fieldPrefab != null)
        {
            Instantiate(fieldPrefab, position, Quaternion.identity);
        }

        // 폭발 시 범위 내 적에게 초기 데미지 적용 (선택)
        Collider[] hitColliders = Physics.OverlapSphere(position, explosionRadius, targetLayer);
        foreach (var collider in hitColliders)
        {
            PlayerStatus player = collider.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.DecreaseHealth(20f); // 폭발 초기 데미지
            }
        }

        Destroy(gameObject, 5.0f); // 투척체 제거
    }
}
