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
    private bool canAttack = true; // 공격 가능 여부
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
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
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
        while (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Vector3 targetPosition = target.position + Vector3.up * flyHeight;

            if (!isAvoiding)
            {
                // 장애물 감지를 위한 Raycast
                if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, obstacleAvoidanceDistance))
                {
                    if (hit.collider.CompareTag("Obstacle")) // 장애물 태그 확인
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
            Vector3 lookDirection = directionToTarget;
            transform.rotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));

            // 공격 범위에 도달하면 ATTACK 상태로 전환
            if (Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                ChangeState(State.ATTACK);
            }

            yield return null; // 다음 프레임까지 대기
        }
    }

    private IEnumerator AvoidanceCooldown()
    {
        yield return new WaitForSeconds(avoidanceDuration);
        isAvoiding = false; // 회피 상태 종료
    }

    private IEnumerator ATTACK()
    {
        if (canAttack)
        {
            canAttack = false;

            // 공격 애니메이션 재생
            anim.SetTrigger("Attack");

            // 플레이어에게 데미지 전달
            if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                target.GetComponent<PlayerStatus>().DecreaseHealth(damage);
                // 타겟에 데미지 주기 (플레이어에게 적용할 메서드 호출)
                // target.GetComponent<PlayerHealth>().TakeDamage(damage);
            }

            yield return new WaitForSeconds(attackCooldown); // 공격 쿨타임 대기
            canAttack = true;
        }

        // 공격 후 거리가 멀어지면 다시 추적
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
