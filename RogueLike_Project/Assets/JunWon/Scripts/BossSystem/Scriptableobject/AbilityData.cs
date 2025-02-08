using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityData", menuName = "Abilities/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string abilityName; // 스킬 이름
    public string abilityDescription; // 스킬 설명
    public float cooldown; // 쿨다운 시간 (초)

    public enum AbilityType { Active, Passive } // 스킬 유형 (Active: 사용 가능, Passive: 지속 효과)
    public AbilityType abilityType;

    public Sprite abilityIcon; // 스킬 아이콘

    // 액티브 스킬
    public enum TargetType { Single, Multi, Area } // 타겟 유형 (Single: 단일, Multi: 다수, Area: 범위)
    public TargetType targetType;
    public float damage; // 공격력
    public float range; // 사정거리

    // 패시브 스킬
    public enum BuffType { None, Attack, Defense, Speed } // 버프 유형
    public BuffType buffType;
    public float buffAmount; // 버프량

    public AnimationClip abilityAnimation; // 스킬 애니메이션 클립

    public GameObject effectPrefab; // 스킬 이펙트 프리팹

}