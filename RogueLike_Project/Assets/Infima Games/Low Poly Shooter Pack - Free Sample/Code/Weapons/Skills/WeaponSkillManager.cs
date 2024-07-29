using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class WeaponSkillManager : MonoBehaviour
{

    

    public abstract void SetSkillCoolTime(float newCoolTime);

    public abstract bool CanActivateSkill();

    public abstract float GetSkillCoolTime();

    public abstract void ResetSkillCount();

    public abstract int GetSkillCount();

    public abstract AudioClip GetAudioClipSkill();
}
