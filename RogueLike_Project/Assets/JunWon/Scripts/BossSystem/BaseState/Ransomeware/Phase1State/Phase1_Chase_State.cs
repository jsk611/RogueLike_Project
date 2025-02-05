using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase1_Chase_State : BossPhaseBase<Ransomware>
{
    private float timer = 0f;
    private float chaseDuration = 2f;

    public Phase1_Chase_State(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
        owner.NmAgent.isStopped = false;
        owner.NmAgent.speed = owner.MonsterStatus.GetMovementSpeed();
        owner.NmAgent.SetDestination(owner.Player.transform.position);
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (owner.NmAgent.isOnNavMesh && owner.Player != null)
        {
            owner.NmAgent.SetDestination(owner.Player.transform.position);
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not on NavMesh or Player is null.");
        }

        Debug.Log("[Phase1_Chase] Update");
    }
}
