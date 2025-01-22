using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSkill : SkillBehaviour
{
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        Debug.Log("Skill activated");
        recentSKillUsed = Time.time;
    }
    
}
