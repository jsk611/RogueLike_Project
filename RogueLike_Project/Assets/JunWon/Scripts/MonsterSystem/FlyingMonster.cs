using System.Collections;
using UnityEngine;

public class FlyingMonster : MonsterBase
{
    //[Header("Flying Monster Settings")]
    //public float flyHeight = 5.0f; // ���� ����
    //private float chaseSpeed; // ���� �ӵ�
    //public float attackRange = 2.0f; // ���� ����
    //public float attackCooldown = 2.0f; // ���� ����
    //private float damage; // ���ݷ�
    //public float obstacleAvoidanceDistance = 5.0f; // ��ֹ� ȸ�� �Ÿ�
    //public float avoidanceDuration = 1.0f; // ȸ�� ���� �ð�

    //private FieldOfView fov; // �þ� ���� ������Ʈ
    //private bool isAvoiding = false; // ȸ�� ���� ����
    //private Vector3 avoidanceDirection; // ȸ�� ����

    //protected override void Start()
    //{
    //    fov = GetComponent<FieldOfView>(); // �þ� ���� ������Ʈ ��������
    //    target = GameObject.FindGameObjectWithTag("Player").transform; // Ÿ�� ����
    //    base.Start();

    //    chaseSpeed = monsterStatus.GetMovementSpeed();
    //    damage = monsterStatus.GetAttackDamage();
    //}

    //protected override IEnumerator StateMachine()
    //{
    //    while (hp > 0)
    //    {
    //        Debug.Log(state + " state Flying");
    //        switch (state)
    //        {
    //            case State.IDLE:
    //                yield return StartCoroutine(IDLE());
    //                break;
    //            case State.CHASE:
    //                yield return StartCoroutine(CHASE());
    //                break;
    //            case State.ATTACK:
    //                yield return StartCoroutine(ATTACK());
    //                break;
    //        }
    //    }
    //}

    //private IEnumerator IDLE()
    //{
    //    if (fov.visibleTargets.Count > 0)
    //    {
    //        target = fov.visibleTargets[0];
    //        ChangeState(State.CHASE);
    //    }
    //    else
    //    {
    //        target = null;
    //    }

    //    yield return new WaitForSeconds(0.5f);
    //}

    //private IEnumerator CHASE()
    //{
    //    if (target == null)
    //    {
    //        ChangeState(State.IDLE);
    //        yield break;
    //    }

    //    Vector3 directionToTarget = (target.position - transform.position).normalized;
    //    Vector3 targetPosition = target.position + Vector3.up * flyHeight;

    //    if (!isAvoiding)
    //    {
    //        // ��ֹ� ������ ���� Raycast
    //        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, obstacleAvoidanceDistance))
    //        {
    //            if (hit.collider.CompareTag("Obstacle"))
    //            {
    //                // ��ֹ��� ȸ���ϱ� ���� ���� ����
    //                avoidanceDirection = Vector3.Cross(directionToTarget, Vector3.up).normalized;
    //                isAvoiding = true;
    //                StartCoroutine(AvoidanceCooldown());
    //            }
    //        }
    //    }

    //    if (isAvoiding)
    //    {
    //        // ȸ�� ���� ����
    //        transform.position += avoidanceDirection * chaseSpeed * Time.deltaTime;
    //    }
    //    else
    //    {
    //        // ��ֹ��� ������ ��ǥ �������� �̵�
    //        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
    //    }

    //    // ��ǥ�� �ٶ󺸵��� ȸ��
    //    transform.rotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));

    //    // ���� ������ �����ϸ� ATTACK ���·� ��ȯ
    //    if (Vector3.Distance(transform.position, target.position) <= attackRange)
    //    {
    //        ChangeState(State.ATTACK);
    //    }

    //    yield return null;
    //}

    //private IEnumerator AvoidanceCooldown()
    //{
    //    yield return new WaitForSeconds(avoidanceDuration);
    //    isAvoiding = false; // ȸ�� ���� ����
    //}

    //private IEnumerator ATTACK()
    //{
    //    if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
    //    {
    //        target.GetComponent<PlayerStatus>().DecreaseHealth(damage * monsterStatus.CalculateCriticalHit());
    //    }

    //    yield return new WaitForSeconds(attackCooldown);

    //    if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
    //    {
    //        ChangeState(State.CHASE);
    //    }
    //}

    //public override void TakeDamage(float damage)
    //{
    //    base.TakeDamage(damage);
    //    if (hp <= 0)
    //    {
    //        Die();
    //    }
    //}

   
}
