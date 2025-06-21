using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShootState_WormBoss : State<WormBossPrime>
{
    public ShootState_WormBoss(WormBossPrime owner) : base(owner) { }

    private float attackTimer = 0f;
    private float attackTime = 2f;
    private List<Transform> bodyList;
    private int bodyCount;
    public override void Enter()
    {
        bodyList = owner.GetComponent<WormBossBodyMovement>().BodyList;
        bodyCount = bodyList.Count;
        attackTimer = 0f;
        attackTime = 3f;
        owner.ShootToWander();
    }
    public override void Update()
    {
        owner.CoroutineRunner(FireWeapon());
        owner.ShootToWander();
    }
    public override void Exit()
    {
       
    }
    IEnumerator FireWeapon()
    {
        while(attackTime>0)
        {
            attackTimer+= Time.deltaTime;
            if (attackTimer >= 1f)
            {
                for (int i = 0; i< bodyList.Count; i++) 
                {
                    if (bodyList.Count != bodyCount) yield break;
                    Transform t = bodyList[i];
                    if (t == null) yield break;
                    t.GetComponent<EnemyWeapon>().Fire();
                    yield return new WaitForSeconds(0.2f);
                }
                attackTimer = 0f;
                attackTime--;
            }
            yield return null;
        }
        
    }
}
