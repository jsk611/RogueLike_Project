using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WanderingState_UnknownVirus : BaseState_UnknownVirus
{
    private float wanderTimer = 0f;
    private float changeDirectionInterval = 3f;
    private Vector3 wanderTarget;

    public WanderingState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("[WanderingState_UnknownVirus] Enter");
        SetNewWanderTarget();
        owner.Animator.SetBool("IsWalking", true);
    }

    public override void Update()
    {
        // Update wander timer
        wanderTimer += Time.deltaTime;

        // Change direction periodically
        if (wanderTimer >= changeDirectionInterval)
        {
            SetNewWanderTarget();
            wanderTimer = 0f;
        }

        // Move towards target
        if (owner.NmAgent.isOnNavMesh)
        {
            owner.NmAgent.SetDestination(wanderTarget);

            // Animation parameter based on speed
            float currentSpeed = owner.NmAgent.velocity.magnitude;
            owner.Animator.SetFloat("MoveSpeed", currentSpeed);
        }
    }

    public override void Exit()
    {
        Debug.Log("[WanderingState_UnknownVirus] Exit");
        owner.Animator.SetBool("IsWalking", false);
    }

    private void SetNewWanderTarget()
    {
        // Choose a random point around the player
        Vector3 direction = Random.insideUnitSphere;
        direction.y = 0;
        direction.Normalize();

        float distance = Random.Range(10f, 20f);
        Vector3 targetPosition = owner.Player.position + direction * distance;

        // Ensure position is on navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 15f, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
        }
        else
        {
            wanderTarget = owner.transform.position + direction * 5f;
        }
    }
}
