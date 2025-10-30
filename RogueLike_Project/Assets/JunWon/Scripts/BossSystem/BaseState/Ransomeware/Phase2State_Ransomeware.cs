using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2State_Ransomeware : BossPhaseBase<Ransomware>
{
    public Phase2State_Ransomeware(Ransomware owner) : base(owner)
    { }

    private StateMachine<Ransomware> subFsm;

    private void InitializeAbility()
    {
        owner.AbilityManager.SetAbilityActive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityActive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityActive("DataBlink");
        owner.AbilityManager.SetMaxCoolTime("DataBlink");
        owner.AbilityManager.SetAbilityActive("Lock");
        owner.AbilityManager.SetMaxCoolTime("Lock");
    }
    private void InitializeStats()
    {
    }
    private void InitializeSubFSM()
    {
        var idleState = new Phase1_Idle_State(owner);
        var attackState = new Phase2_MeleeAttackState(owner);
        var rangedAttackState = new Phase2_RangedAttackState(owner);
        var chaseState = new Phase1_Chase_State(owner);
        var blinkState = new Phase2_DataBlink_State(owner);
        var lockState = new Phase2_RansomLock_State(owner);

        subFsm = new StateMachine<Ransomware>(idleState);

        // Idle���� Chase�� ��ȯ
        subFsm.AddTransition(new Transition<Ransomware>(
           idleState,
           chaseState,
           () => true
        ));

        // Ư�� ��� ��ȯ (�켱������ ����)
        // 1. DataBlink ��ȯ
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            blinkState,
            () => owner.AbilityManager.GetAbilityRemainingCooldown("DataBlink") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            blinkState,
            chaseState,
            () => blinkState.IsAnimationFinished()
        ));

        // 2. Lock ��ȯ
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            lockState,
            () => owner.AbilityManager.GetAbilityRemainingCooldown("Lock") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            lockState,
            chaseState,
            () => lockState.IsAnimationFinished() // ����: �������� blinkState�� ����ϰ� �־���
        ));

        // �⺻ ���� ���� ��ȯ (�켱������ ����)
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            attackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= owner.MeleeAttackRange &&
             owner.AbilityManager.GetAbilityRemainingCooldown("BasicMeeleAttack") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            attackState,
            chaseState,
            () => attackState.IsAnimationFinished()
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            rangedAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) > owner.MeleeAttackRange &&
                  Vector3.Distance(owner.transform.position, owner.Player.position) <= owner.RangedAttackRange &&
                    owner.AbilityManager.GetAbilityRemainingCooldown("BasicRangedAttack") == 0
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            rangedAttackState,
            chaseState,
            () => rangedAttackState.IsAnimationFinished()
        ));

    }



    public override void Enter()
    {
        Debug.Log("�������� ���� ������2 ����");
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase2"), 1);
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase1"), 1);
        InitializeStats();
        InitializeAbility();
        InitializeSubFSM();

    }

    public override void Update()
    {
        subFsm.Update();
    }

    public override void Exit()
    {
        // ������ ���� �� ����FSM�� ����
        if (subFsm != null && subFsm.CurrentState != null)
        {
            subFsm.CurrentState.Exit();
        }
        subFsm = null;

    }

    public override void Interrupt()
    {
        if (isInterrupted) return;
        isInterrupted = true;

        // ���� ���� ���� ���� ������Ʈ�� Interrupt ȣ��
        if (subFsm != null && subFsm.CurrentState != null)
        {
            subFsm.CurrentState.Interrupt();
        }

        // ������1 ���� ���� �۾�
        owner.AbilityManager.SetAbilityInactive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityInactive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityInactive("DataBlink");
        owner.AbilityManager.SetAbilityInactive("SummonShadows");
        owner.AbilityManager.SetAbilityInactive("Lock");

        owner.NmAgent.isStopped = true;
        owner.SetRotationLock(true);
    }
}
