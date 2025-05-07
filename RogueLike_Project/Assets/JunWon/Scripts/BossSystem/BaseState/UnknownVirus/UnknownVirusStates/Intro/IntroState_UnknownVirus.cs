using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroState_UnknownVirus : BaseState_UnknownVirus
{
    private float timer = 0f;
    private const float introDuration = 3f;

    public IntroState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Intro State 진입");
        timer = 0f;

        // 보스 초기 상태 설정
        owner.BossStatus.SetMovementSpeed(5f);
        owner.BossStatus.SetAttackDamage(20f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        // 인트로 시간이 지나면 자동으로 다음 상태로 전환 (트랜지션)
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Intro State 종료");
    }
}
