using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShootState_WormBoss : State<WormBossPrime>
{
    public ShootState_WormBoss(WormBossPrime owner) : base(owner) { }

    private float attackTimer = 0f;
    private float attackTime = 3f;
    List<Transform> bodyList;
    public override void Enter()
    {
        bodyList = owner.GetComponent<WormBossBodyMovement>().BodyList;
        attackTimer = 0f;
        attackTime = 3f;
        Debug.Log("attack!!!");
    }
    public override void Update()
    {
        attackTime -= Time.deltaTime;
        attackTimer += Time.deltaTime;
        if(attackTimer >= 1f)
        {
            attackTimer = 0f;
            foreach (Transform t in bodyList) 
            {
                t.GetComponent<EnemyWeapon>().Fire();
            }
        }
        if (attackTime <= 0f) owner.FlyToWander();
    }
    public override void Exit()
    {
        owner.FlyToWander();
    }
}
