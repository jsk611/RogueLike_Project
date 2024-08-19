using InfimaGames.LowPolyShooterPack;
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

    [Tooltip("Creature Stamina Regeneration")]
    [SerializeField]
    private float StaminaRegen;

    [Tooltip("Creature Defence")]
    [SerializeField]
    private float Defence;

    [Tooltip("Creature Damage Alleviation")]
    [SerializeField]
    private float DamageAlleviation = 0;

    [Tooltip("Creature CriticalRate")]
    [SerializeField]
    private float CriticalRate;

    [Tooltip("Creature CriticalDamage")]
    [SerializeField]
    private float CriticalDamage;

    [Tooltip("Creature Effect Resistance")]
    [SerializeField]
    private float EffectResist = 0f;

    [Tooltip("Creature Movement Speed")]
    [SerializeField]
    private float MoveSpeed;

    [Tooltip("Creature Reload Speed")]
    [SerializeField]
    private float ReloadSpeed = 100f;

    [Tooltip("Creature AttackDamage")]
    [SerializeField]
    private float Damage;

    [Tooltip("Creature Attack Speed")]
    [SerializeField]
    private float AttackSpeed = 100f;

    [Tooltip("Creature Jump Power")]
    [SerializeField]
    private float JumpPower;


    CharacterBehaviour character;
    private Animator characterAnimator;
    private Animator weaponAnimator;

    private static readonly int HashReloadSpeed = Animator.StringToHash("Reload Speed");
    private static readonly int HashAttackSpeed = Animator.StringToHash("Fire Speed");
    private void Start()
    {
        character = ServiceLocator.Current.Get<GameModeService>().GetPlayerCharacter();
        characterAnimator = character.GetPlayerAnimator();
        weaponAnimator = character.GetWeaponAnimator();

    }

    // Current Health
    public override void DecreaseHealth(float damage)
    { 
        Health -= damage * (100f - DamageAlleviation / 100.0f) * Mathf.Pow(Mathf.Pow(0.5f,0.005f),Defence);
        if (Health < 0) Health = 0;
    }
    public override void IncreaseHealth(float health)
    {
        Health += health;
        if (Health > MaxHealth) Health = MaxHealth;
    }
    public override void SetHealth(float health)
    { 
        Health = Mathf.Clamp(health,0,MaxHealth);
    }

    // Max Health
    public override void IncreaseMaxHealth(float maxHealth)
    { MaxHealth += maxHealth; }
    public override void DecreaseMaxHealth(float maxHealth)
    { 
        MaxHealth -= maxHealth; 
        if (MaxHealth < 0) MaxHealth = 0;
    }
    public override void SetMaxHealth(float maxHealth)
    { 
        MaxHealth = maxHealth;
        if (MaxHealth <0 ) MaxHealth = 0;
    }

    // StaminaRegeneration
    public override void IncreaseStaminaRegen(float staminaRegen)
    { StaminaRegen += staminaRegen; }
    public override void DecreaseStaminaRegen(float staminaRegen)
    { 
        StaminaRegen -= staminaRegen;
        if (StaminaRegen < 0 ) StaminaRegen = 0;
    }
    public override void SetStaminaRegen(float staminaRegen)
    {
        StaminaRegen = staminaRegen;
        if (StaminaRegen < 0) StaminaRegen = 0;
    }

    // Defence
    public override void IncreaseDefence(float defence)
    { Defence += defence; }
    public override void DecreaseDefence(float defence)
    { Defence -= defence; }
    public override void SetDefence(float defence)
    { Defence = defence; }

    // Critical Rate
    public override void IncreaseCriticalRate(float criticalRate)
    { CriticalRate += criticalRate; }
    public override void DecreaseCriticalRate(float criticalRate)
    {
        CriticalRate -= criticalRate; 
        if (CriticalRate < 0 ) CriticalRate = 0;
    }
    public override void SetCriticalRate(float criticalRate)
    {
        CriticalRate = criticalRate;
        if (CriticalRate < 0) CriticalRate = 0;
    }

    // Critical Damage
    public override void IncreaseCriticalDamage(float criticalDamage)
    { CriticalDamage += criticalDamage; }
    public override void DecreaseCriticalDamage(float criticalDamage)
    { 
        CriticalDamage -= criticalDamage; 
        if (CriticalDamage < 0 ) CriticalDamage = 0;
    }
    public override void SetCriticalDamage(float criticalDamage)
    { 
        CriticalDamage = criticalDamage; 
        if (CriticalDamage < 0) CriticalDamage = 0;
    }

    // Effect Resistance
    public override void IncreaseEffectResist(float effectResist)
    {
        EffectResist += effectResist;
        if (EffectResist > 100) effectResist = 100;
    }
    public override void DecreaseEffectResist(float effectResist)
    { 
        EffectResist -= effectResist; 
        if (EffectResist < 0) EffectResist = 0;
    }
    public override void SetEffectResist(float effectResist)
    {
        EffectResist = effectResist;
        EffectResist = Mathf.Clamp(EffectResist, 0, 100);
    }

    // Movement Speed
    public override void IncreaseMovementSpeed(float moveSpeed)
    { MoveSpeed += moveSpeed; }
    public override void DecreaseMovementSpeed(float moveSpeed)
    {
        MoveSpeed -= moveSpeed;
        if (MoveSpeed < 0) MoveSpeed = 0;
    }
    public override void SetMovementSpeed(float moveSpeed)
    { 
        MoveSpeed = moveSpeed;
        if (MoveSpeed < 0) MoveSpeed = 0;
    }

    // Reload Speed
    public override void IncreaseReloadSpeed(float reloadSpeed)
    { 
        ReloadSpeed += reloadSpeed;
        characterAnimator.SetFloat(HashReloadSpeed,ReloadSpeed);
        weaponAnimator.SetFloat(HashReloadSpeed, ReloadSpeed);

    }
    public override void DecreaseReloadSpeed(float reloadSpeed)
    {
        ReloadSpeed -= reloadSpeed;
        if (ReloadSpeed <= 0) ReloadSpeed = Mathf.Epsilon;
        characterAnimator.SetFloat(HashReloadSpeed, ReloadSpeed);
        weaponAnimator.SetFloat(HashReloadSpeed, ReloadSpeed);
    }
    public override void SetReloadSpeed(float reloadSpeed)
    {
        ReloadSpeed = reloadSpeed;
        if (ReloadSpeed <= 0) ReloadSpeed = Mathf.Epsilon;
        characterAnimator.SetFloat(HashReloadSpeed, ReloadSpeed);
        weaponAnimator.SetFloat(HashReloadSpeed, ReloadSpeed);
    }

    // Attack Damage
    public override void IncreaseAttackDamage(float attackDamage)
    { Damage += attackDamage; }
    public override void DecreaseAttackDamage(float attackDamage)
    {
        Damage -= attackDamage;
        if (Damage < 0) Damage = 0;
    }
    public override void SetAttackDamage(float attackDamage)
    { 
        Damage = attackDamage;
        if (Damage < 0) Damage = 0;
    }

    // Attack Speed
    public override void IncreaseAttackSpeed(float attackSpeed)
    {
        AttackSpeed += attackSpeed; 
        characterAnimator.SetFloat(HashAttackSpeed, AttackSpeed);
        weaponAnimator.SetFloat(HashAttackSpeed, AttackSpeed);
    }
    public override void DecreaseAttackSpeed(float attackSpeed)
    { 
        AttackSpeed -= attackSpeed; 
        characterAnimator.SetFloat(HashAttackSpeed, AttackSpeed);
        weaponAnimator.SetFloat(HashAttackSpeed, AttackSpeed);
    }
    public override void SetAttackSpeed(float attackSpeed)
    {
        AttackSpeed = attackSpeed;
        characterAnimator.SetFloat(HashAttackSpeed, AttackSpeed);
        weaponAnimator.SetFloat(HashAttackSpeed, AttackSpeed);
    }

    // Damage Alleviation
    public override void IncreaseDamageAlleviation(float alleviation)
    {
        DamageAlleviation += alleviation; 
        if (DamageAlleviation >100) DamageAlleviation = 100;
    }
    public override void DecreaseDamageAlleviation(float alleviation)
    { DamageAlleviation -= alleviation; }
    public override void SetAlleviation(float alleviation)
    { DamageAlleviation = alleviation; }

    public override void IncreaseJumpPower(float jumpPower)
    { JumpPower += jumpPower; }
    public override void DecreaseJumpPower(float jumpPower)
    {
        jumpPower -= jumpPower; 
        if (JumpPower < 0) JumpPower = 0;
    }
    public override void SetJumpPower(float jumpPower)
    { 
        if (jumpPower < 0) jumpPower = 0;
        JumpPower = jumpPower; 
    }

    public override float GetHealth() => Health;
    public override float GetMaxHealth() => MaxHealth;
    public override float GetStaminaRegen() => StaminaRegen;
    public override float GetDefence() => Defence;
    public override float GetAttackDamage() => Damage;
    public override float GetAttackSpeed() => AttackSpeed;
    public override float GetCriticalDamage() => CriticalDamage;
    public override float GetCriticalRate() => CriticalRate;
    public override float GetEffectResist() => EffectResist;
    public override float GetDamageAlleviation() => DamageAlleviation;
    public override float GetMovementSpeed() => MoveSpeed;
    public override float GetReloadSpeed() => ReloadSpeed;
    public override float GetJumpPower() => JumpPower;
}
