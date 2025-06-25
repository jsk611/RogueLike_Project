using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Phase1_DIe_State : State<SpiderPrime>
{
    // Start is called before the first frame update
    public Phase1_DIe_State(SpiderPrime owner) : base(owner) { }

    float deathTIme = 1f;
    float elapsedTIme = 0f;

    bool deadCounted = false;
    public override void Enter()
    {
        if (owner.NmAgent.enabled)
        {
            owner.NmAgent.enabled = false;
            Rigidbody rigidBody = owner.AddComponent<Rigidbody>();
            rigidBody.mass = 60f;
        }
        elapsedTIme = 0f;
    }
    public override void Update()
    {
        if (elapsedTIme < deathTIme)
        {
            owner.transform.Rotate(new Vector3(0, 0, 180) * Time.deltaTime*deathTIme);
        }
        if ((elapsedTIme > 6f || !owner.isBoss) && !deadCounted)
        {
            deadCounted = true;
            owner.EnemyCountData.enemyCount--;
            GameObject.Destroy(owner.gameObject, 0.2f);
        }
        elapsedTIme += Time.deltaTime;
    }
    public override void Exit()
    {
        
    }
}
