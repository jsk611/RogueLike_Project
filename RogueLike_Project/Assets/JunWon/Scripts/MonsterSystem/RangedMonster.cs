using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("settings")]
    [SerializeField] float firerate = 1.5f;
    bool isFired = false;

    public EnemyWeapon gun;

    protected override void UpdateAttack()
    {
        nmAgent.isStopped = true;
        nmAgent.speed = 0f;

        // 공격 타이머 진행
        if (!isFired)
        {
            gun.Fire();
            isFired = true;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            // 공격 후 타겟이 범위를 벗어났다면 추적 상태로 전환
            if (Vector3.Distance(transform.position, target.position) > attackRange)
                ChangeState(State.CHASE);

            // 공격 타이머 초기화
            attackTimer = 0f;
            isFired = false;
        }
      
    }


  
}
