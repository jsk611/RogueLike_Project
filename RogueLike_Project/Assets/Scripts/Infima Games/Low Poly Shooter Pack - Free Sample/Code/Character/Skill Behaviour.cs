using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBehaviour : MonoBehaviour
{


    float recentSKillUsed;
    [Tooltip("CoolTime of Skill")]
    [SerializeField]
    float skillCoolTime;

    bool canUseSkill;

    Transform currentWeapon;
    // Start is called before the first frame update
    void Start()
    {
        //currentWeapon = Inventory.ge();
    }

    // Update is called once per frame
    void Update()
    {
        canUseSkill = CanActivateSkill();
    }

    private void SkillActivation()
    {

    }

    private bool CanActivateSkill()
    {
        float currentSkillUsed = Time.time;
        if (currentSkillUsed - recentSKillUsed > skillCoolTime) return true;
        else return false;
    }
}
