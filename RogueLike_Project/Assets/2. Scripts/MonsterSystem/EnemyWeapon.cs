using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] Transform firePoint; //발사 위치;
    public GameObject bulletPrefab;
    public MonsterStatus monsterStatus;

    // HitScan Method
    [SerializeField] LineRenderer lineRenderer;
    RaycastHit hitInfo;
    Vector3 fireDirection; // 발사 방향: 적의 정면 기준
    float laserRange = 100f; // 레이저 사거리
    Vector3 playerPos;
    private PlayerStatus player;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        // 기본 설정
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.enabled = false;

        player = FindObjectOfType<PlayerStatus>();
        Debug.Log(player.transform.position);
        

    }



    public void Fire()
    {
        playerPos = GameObject.Find("Player").transform.position;
        Quaternion spawnRotation = Quaternion.LookRotation(playerPos - firePoint.position);
        // 지정된 회전으로 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, spawnRotation);
        bullet.GetComponent<MProjectile>().SetBulletDamage(monsterStatus.GetAttackDamage()*monsterStatus.CalculateCriticalHit());
    }

    public void FireLaser()
    {
        // 레이캐스트를 위한 정보
        fireDirection = transform.forward; // 발사 방향: 적의 정면 기준
        laserRange = 100f; // 레이저 사거리

        // 레이캐스트 실행
        if (Physics.Raycast(firePoint.position, fireDirection, out hitInfo, laserRange))
        {
            Debug.Log($"Hit: {hitInfo.collider.name}");

            // 히트 대상이 플레이어인지 확인
            if (hitInfo.collider.CompareTag("Player"))
            {
                // 데미지 처리
                PlayerStatus playerStatus = hitInfo.collider.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    // 몬스터의 공격력을 기반으로 플레이어에게 데미지 전달
                    float damage = monsterStatus.GetAttackDamage() * monsterStatus.CalculateCriticalHit();
                    playerStatus.DecreaseHealth(damage);
                }
            }

        }
    }

    public void AimReady()
    {
        // 레이캐스트를 위한 정보
        fireDirection = transform.forward; // 발사 방향: 적의 정면 기준
        laserRange = 100f; // 레이저 사거리

        DrawLaserEffect(firePoint.position, firePoint.position + fireDirection * laserRange);
    }


    private void DrawLaserEffect(Vector3 start, Vector3 end)
    {
        // LineRenderer를 활용해 레이저 시각 효과 구현
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true; // LineRenderer 활성화
            lineRenderer.SetPosition(0, start); // 시작점
            lineRenderer.SetPosition(1, end);   // 끝점
            StartCoroutine(DisableLaser(lineRenderer, 2.1f)); // 0.1초 뒤 비활성화
        }
    }

    // 레이저 효과 비활성화
    private IEnumerator DisableLaser(LineRenderer lineRenderer, float duration)
    {
        yield return new WaitForSeconds(duration);
        lineRenderer.enabled = false;
    }
}

