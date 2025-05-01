using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_DIe_State : State<SpiderPrime>
{
    // Start is called before the first frame update
    public Phase1_DIe_State(SpiderPrime owner) : base(owner) { }

    float deathTIme = 1f;
    float elapsedTIme = 0f;
    public override void Enter()
    {
        owner.NmAgent.enabled = false;
        elapsedTIme = 0f;
    }
    public override void Update()
    {
        if (elapsedTIme < deathTIme)
        {
            owner.transform.Rotate(new Vector3(0, 0, 180) * Time.deltaTime);
            elapsedTIme+= Time.deltaTime;
        }
    }
    public override void Exit()
    {
        
    }
}
