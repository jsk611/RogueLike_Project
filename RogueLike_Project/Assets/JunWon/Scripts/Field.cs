using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Field : MonoBehaviour
{
    public float damagePerSecond = 10f; // 초당 데미지
    public float damageInterval = 1.0f;
    private bool isPlayerInside = false; // 플레이어가 영역 안에 있는지 확인
    [SerializeField] private GameObject player; // 플레이어 객체 참조
    [SerializeField] private float damageTimer = 0f; // 타이머 변수
    private void Update()
    {
        if (player == null || !isPlayerInside) return;

        // 지정된 간격에 도달하면 데미지를 적용
        if (damageTimer >= damageInterval)
        {
            Debug.Log("Get Blazed");
            ApplyDamage();
            damageTimer = 0f; // 타이머 초기화
        }
        else
        {
            // 타이머를 증가시킴
            damageTimer += Time.deltaTime;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Area");
        if (other.CompareTag("Player")) // 플레이어가 들어왔는지 확인
        {
            isPlayerInside = true;
            player = other.gameObject; // 플레이어 객체 저장
            damageTimer = 0.0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // 플레이어가 나갔는지 확인
        {
            isPlayerInside = false;
            player = null;
        }
    }

    private void ApplyDamage()
    {
        if (player != null)
        {
            player.GetComponent<PlayerStatus>().DecreaseHealth(damagePerSecond);
        }
    }



}


