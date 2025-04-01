using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class DigitalShadow : RangedMonster
{

    public delegate void ShadowDestroyedHandler(GameObject shadow);
    public event ShadowDestroyedHandler OnShadowDestroyed;

    [Header("Digital Shadow Repulsion Settings")]
    [SerializeField] private float repulsionDistance = 2.5f; // 다른 그림자와의 최소 거리
    [SerializeField] private float repulsionStrength = 3.0f; // 반발력 강도
    [SerializeField] private float jitterAmount = 0.5f;       // 약간의 랜덤 이동 범위

    // 플레이어를 향해 추격하는 동안 겹침 방지를 위해 UpdateChase 오버라이드
    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;

        // 주변의 다른 DigitalShadow들과의 반발력 계산
        Vector3 repulsion = CalculateRepulsion();

        // 약간의 랜덤 지터(자유로운 움직임 부여)
        Vector3 jitter = new Vector3(Random.Range(-jitterAmount, jitterAmount), 0, Random.Range(-jitterAmount, jitterAmount));

        // 최종 목적지는 플레이어 위치에 반발 벡터와 지터를 더한 값
        Vector3 destination = target.position + repulsion + jitter;
        nmAgent.SetDestination(destination);

        // 플레이어와 가까워지면 공격 상태로 전환
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK);
        }
    }

    // 주변의 다른 DigitalShadow와의 거리를 계산하여 반발 벡터를 반환
    private Vector3 CalculateRepulsion()
    {
        Vector3 repulsion = Vector3.zero;
        DigitalShadow[] shadows = FindObjectsOfType<DigitalShadow>();
        int count = 0;

        foreach (var shadow in shadows)
        {
            if (shadow == this) continue;
            float distance = Vector3.Distance(transform.position, shadow.transform.position);
            if (distance < repulsionDistance && distance > 0f)
            {
                // 거리가 가까울수록 강하게 밀어내도록 (역수로 계산)
                Vector3 pushDir = (transform.position - shadow.transform.position).normalized;
                repulsion += pushDir * (repulsionStrength / distance);
                count++;
            }
        }

        if (count > 0)
        {
            repulsion /= count;
        }

        return repulsion;
    }

    protected override void UpdateDie()
    {
        // 기존 죽음 처리 로직 실행
        base.UpdateDie();

        // 이벤트를 통해 보스에게 죽었음을 알림
        OnShadowDestroyed?.Invoke(gameObject);
    }
}

