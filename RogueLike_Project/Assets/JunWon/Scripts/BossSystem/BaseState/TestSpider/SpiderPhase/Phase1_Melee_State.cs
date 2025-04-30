using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Phase1_Melee_State : State<SpiderPrime>
{
    public Phase1_Melee_State(SpiderPrime owner) : base(owner) { }
    // Start is called before the first frame update
    private bool attackFinished = false;
    public bool IsAttackFinished => attackFinished;

    private float attackTime = 0f;

    Transform spiderHead;
    Vector3 originScale;
    bool isAttacking = true;
    float expansionTIme = 0.2f;
    float shrinkingTime = 0.6f;
    public override void Enter()
    {
        if (spiderHead == null)
        {
            spiderHead = owner.HeadWeapon.transform;
            originScale = spiderHead.localScale;
        }
        attackTime = 0f;
        isAttacking = true;
    }
    public override void Update()
    {
        if (isAttacking) Expansion();
        else Shrink();
        attackTime += Time.deltaTime;
    }


    public override void Exit()
    {
        attackFinished = false;
    }
    void Expansion()
    {
        float elapsedTime = attackTime / expansionTIme;
        spiderHead.localScale += Vector3.one * 0.2f;
        if (elapsedTime > 1f)
        {
            attackTime = 0f;
            isAttacking = false;
        }
    }
    void Shrink()
    {
        float elaspedTime = attackTime / shrinkingTime;
        spiderHead.localScale = Vector3.Lerp(spiderHead.localScale, originScale, elaspedTime);
        if (elaspedTime > 1f) {
            spiderHead.localScale = originScale;
            attackFinished = true;
        }
    }
}
