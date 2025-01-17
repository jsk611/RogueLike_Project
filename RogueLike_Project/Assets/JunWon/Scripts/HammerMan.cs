using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
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

    private CapsuleCollider collider;

    protected override void Start()
    {
        base.Start();
        collider = GetComponent<CapsuleCollider>();
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

        //if (Vector3.Distance(transform.position, target.position) <= attackRange)
        //{
        //    ChangeState(State.ATTACK);
        //}

        TryJump();
    }

    private void TryJump()
    {
        if (isJumping || !canJump) return;

        isJumping = true;
        canJump = false;
        anim.SetBool("CanJump", canJump);
        StartCoroutine(MoveInAir());
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
            //if (Physics.SphereCast(transform.position, collider.radius - 0.06f;,-transform.up,0.2f,LayerMask.NameToLayer("Wall")))
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
        anim.SetBool("CanJump", canJump);
    }

    private void CreateShockwave(Vector3 position)
    {
        Instantiate(shockwavePrefab, position, Quaternion.identity);
    }
    IEnumerator MoveInAir()
    {
        while(isJumping)
        {
            Vector3 nav = (target.position-transform.position);
            nav.y = 0;
            
            rb.AddForce(nav*chaseSpeed);
            Debug.Log("mooooooving");
            yield return null;
        }
    }
}
