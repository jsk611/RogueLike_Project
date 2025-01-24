using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FieldMage : MonsterBase
{
    [Header("Field Settings")]
    [SerializeField] protected float maintainDistance = 10f;          // 플레이어와 유지할 최소 거리
    [SerializeField] protected float fieldSpawnInterval = 5f;     // 필드 생성 간격(초)
    protected float fieldSpawnTimer = 0f;                          // 타이머

    [SerializeField] private GameObject debuffFieldPrefab;         // 플레이어 위치에 깔 디버프 필드
    [SerializeField] private GameObject buffFieldPrefab;           // 몬스터 위치에 깔 버프 필드

    bool hasCast = false;


    [SerializeField]
    private LayerMask monsterLayer; // 몬스터 Layer
    [SerializeField]
    private LayerMask playerLayer; // 플레이어 Layer


    protected override void Start()
    {
        base.Start();
        // stateActions에 CAST 상태와 연관 메서드 등록
        stateActions[State.CAST] = UpdateCast;
        stateActions.Remove(State.ATTACK);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"),LayerMask.NameToLayer("IgnorePlayerCollision"));
    }

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        if (distanceToPlayer <= maintainDistance)
        {
            nmAgent.isStopped = true;
            nmAgent.speed = 0;
        }
        else
        {
            nmAgent.isStopped = false;
            nmAgent.speed = chaseSpeed;
            nmAgent.SetDestination(target.position);
        }

        fieldSpawnTimer += Time.deltaTime;
        if (fieldSpawnTimer >= fieldSpawnInterval)
        {
            ChangeState(State.CAST);
            fieldSpawnTimer = 0;
        }

    }

    private void UpdateCast()
    {
        nmAgent.isStopped = true;
        nmAgent.speed = 0;

        if (!hasCast)
        {
            if (Random.Range(0, 2) == 0) // 0 또는 1 생성 (50% 확률)
            {
                PlaceDebuffField();
            }
            else
            {
                PlaceBuffField();
            }

            hasCast = true;
        }

        if (Time.time - lastTransitionTime >= 1f)
        {
            ChangeState(State.CHASE);
            hasCast = false; // 플래그 초기화
        }
    }
    protected virtual void PlaceDebuffField()
    {
        if (debuffFieldPrefab != null && target != null)
        {
            // 플레이어 위치에 디버프 필드를 생성
            Instantiate(debuffFieldPrefab, target.position, Quaternion.identity);
        }
    }

    protected virtual void PlaceBuffField()
    {
        if (buffFieldPrefab != null)
        {
            // 1. 주변 몬스터 탐색 (반경 10 유닛)
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, monsterLayer); // 몬스터 레이어
            Debug.Log("마법사 영역내에 " + colliders.Length + "마리 ");

            // 2. 체력이 가장 낮은 몬스터 찾기 (자신 포함)
            MonsterStatus lowestHealthMonster = null;
            float lowestHealth = float.MaxValue;

            foreach (Collider collider in colliders)
            {
                MonsterStatus monster = collider.GetComponent<MonsterStatus>();
                if (monster != null)
                {
                    if (monster.GetHealth() < lowestHealth)
                    {
                        lowestHealth = monster.GetHealth();
                        lowestHealthMonster = monster;
                    }
                }
            }

            // 자신도 포함
            if (monsterStatus.GetHealth() < lowestHealth)
            {
                lowestHealthMonster = monsterStatus;
            }

            // 3. 찾은 몬스터 또는 자신의 위치에 버프 필드 생성
            if (lowestHealthMonster != null)
            {
                Debug.Log("버프 생성!!");
                Instantiate(buffFieldPrefab, lowestHealthMonster.transform.position, Quaternion.identity);
            }
        }
    }
}
