using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeaponSkill : WeaponSkillManager
{
    float recentSkillUsed;
    [Tooltip("Skill Cool Time")]
    [SerializeField]
    float skillCoolTime;

    int skillCount;

    Tazer tazer;

    [Tooltip("Skill Audio")]
    [SerializeField]
    AudioClip AudioClipSkill;
    // Start is called before the first frame update
    void Start()
    {
        recentSkillUsed = Time.time;
        skillCount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool CanActivateSkill()
    {
        
        float currentSkillUsed = Time.time;
        Debug.Log(currentSkillUsed - recentSkillUsed);
        if (currentSkillUsed - recentSkillUsed > skillCoolTime)
        {
            recentSkillUsed = currentSkillUsed;
            skillCount = 1;
            return true;
        }
        else return false;
    }
    public override void SetSkillCoolTime(float newCoolTime)
    {
        Debug.Log("new coolTime &{newcoolTime}");

        skillCoolTime = newCoolTime;
    }

    public override float GetSkillCoolTime() => skillCoolTime;
    
    public override void ResetSkillCount()
    {
        skillCount = 0;
    }

    public override int GetSkillCount() => skillCount;


    public override AudioClip GetAudioClipSkill() => AudioClipSkill;
}
