using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttackState_UnknownVirus : BossPhaseBase<UnknownVirusBoss>
{
    private bool isAttackFinished = false;
    private Coroutine attackCoroutine; // �ڷ�ƾ ���� ����

    public MapAttackState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        // ������ �ڽ��� �� ���� ���¸� �˰� ��
        owner.SetMapAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Map Attack State ����");
        VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
        if (vfx == null) Debug.Log("Can't find vfx");
        isAttackFinished = false;

        // �̵� ����
        owner.NmAgent.isStopped = true;
        owner.Animator.SetBool("IsMoving", false);

        attackCoroutine = owner.StartCoroutine(ExecuteSequentialAttack());

        // ���� �ִϸ��̼� & ȿ��
        owner.Animator.SetTrigger("MapAttack");
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Map Attack State ����");

        // ���� ���� ���� �ڷ�ƾ �ߴ�
        if (attackCoroutine != null)
        {
            owner.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // ť�� ���� ȿ�� �ߴ�
        StopCubeAttackEffect();

        owner.NmAgent.isStopped = false;

        owner.Animator.ResetTrigger("MapAttack");
    }

    public override void Interrupt()
    {
        base.Interrupt();

        Debug.Log("MapAttackState: ���� �ߴܵ�");

        // ���� ���� ���� ��� �ߴ�
        if (attackCoroutine != null)
        {
            owner.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // ť�� ���� ȿ�� ��� �ߴ�
        StopCubeAttackEffect();

        // ���� �Ϸ�� ǥ���Ͽ� ���� ��ȯ ���
        isAttackFinished = true;
    }

    /// <summary>
    /// ť�� ���� ȿ�� �ߴ�
    /// </summary>
    private void StopCubeAttackEffect()
    {
        if (owner.basic != null)
        {
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx != null)
            {
                vfx.StopEffect(); // ť�� ȿ�� �ߴ�
            }
        }
    }
    /// <summary>�ִϸ��̼� �̺�Ʈ�� ���� Ÿ�̸� ���� �� ȣ��</summary>
    public void OnAttackFinished()
    {
        if (isAttackFinished) return;
        isAttackFinished = true;
    }

    private IEnumerator ExecuteSequentialAttack()
    {
        if (owner.basic != null)
        {
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx == null)
                vfx = owner.basic.AddComponent<VirusCubeAttackEffect>();

            // 1. ���̷��� ť�� ������ ���� ����
            vfx.StartLaserAttack();

            // 2. ���� �Ϸ���� ��� (3.6��)
            yield return new WaitForSeconds(3.2f);

            // 3. �� ���� ����
            if (owner.AbilityManager.UseAbility("MapAttack"))
            {
                owner.TriggerMapAttack();
            }
        }

        // 4. ���� �Ϸ�
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
