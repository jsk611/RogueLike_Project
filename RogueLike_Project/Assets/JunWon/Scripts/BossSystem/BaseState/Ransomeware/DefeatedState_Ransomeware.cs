using System.Collections;
using System.Collections.Generic;
using Unity.Play.Publisher.Editor;
using UnityEngine;
using UnityEngine.AI;

public class DefeatedState_Ransomeware : State<Ransomware>
{
    private bool isAnimationFinished = false;
    private float deathEffectDelay = 0.5f;
    private float despawnDelay = 5f;

    public DefeatedState_Ransomeware(Ransomware owner) : base(owner)
    {
        owner.SetDefeatedState(this);
    }

    public override void Enter()
    {
        Debug.Log("[DefeatedState_Ransomeware] Enter");

        // NavMeshAgent ����
        if (owner.NmAgent != null)
        {
            owner.NmAgent.isStopped = true;
            owner.NmAgent.enabled = false;
        }

        // �ݶ��̴� ��Ȱ��ȭ (������)
        Collider[] colliders = owner.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // �ִϸ��̼� ����
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Dead"), 1.0f);
        owner.Animator.SetTrigger("Dead");

        // ���� ����Ʈ ��� (������ ����)
        owner.StartCoroutine(PlayDeathEffectWithDelay());

        // ���� ������ ��� (������)
        DropRewards();
    }

    public override void Update()
    {
        // �ʿ��� ��� ������Ʈ ����
    }

    public override void Exit()
    {
        Debug.Log("[DefeatedState_Ransomeware] Exit");
    }

    public void OnDeathAnimationFinished()
    {
        Debug.Log("[DefeatedState_Ransomeware] ���� �ִϸ��̼� �Ϸ�");
        isAnimationFinished = true;

        // ���� ���
        DropRewards();

        // ������Ʈ ��Ȱ��ȭ
        DisableRansomware();
    }

    private IEnumerator PlayDeathEffectWithDelay()
    {
        yield return new WaitForSeconds(deathEffectDelay);

        // ���� ����Ʈ ���
        //if (owner.DeathEffect != null)
        //{
        //    GameObject effect = GameObject.Instantiate(owner.DeathEffect, owner.transform.position, Quaternion.identity);
        //    // ���������� ����Ʈ ��ƼŬ ���� �� ��ġ ����
        //}

        // ������ ���� ȿ���� ���
        AudioSource audioSource = owner.GetComponent<AudioSource>();
        //if (audioSource != null && owner.DeathSound != null)
        //{
        //    audioSource.PlayOneShot(owner.DeathSound);
        //}
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);

        // �������� ���� ����
        GameObject.Destroy(owner.gameObject);
    }

    private void DropRewards()
    {
        // ���� ��� ����
        // ������ ȸ�� ������, ���׷��̵� Ű �� ���
    }

    private void DisableRansomware()
    {
        Debug.Log("[DefeatedState_Ransomeware] �������� ��Ȱ��ȭ");

        // �������� ���� ��Ȱ��ȭ
        GameObject.Destroy(owner.gameObject);

        // �ʿ��� ��� �߰� Ŭ���� �۾�
        // ��: Ư�� �Ŵ����� ���� óġ �˸� ��
    }

    public bool IsAnimationFinished() => isAnimationFinished;
}