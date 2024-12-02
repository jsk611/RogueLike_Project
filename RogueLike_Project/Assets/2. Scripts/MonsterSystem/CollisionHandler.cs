using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private MonsterBase monsterBase;
    private string partName;
    private float damageMultiplier;

    public void Initialize(MonsterBase monster, string partName, float damageMultiplier)
    {
        this.monsterBase = monster;
        this.partName = partName;
        this.damageMultiplier = damageMultiplier;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            float baseDamage = 10f; // 기본 데미지 예시
            float finalDamage = baseDamage * damageMultiplier;

            Debug.Log($"Collision detected on {partName}. Damage: {finalDamage}");
            monsterBase?.TakeDamage(finalDamage);
        }
    }
}
