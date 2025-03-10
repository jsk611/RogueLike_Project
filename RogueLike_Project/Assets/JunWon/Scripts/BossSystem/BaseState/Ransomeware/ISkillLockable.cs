using System.Collections.Generic;
using UnityEngine;

public interface ISkillLockable
{
    void SetSkillEnabled(SkillType skillType, bool enabled);

    bool IsSkillEnabled(SkillType skillType);

    void UnlockAllSkills();
}

public enum SkillType
{
    Running,
    Jumping,
    Dash,
    Movement,
    Shooting,
    WeaponSwitch,
    Interaction,
}