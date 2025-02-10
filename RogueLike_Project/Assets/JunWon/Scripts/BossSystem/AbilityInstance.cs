using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance : MonoBehaviour
{
    public AbilityData data;
    private float lastUsedTime;

    bool isActive = false;

    public AbilityInstance(AbilityData data)
    {
        isActive = false;
        this.data = data;
        lastUsedTime = -data.cooldown;
    }

    #region Skill Activate Methon
    public void Activate()
    {
        isActive = true;
        Debug.Log(data.abilityName + " 스킬 활성화됨");
    }

    public void Deactivate()
    {
        isActive = false;
        Debug.Log(data.abilityName + " 스킬 비활성화됨");
    }
    #endregion

    // 스킬 사용 가능 여부 체크 (Time.unscaledTime 사용)
    public bool IsReady()
    {
        return Time.unscaledTime >= lastUsedTime + data.cooldown;
    }

    // 스킬 사용 처리 (쿨다운 갱신)
    public void Use()
    {
        if (!isActive)
        {
            Debug.Log(data.abilityName + " 스킬은 현재 비활성 상태입니다.");
            return;
        }

        if (IsReady())
        {
            lastUsedTime = Time.unscaledTime;
            Debug.Log(data.abilityName + " 스킬 사용!");
            // 추가 스킬 실행 로직
        }
    }

    // 남은 쿨다운 시간 반환
    public float GetRemainingCooldown()
    {
        float remain = (lastUsedTime + data.cooldown) - Time.unscaledTime;
        return remain > 0 ? remain : 0;
    }

    public void InitializeWithMaxCooldown()
    {
        lastUsedTime = Time.unscaledTime;
    }

    public float GetDmg()
    {
        return data.damage;
    }
}
