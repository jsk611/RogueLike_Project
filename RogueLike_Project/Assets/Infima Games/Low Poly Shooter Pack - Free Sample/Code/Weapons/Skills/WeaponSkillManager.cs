using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class WeaponSkillManager : MonoBehaviour
{

    protected float recentSkillUsed;
    [Tooltip("Skill Cool Time")]
    [SerializeField]
    public float skillCoolTime;

    [Tooltip("Max Skill Count")]
    [SerializeField]
    public int maxSkillCount;

    [Tooltip("Skill Audio")]
    [SerializeField]
    AudioClip AudioClipSkill;
    
    private int skillCount;
    protected void Awake()
    {
        recentSkillUsed = Time.time;
        skillCount = maxSkillCount;
    }

    public void SetSkillCoolTime(float newCoolTime)
    { skillCoolTime = newCoolTime; }

    public  bool CanActivateSkill()
    {

        float currentSkillUsed = Time.time;
        if (currentSkillUsed - recentSkillUsed > skillCoolTime)
        {
            recentSkillUsed = currentSkillUsed;
            if (skillCount < maxSkillCount)
                skillCount += 1;

        }
        if (skillCount > 0) return true;
        else return false;
    }
    public float GetSkillCoolTime()
    { return skillCoolTime; }

    public void DecreaseSkillCount()
    { skillCount -= 1; }

    public void IncreaseSkillCount()
    { skillCount += 1; }

    public int GetSkillCount()
    { return skillCount; }

    public AudioClip GetAudioClipSkill()
    { return AudioClipSkill; }

    public abstract void FireSkill();
}
