using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrowableWeapon : MonoBehaviour
{
    [SerializeField] private GameObject fieldPrefab; // 생성할 장판 프리팹
    [SerializeField] private float explosionRadius = 3f; // 폭발 범위
    [SerializeField] private LayerMask targetLayer; // 충돌 대상 레이어
    [SerializeField] private float fireFieldDuration = 5f; // 지속 시간

    private bool hasExploded = false;


    private void OnCollisionEnter(Collision collision)
    {
        // 바닥과 충돌 확인
        if (collision.gameObject.CompareTag("Floor"))
        {
            if (hasExploded) return;
            hasExploded = true;
            if (fieldPrefab == null)
            {
                Debug.Log("Field prefab is missing. Please assign it.");
            }

            //Debug.Log($"Number of contacts: {collision.contacts.Length}");
            //foreach (var contact in collision.contacts)
            //{
            //    Debug.Log($"Contact point: {contact.point}");
            //}

            Debug.Log("Boom");
            // 폭탄 제거
            Destroy(gameObject);

            // 불타오르는 필드 생성
            GameObject fireField = Instantiate(fieldPrefab,
                                               collision.contacts[0].point,
                                               Quaternion.identity);

            // 필드 지속 시간 설정
            Destroy(fireField, fireFieldDuration);
        }
    }
    
}
