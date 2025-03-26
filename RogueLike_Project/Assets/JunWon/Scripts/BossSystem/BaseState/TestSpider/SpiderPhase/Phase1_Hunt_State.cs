using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase1_Hunt_State : State<SpiderPrime>
{
    NavMeshAgent agent;
    Transform target;
    public Phase1_Hunt_State(SpiderPrime owner) : base(owner) { }
    // Start is called before the first frame update
    void Start()
    {
        if (agent == null)
        {
            agent = owner.NmAgent;
            target = owner.Player;
        }
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
    }
}
