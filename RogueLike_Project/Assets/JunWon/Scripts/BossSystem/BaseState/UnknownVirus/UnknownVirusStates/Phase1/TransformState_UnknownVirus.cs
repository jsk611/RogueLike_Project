using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Combat State 진입");
    }

    public override void Update()
    {
        // 전투 상태에서는 현재 활성화된 보스 형태가 공격을 담당
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Combat State 종료");
    }
}