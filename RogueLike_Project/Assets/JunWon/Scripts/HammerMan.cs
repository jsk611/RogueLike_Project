using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
    private TileManager tileManager;

    protected override void Start()
    {
        base.Start();
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        nmAgent.updatePosition = false;
        nmAgent.updateRotation = false;

        tileManager = FindAnyObjectByType<TileManager>();
        Physics.IgnoreLayerCollision(LayerMask.GetMask("Creature"), LayerMask.GetMask("Projectile"),true);
    }

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        TryJump();
    }

    private void TryJump()
    {
        if (isJumping || !canJump) return;

        isJumping = true;
        canJump = false;
        anim.SetBool("CanJump", canJump);
        
        // 점프 로직
        Vector3 dir = (target.position - transform.position).normalized;
        rb.AddForce(new Vector3(dir.x * jumpForce, 15.0f * rb.mass, dir.z * jumpForce), ForceMode.Impulse);
        StartCoroutine(MoveInAir());
        // 점프 쿨타임 시작
        Invoke(nameof(ResetJumpCooldown), jumpCooldown);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 착지 확인
        if (isJumping )//&& collision.gameObject.layer==LayerMask.GetMask("Wall"))
        {
            CreateShockwave(collision.contacts[0].point);
            OnLanding();
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
        RaycastHit hit;
        int wallLayerMask = LayerMask.GetMask("Wall"); // "Wall" 레이어 마스크 생성
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, wallLayerMask))
        {
            Tile tile = hit.transform.GetComponent<Tile>();
            if (tile != null)
            {
                int z = (int)transform.position.x / 2;
                int x = (int)transform.position.z / 2;
                Debug.Log(hit.transform.name);
                StartCoroutine(tileManager.CreateShockwave(z, x, 4,1));
                Collider[] boom = Physics.OverlapSphere(transform.position, 8, LayerMask.GetMask("Character"));
                if (boom.Length > 0)
                {
                    target.GetComponent<PlayerStatus>().DecreaseHealth(monsterStatus.GetAttackDamage());
                    StartCoroutine(target.GetComponent<PlayerControl>().AirBorne(target.position-transform.position));
                }
              //  StartCoroutine(tile.CreateShockwave());
            }
        }
    }

    IEnumerator MoveInAir()
    {
        while (isJumping)
        {
            Vector3 nav = (target.position - transform.position);
            nav.y = 0;

            rb.AddForce(nav * chaseSpeed);
            yield return null;
        }
    }
}