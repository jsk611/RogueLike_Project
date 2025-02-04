using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroState_Ransomeware : IntroState<Ransomware>
{
    public IntroState_Ransomeware(Ransomware owner) : base(owner)
    {
    }
    public override void Enter()
    {
        owner.GetComponent<Animator>().SetTrigger("Intro");
    }

    public override void Update()
    {
        // 애니메이션이 끝났다면 페이즈1으로 전환
        if (owner.IsIntroAnimFinished)
        {
        }
    }

    public override void Exit()
    {
        Debug.Log("[IntroState_Ransomeware] Exit");
    }

}
