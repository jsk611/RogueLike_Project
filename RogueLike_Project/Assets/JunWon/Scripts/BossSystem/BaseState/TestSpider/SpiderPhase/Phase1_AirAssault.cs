using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1_AirAssault : State<SpiderPrime>
{
    public Phase1_AirAssault(SpiderPrime owner) : base(owner) { }
    Vector3 endPos;
    Vector3 curPos;
    Vector3 midPos;
    float glideTime = 0f;
    float aerialDuration = 2f;

    public bool isAttackFinished = false;
    // Start is called before the first frame update
    public override void Enter()
    {
        endPos = owner.Player.position;
        curPos = owner.transform.position;
        midPos = (endPos+curPos)/2+Vector3.up*20;

        glideTime = 0f;

        owner.transform.position = owner.Player.position + Vector3.up * 30;
     //   owner.NmAgent.isStopped = true;
    }
    public override void Update()
    {

        float elapsedTIme = glideTime / aerialDuration;
        if (elapsedTIme >= 1f || Vector3.Distance(owner.Player.position, owner.transform.position) <= 2f)
        {
            isAttackFinished = true;
        }
        //else
        //{
        //    midPos = Vector3.Lerp(midPos, endPos, elapsedTIme);
        //    curPos = Vector3.Lerp(owner.transform.position, midPos, elapsedTIme);

        //    owner.transform.position = Vector3.Lerp(
        //        curPos,
        //        midPos,
        //        elapsedTIme
        //    );
        //}
        glideTime += Time.deltaTime;
    }
    public override void Exit() {
        isAttackFinished = false;
    //    owner.NmAgent.isStopped = false;
    }
}
