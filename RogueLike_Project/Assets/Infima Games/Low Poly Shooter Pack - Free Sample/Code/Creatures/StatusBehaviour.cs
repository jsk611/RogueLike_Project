using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class StatusBehaviour : MonoBehaviour
{
    public abstract void DecreaseHealth(float damage);
    public abstract void IncreaseHealth(float health);
    public abstract void SetHealth(float health);
    public abstract float GetHealth();

    public abstract void IncreaseMaxHealth(float maxHealth);
    public abstract void DecreaseMaxHealth(float maxHealth);
    public abstract void SetMaxHealth(float maxHealth);
    public abstract float GetMaxHealth();

    public abstract void IncreaseDefence(float defence);
    public abstract void DecreaseDefence(float defence);
    public abstract void SetDefence(float defence);
    public abstract float GetDefence();

    public abstract void IncreaseDamageAlleviation(float alleviation);
    public abstract void DecreaseDamageAlleviation(float alleviation);
    public abstract void SetAlleviation(float alleviation);
    public abstract float GetDamageAlleviation();

    public abstract void IncreaseCriticalRate(float criticalRate);
    public abstract void DecreaseCriticalRate(float criticalRate);
    public abstract void SetCriticalRate(float criticalRate);
    public abstract float GetCriticalRate();

    public abstract void IncreaseCriticalDamage(float criticalDamage);
    public abstract void DecreaseCriticalDamage(float criticalDamage);
    public abstract void SetCriticalDamage(float criticalDamage);
    public abstract float GetCriticalDamage();

    public abstract void IncreaseMovementSpeed(float moveSpeed);
    public abstract void DecreaseMovementSpeed(float moveSpeed);
    public abstract void SetMovementSpeed(float moveSpeed);
    public abstract float GetMovementSpeed();

    public abstract void IncreaseAttackDamage(float attackDamage);
    public abstract void DecreaseAttackDamage(float attackDamage);
    public abstract void SetAttackDamage(float attackDamage);
    public abstract float GetAttackDamage();

    public abstract void IncreaseJumpPower(float jumpPower);
    public abstract void DecreaseJumpPower(float jumpPower);
    public abstract void SetJumpPower(float jumpPower);
    public abstract float GetJumpPower();
}
