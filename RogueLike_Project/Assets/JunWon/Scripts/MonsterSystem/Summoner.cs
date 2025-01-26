using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Summoner : MonsterBase
{
    [Header("Field Settings")]
    [SerializeField] protected float maintainDistance = 10f;          // 플레이어와 유지할 최소 거리
    [SerializeField] protected float SummonTimeInterval = 5f;     // 필드 생성 간격(초)
    [SerializeField] protected float MaxSummonCount = 5;     
    protected float SummonTimer = 0f;                          // 타이머

    [SerializeField] private GameObject[] summonedEnemies;
    private List<GameObject> totalEnemies = new List<GameObject>();
    private int currentSummonCount = 0;

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

        SummonTimer += Time.deltaTime;
        if (SummonTimer >= SummonTimeInterval)
        {
            ChangeState(State.CAST);
            SummonTimer = 0;
        }

    }

    protected virtual void UpdateCast()
    {
        nmAgent.isStopped = true;
        nmAgent.speed = 0;

        if (!hasCast)
        { 
            if (currentSummonCount < MaxSummonCount)
            {
                Vector3 randomPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                GameObject enemy = Instantiate(summonedEnemies[Random.Range(0, summonedEnemies.Length)], transform.position + randomPosition, Quaternion.identity);
                enemy.GetComponent<MonsterBase>().summonedMonster = true;
                enemy.GetComponent<MonsterStatus>().SetMaxHealth(20);
                enemy.GetComponent<MonsterBase>().master = GetComponent<Summoner>();
                totalEnemies.Add(enemy);
                currentSummonCount++;
                hasCast = true;
            }
            
        }

        if (Time.time - lastTransitionTime >= 1f)
        {
            ChangeState(State.CHASE);
            hasCast = false; // 플래그 초기화
        }
    }
    private void OnDestroy()
    {
        foreach (GameObject enemy in totalEnemies)
        {
            if (enemy == null) continue;
            enemy.GetComponent<MonsterBase>().TakeDamage(9999, false);
        }
    }

    public void summonDead(GameObject obj)
    {
      currentSummonCount--;
        totalEnemies.Remove(obj);
    }

}
