using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    // 인스펙터에서 여러 AbilityData를 할당할 수 있도록 리스트로 관리
    [SerializeField] private List<AbilityData> abilitiesData;

    // 각 스킬을 이름으로 구분하여 관리
    private Dictionary<string, AbilityInstance> abilities;

    private void Awake()
    {
        abilities = new Dictionary<string, AbilityInstance>();
        foreach (var data in abilitiesData)
        {
            if (data != null && !abilities.ContainsKey(data.abilityName))
            {
                abilities.Add(data.abilityName, new AbilityInstance(data));
            }
        }
    }

    /// <summary>
    /// 스킬 사용 시도 (쿨다운 체크 포함)
    /// </summary>
    public bool UseAbility(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            AbilityInstance ability = abilities[abilityName];
            if (ability.IsReady())
            {
                ability.Use();
                return true;
            }
            else
            {
                Debug.Log(abilityName + " 스킬은 아직 쿨다운 중입니다. 남은 시간: " + ability.GetRemainingCooldown());
            }
        }
        else
        {
            Debug.LogWarning("AbilityManager에 " + abilityName + " 스킬이 등록되어 있지 않습니다.");
        }
        return false;
    }

    /// 특정 스킬의 남은 쿨다운 시간 반환
    public float GetAbilityRemainingCooldown(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            return abilities[abilityName].GetRemainingCooldown();
        }
        return 0;
    }

    /// 페이즈 변경 등으로 활성 스킬을 업데이트하고 싶다면 이 메서드를 활용할 수 있습니다.
    /// 예를 들어, 페이즈에 따라 특정 스킬을 비활성화/활성화할 수 있습니다.
    public void SetAbilityActive(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            // active 플래그를 AbilityData나 AbilityInstance 내에 추가하여 처리할 수 있습니다.
            // 예시로 AbilityData에 bool isActive를 추가한 뒤, 이를 활용할 수 있습니다.
            abilities[abilityName].Activate();
        }
    }
    public void SetAbilityInactive(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            abilities[abilityName].Deactivate();
        }
    }
    public float GetAbiltiyDmg(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            return abilities[abilityName].GetDmg();
        }
        return 0;
    }

    /// <summary>
    /// 특정 스킬(어빌리티)의 프리팹을 반환
    /// </summary>
    public GameObject GetAbilityPrefab(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            return abilities[abilityName].GetPrefab();
        }
        return null;
    }

    public void SetMaxCoolTime(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            abilities[abilityName].InitializeWithMaxCooldown();
        }

    }

}
