using System.Collections;
using UnityEngine;

public class FlyingMonster : MonsterBase
{
    [Header("Flying Monster Settings")]
    public float flyHeight = 5.0f; // 공중 높이
    private float chaseSpeed; // 추적 속도
    public float attackRange = 2.0f; // 공격 범위
    public float attackCooldown = 2.0f; // 공격 간격
    private float damage; // 공격력
    public float obstacleAvoidanceDistance = 5.0f; // 장애물 회피 거리
    public float avoidanceDuration = 1.0f; // 회피 지속 시간

    private FieldOfView fov; // 시야 각도 컴포넌트
    private bool isAvoiding = false; // 회피 상태 여부
    private Vector3 avoidanceDirection; // 회피 방향

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>(); // 시야 각도 컴포넌트 가져오기
        target = GameObject.FindGameObjectWithTag("Player").transform; // 타겟 지정
        base.Start();

        chaseSpeed = monsterStatus.GetMovementSpeed();
        damage = monsterStatus.GetAttackDamage();
    }

    protected override IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            Debug.Log(state + " state Flying");
            switch (state)
            {
                case State.IDLE:
                    yield return StartCoroutine(IDLE());
                    break;
                case State.CHASE:
                    yield return StartCoroutine(CHASE());
                    break;
                case State.ATTACK:
                    yield return StartCoroutine(ATTACK());
                    break;
            }
        }
    }

    private IEnumerator IDLE()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator CHASE()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            yield break;
        }

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 targetPosition = target.position + Vector3.up * flyHeight;

        if (!isAvoiding)
        {
            // 장애물 감지를 위한 Raycast
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, obstacleAvoidanceDistance))
            {
                if (hit.collider.CompareTag("Obstacle"))
                {
                    // 장애물을 회피하기 위한 방향 설정
                    avoidanceDirection = Vector3.Cross(directionToTarget, Vector3.up).normalized;
                    isAvoiding = true;
                    StartCoroutine(AvoidanceCooldown());
                }
            }
        }

        if (isAvoiding)
        {
            // 회피 동작 수행
            transform.position += avoidanceDirection * chaseSpeed * Time.deltaTime;
        }
        else
        {
            // 장애물이 없으면 목표 방향으로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
        }

        // 목표를 바라보도록 회전
        transform.rotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));

        // 공격 범위에 도달하면 ATTACK 상태로 전환
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK);
        }

        yield return null;
    }

    private IEnumerator AvoidanceCooldown()
    {
        yield return new WaitForSeconds(avoidanceDuration);
        isAvoiding = false; // 회피 상태 종료
    }

    private IEnumerator ATTACK()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            target.GetComponent<PlayerStatus>().DecreaseHealth(damage);
        }

        yield return new WaitForSeconds(attackCooldown);

        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (hp <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        base.Die();
        // 사망 애니메이션 재생 및 추가 로직
    }
}
