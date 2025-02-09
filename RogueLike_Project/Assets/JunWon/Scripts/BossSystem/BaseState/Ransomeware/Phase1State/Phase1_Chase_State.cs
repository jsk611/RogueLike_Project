using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Chase_State : BossPhaseBase<Ransomware>
{
    public Phase1_Chase_State(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
        owner.NmAgent.isStopped = false;
        owner.NmAgent.speed = owner.MonsterStatus.GetMovementSpeed();
        owner.Animator.SetBool("IsMoving", true);
        owner.NmAgent.SetDestination(owner.Player.transform.position);
    }

    public override void Update()
    {
        if (owner.NmAgent.isOnNavMesh && owner.Player != null)
        {
            owner.NmAgent.SetDestination(owner.Player.transform.position);

            // 이동 속도에 따른 애니메이션 파라미터 조절
            float currentSpeed = owner.NmAgent.velocity.magnitude;
            owner.Animator.SetFloat("MoveSpeed", currentSpeed);
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not on NavMesh or Player is null.");
        }
    }

    public override void Exit()
    {
        owner.Animator.SetBool("IsMoving", false);
        owner.Animator.SetFloat("MoveSpeed", 0f);
    }
}
