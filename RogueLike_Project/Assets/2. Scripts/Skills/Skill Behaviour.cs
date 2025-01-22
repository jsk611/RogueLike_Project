using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBehaviour : MonoBehaviour
{
    protected float recentSKillUsed;
    [Tooltip("CoolTime of Skill")]
    [SerializeField]
    protected float skillCoolTime = 10f;

    bool canUseSkill;

    public void SetSkillCoolTime(float time)
    {
        skillCoolTime = time;
    }

    public abstract void SkillActivation();

    protected bool CanActivateSkill()
    {
        float currentSkillUsed = Time.time;
        if (currentSkillUsed - recentSKillUsed >= skillCoolTime) return true;
        else
        {
            Debug.Log("CoolTime : "+(skillCoolTime-(currentSkillUsed-recentSKillUsed)));
            return false;
        }
    }
}
