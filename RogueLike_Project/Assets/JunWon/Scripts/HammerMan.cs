using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class HammerMan : MonsterBase
{
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public GameObject shockwavePrefab;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private float jumpCooldown = 1f;
    private bool canJump = true;
    private Rigidbody rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        nmAgent.updatePosition = false;
        nmAgent.updateRotation = false;
    }

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK);
        }

        TryJump();
    }

    private void TryJump()
    {
        if (isJumping || !canJump) return;

        isJumping = true;
        canJump = false;

        // 점프 로직
        Vector3 dir = (target.position - transform.position).normalized;
        Debug.Log("direction: " +  "{" + dir.x + " " + dir.y + " " + dir.z + "}");
        rb.AddForce(new Vector3(dir.x * jumpForce, 15.0f, dir.z * jumpForce), ForceMode.Impulse);

        // 점프 쿨타임 시작
        Invoke(nameof(ResetJumpCooldown), jumpCooldown);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 착지 확인
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            OnLanding();
        }

        if (shockwavePrefab != null && collision.gameObject.layer == groundLayer)
        {
            CreateShockwave(collision.contacts[0].point);
        }
    }

    void OnLanding()
    {
        // 착지 시 호출
        isJumping = false;
    }

    private void ResetJumpCooldown()
    {
        canJump = true;
    }

    private void CreateShockwave(Vector3 position)
    {
        Instantiate(shockwavePrefab, position, Quaternion.identity);
    }
}
