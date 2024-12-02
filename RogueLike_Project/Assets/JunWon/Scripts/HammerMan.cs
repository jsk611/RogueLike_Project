using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HammerMan : MonsterBase
{

    [Header("HammerMan Settings")]
    [SerializeField] private float jumpForce = 15f; // 점프 힘
    [SerializeField] private float jumpCooldown = 2f; // 점프 쿨타임
    [SerializeField] private float shockwaveRadius = 5f; // 충격파 반경
    [SerializeField] private float shockwaveDamage = 20f; // 충격파 데미지
    [SerializeField] private LayerMask groundLayer; // 충격파 데미지를 받을 레이어

    private NavMeshPath navPath; // NavMesh 경로
    private Rigidbody rb;
    [SerializeField] bool isJumping = false; // 점프 중인지 확인
    private bool canJump = true; // 점프 가능 상태 확인

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        navPath = new NavMeshPath();
    }

    protected override void UpdateIdle()
    {
        CheckPlayer();
    }

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        if (!isJumping && canJump)
        {
            JumpTowardsTarget();
        }
    }

    protected override void UpdateAttack()
    {
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
            return;
        }

        if (!isJumping && canJump)
        {
            JumpTowardsTarget();
        }
    }

    private void JumpTowardsTarget()
    {
        if (NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navPath))
        {
            if (navPath.corners.Length > 1) // 유효한 경로가 있는 경우
            {
                Vector3 nextJumpPoint = navPath.corners[1];
                Vector3 jumpDirection = (nextJumpPoint - transform.position).normalized;

                rb.AddForce(new Vector3(jumpDirection.x, 1, jumpDirection.z) * jumpForce, ForceMode.Impulse);
                isJumping = true;
                canJump = false;

                StartCoroutine(JumpCooldown());
            }
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isJumping && collision.collider.CompareTag("Wall"))
        {
            isJumping = false;

            CreateShockWave();

            if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                ChangeState(State.ATTACK);
            }
            else
            {
                ChangeState(State.CHASE);
            }
        }
    }

    private void CreateShockWave()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, shockwaveRadius, groundLayer);
        foreach (var hit in hitTargets)
        {
            PlayerStatus player = hit.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.DecreaseHealth(shockwaveDamage);
            }
        }

        Debug.Log("Shockwave created!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);

        if (navPath != null && navPath.corners.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < navPath.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(navPath.corners[i], navPath.corners[i + 1]);
            }
        }
    }
}
