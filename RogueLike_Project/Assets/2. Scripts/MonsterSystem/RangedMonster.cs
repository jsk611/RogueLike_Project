using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("Settings")]
    [SerializeField] private float fireRate = 1.5f; // �߻� �ӵ�
    [SerializeField] protected bool isHitScan = false; // ��Ʈ ��ĵ ����

    [SerializeField] protected float aimTime; // ���� �ð�: ��Ÿ���� 60%
    [SerializeField] protected float attackTime;
    public EnemyWeapon gun;

    protected override void Start()
    {
        base.Start();
        aimTime = attackCooldown * 0.6f;
        attackTime = attackCooldown * 0.8f;
    }
    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE); // Ÿ���� ������ ��� ���·� ��ȯ
            return;
        }

        nmAgent.isStopped = false; // �̵� Ȱ��ȭ
        nmAgent.speed = chaseSpeed;
        nmAgent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK); // ���� ���� �ȿ� ������ ���� ���·� ��ȯ
        }
    }

    protected override void UpdateAttack()
    {
        if (!IsTargetValid())
        {
            ChangeState(State.CHASE); // Ÿ���� ���ų� ������ ����� ���� ���·� ��ȯ
            ResetAttackTimer();
            return;
        }

        nmAgent.isStopped = true; // �̵� ����
        attackTimer += Time.deltaTime; // ���� Ÿ�̸� ����

        HandleAttackPhases();

        if (attackTimer >= attackCooldown)
        {
            ResetAttackTimer(); // ��Ÿ�� �Ϸ� �� Ÿ�̸� �ʱ�ȭ
        }
    }

    private void HandleAttackPhases()
    {
        float aimTime = attackCooldown * 0.6f; // ���� �ð�: ��Ÿ���� 60%
        float fireTime = attackCooldown * 0.8f; // �߻� �ð�: ��Ÿ���� 80%

        if (attackTimer <= aimTime)
        {
            HandleAim(); // ���� ó��
        }
        else if (attackTimer <= fireTime)
        {
            HandleFirePreparation(); // �߻� �غ�
        }
        else
        {
            HandleFire(); // ���� �߻�
        }
    }

    private void HandleAim()
    {
        SetAnimatorState(State.AIM); // AIM ���� �ִϸ��̼�
    }

    private void HandleFirePreparation()
    {
        SetAnimatorState(State.COOLDOWN); // �߻� �غ� �ִϸ��̼�
    }

    private void HandleFire()
    {
        SetAnimatorState(State.ATTACK); // ATTACK ���� �ִϸ��̼�
    }

    public virtual void FireEvent()
    {
        if (gun == null) return; // ���Ⱑ ������ �������� ����

        if (!isHitScan)
        {
            gun.Fire(); // �Ѿ� �߻�
            Debug.Log("Gun fired via Animation Event!");
        }
        else
        {
            gun.FireLaser(); // ��Ʈ ��ĵ ������� �߻�
            Debug.Log("Hit scan Activated");
        }
    }

    private bool IsTargetValid()
    {
        return target != null && Vector3.Distance(transform.position, target.position) <= attackRange && DetectedPlayer();
    }

    private void ResetAttackTimer()
    {
        attackTimer = 0f;
    }
}
