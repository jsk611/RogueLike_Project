using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : StatusBehaviour
{
    [Tooltip("Creature Health")]
    [SerializeField]
    private float Health;

    [Tooltip("Creature MaxHealth")]
    [SerializeField]
    private float MaxHealth;

    [Tooltip("Creature Defence")]
    [SerializeField]
    private float Defence;

    [Tooltip("Creature Damage Alleviation")]
    [SerializeField]
    private float DamageAlleviation;

    [Tooltip("Creature CriticalRate")]
    [SerializeField]
    private float CriticalRate;

    [Tooltip("Creature CriticalDamage")]
    [SerializeField]
    private float CriticalDamage;

    [Tooltip("Creature MovementSpeed")]
    [SerializeField]
    private float MoveSpeed;

    [Tooltip("Creature AttackDamage")]
    [SerializeField]
    private float Damage;




    // Start is called before the first frame update
    public override void DecreaseHealth(float damage)
    { Health -= damage * (DamageAlleviation / 100.0f) * Mathf.Pow(Mathf.Pow(0.5f,0.005f),Defence); }
    public override void IncreaseHealth(float health)
    { Health += health; }
    public override void SetHealth(float health)
    { Health = health; }

    public override void IncreaseMaxHealth(float maxHealth)
    { MaxHealth += maxHealth; }
    public override void DecreaseMaxHealth(float maxHealth)
    { MaxHealth -= maxHealth; }
    public override void SetMaxHealth(float maxHealth)
    { MaxHealth = maxHealth; }

    public override void IncreaseDefence(float defence)
    { Defence += defence; }
    public override void DecreaseDefence(float defence)
    { Defence -= defence; }
    public override void SetDefence(float defence)
    { Defence = defence; }

    public override void IncreaseCriticalRate(float criticalRate)
    { CriticalRate += criticalRate; }
    public override void DecreaseCriticalRate(float criticalRate)
    { CriticalRate -= criticalRate; }
    public override void SetCriticalRate(float criticalRate)
    { CriticalRate = criticalRate; }

    public override void IncreaseCriticalDamage(float criticalDamage)
    { CriticalDamage += criticalDamage; }
    public override void DecreaseCriticalDamage(float criticalDamage)
    { CriticalDamage -= criticalDamage; }
    public override void SetCriticalDamage(float criticalDamage)
    { CriticalDamage = criticalDamage; }

    public override void IncreaseMovementSpeed(float moveSpeed)
    { MoveSpeed += moveSpeed; }
    public override void DecreaseMovementSpeed(float moveSpeed)
    { MoveSpeed -= moveSpeed; }
    public override void SetMovementSpeed(float moveSpeed)
    { MoveSpeed = moveSpeed; }

    public override void IncreaseAttackDamage(float attackDamage)
    { Damage += attackDamage; }
    public override void DecreaseAttackDamage(float attackDamage)
    { Damage -= attackDamage; }
    public override void SetAttackDamage(float attackDamage)
    { Damage = attackDamage; }

    public override void IncreaseDamageAlleviation(float alleviation)
    { DamageAlleviation += alleviation; }
    public override void DecreaseDamageAlleviation(float alleviation)
    { DamageAlleviation -= alleviation; }
    public override void SetAlleviation(float alleviation)
    { DamageAlleviation = alleviation; }


    public override float GetHealth() => Health;
    public override float GetMaxHealth() => MaxHealth;
    public override float GetDefence() => Defence;
    public override float GetAttackDamage() => Damage;
    public override float GetCriticalDamage() => CriticalDamage;
    public override float GetCriticalRate() => CriticalRate;
    public override float GetDamageAlleviation() => DamageAlleviation;
    public override float GetMovementSpeed() => MoveSpeed;
}
