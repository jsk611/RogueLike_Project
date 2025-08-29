using System;
using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class PlayerStatus : StatusBehaviour
{
    [Header("Player Status")]

    [Tooltip("Creature Health")]
    [SerializeField]
    private float Health;

    [Tooltip("Creature MaxHealth")]
    [SerializeField]
    private float MaxHealth;

    [Tooltip("Creature Stamina Regeneration Per Seconds")]
    [SerializeField]
    private float StaminaRegen;

    //[Tooltip("Creature Defence")]
    //[SerializeField]
    //private float Defence;

    //[Tooltip("Creature Damage Alleviation")]
    //[SerializeField]
    //private float DamageAlleviation = 0;

    [Tooltip("Creature CriticalRate")]
    [SerializeField]
    private float CriticalRate;

    [Tooltip("Creature CriticalDamage")]
    [SerializeField]
    private float CriticalDamage;

    //[Tooltip("Creature Effect Resistance")]
    //[SerializeField]
    //private float EffectResist = 0f;

    [Tooltip("Creature Movement Speed")]
    [SerializeField]
    private float MoveSpeed;

    private float OriginMoveSpeed;

    [Tooltip("Creature Reload Speed")]
    [SerializeField]
    private float ReloadSpeed = 100f;

    [Tooltip("Creature AttackDamage")]
    [SerializeField]
    private float Damage;

    [Tooltip("Creature Attack Speed")]
    [SerializeField]
    private float AttackSpeed = 100f;

    //[Tooltip("Creature Jump Power")]
    //[SerializeField]
    //private float JumpPower;

    [Header("Coins")]

    [Tooltip("Coin Owned")]
    [SerializeField]
    private int Coins;

    [Tooltip("Permanent Coin Owned")]
    [SerializeField]
    private int PermanentCoins;

    [Tooltip("Coin Acquisition Rate")]
    [SerializeField]
    private float CoinAcquisitionRate;

    [Tooltip("Permanent Coin Acquisition Rate")]
    [SerializeField]
    private float PermanentCoinAcquisitionRate; 

    [Header("Player Stamina")]
    [SerializeField] [Range(0,100)] float Stamina;



    
    CharacterBehaviour character;
    PlayerControl playerControl;
    private Animator characterAnimator;
    private Animator weaponAnimator;

    private static readonly int HashReloadSpeed = Animator.StringToHash("Reload Speed");
    private static readonly int HashAttackSpeed = Animator.StringToHash("Fire Speed");

    private PostProcessingManager postProcessingManager;

    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();

        playerControl = character.GetComponent<PlayerControl>();
        characterAnimator = character.GetPlayerAnimator();
        weaponAnimator = character.GetWeaponAnimator();
        OriginMoveSpeed = MoveSpeed;

        postProcessingManager = FindObjectOfType<PostProcessingManager>();

        SetMaxHealth(PermanentUpgradeManager.instance.upgradeData.Basic_HP);
        SetHealth(PermanentUpgradeManager.instance.upgradeData.Basic_HP);
        SetAttackDamage(PermanentUpgradeManager.instance.upgradeData.Basic_ATK);

        SetPermanentCoin(PermanentUpgradeManager.instance.upgradeData.CurrentDNA);
    }

    private void Update()
    {
        ///Only for Stamina
            Stamina = playerControl.Stamina;
            UIManager.instance.BarValueChange(1,100,Stamina);
    }

    // Current Health
    public override void DecreaseHealth(float damage)
    {
        Health -= damage;
        CheckDeath();
        UpdateUI();

        float intensity = damage / MaxHealth;

        // ���� ���¿� ���� �ٸ� ������ ������ ȿ���� �����մϴ�.
        if (currentCon == Condition.Frozen)
        {
            Debug.Log("Frozen");
            // ��: Frozen ���¶�� cyan ������ ������ ȿ���� ����
            postProcessingManager.DamagedEffect(intensity, Color.cyan);
        }
        else if (currentCon == Condition.Blazed)
        {
            // ��: Blazed ������ ���� ���ϴ� �� (���� ���, ��Ȳ��)
            postProcessingManager.DamagedEffect(intensity, new Color(1f, 0.5f, 0f)); // ��Ȳ��
        }
        else
        {
            // Ư���� ���°� �ƴ϶�� ���� ������ ȿ�� ���� (�⺻ DamagedEffect)
            postProcessingManager.DamagedEffect(intensity);
        }
    }

    private void CheckDeath()
    {
        if (Health <= 0)
        {
            Health = 0;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        PermanentUpgradeManager.instance.SaveData();
        StartCoroutine(UIManager.instance.DieBuffering());
    }

    private void UpdateUI()
    {
        UIManager.instance.BarValueChange(0, MaxHealth, Health);
    }

   
    public override void IncreaseHealth(float health)
    {
        Health += health * PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate;
        if (Health > MaxHealth) Health = MaxHealth;
        if (Health / MaxHealth > 0.33f)
        {
            postProcessingManager.ChangeVignetteColor(Color.white);
            postProcessingManager.ChangeChromaticAberrationActive(false);
        }
        UIManager.instance.BarValueChange(0, MaxHealth, Health);
    }
    public override void SetHealth(float health)
    { 
        Health = Mathf.Clamp(health,0,MaxHealth);
        if (health <= 0) Destroy(gameObject);

        UIManager.instance.BarValueChange(0, MaxHealth, Health);
    }

    // Max Health
    public override void IncreaseMaxHealth(float maxHealth)
    {
        MaxHealth += maxHealth;

        UIManager.instance.BarValueChange(0, MaxHealth, Health);
    }
    public override void DecreaseMaxHealth(float maxHealth)
    { 
        MaxHealth -= maxHealth;
        if (MaxHealth < 0) MaxHealth = 0;
        if (MaxHealth < Health) Health = MaxHealth;

        UIManager.instance.BarValueChange(0, MaxHealth, Health);
    }
    public override void SetMaxHealth(float maxHealth)
    { 
        MaxHealth = maxHealth;
        if (MaxHealth <0 ) MaxHealth = 0;

        UIManager.instance.BarValueChange(0, MaxHealth, Health);
    }

    // StaminaRegeneration
    public void IncreaseStaminaRegen(float staminaRegen)
    {
        StaminaRegen += staminaRegen;
    }
    public void DecreaseStaminaRegen(float staminaRegen)
    { 
        StaminaRegen -= staminaRegen;
        if (StaminaRegen < 0 ) StaminaRegen = 0;
    }
    public void SetStaminaRegen(float staminaRegen)
    {
        StaminaRegen = staminaRegen;
        if (StaminaRegen < 0) StaminaRegen = 0;
    }

    //// Defence
    //public override void IncreaseDefence(float defence)
    //{ Defence += defence; }
    //public override void DecreaseDefence(float defence)
    //{ Defence -= defence; }
    //public override void SetDefence(float defence)
    //{ Defence = defence; }

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

    public override float CalculateCriticalHit()
    {
        float ran = Random.Range(0, 100);
        if (ran < CriticalRate)
            return CriticalDamage / 100f;
        else
            return 1f;
    }

    //// Effect Resistance
    //public override void IncreaseEffectResist(float effectResist)
    //{
    //    EffectResist += effectResist;
    //    if (EffectResist > 100) effectResist = 100;
    //}
    //public override void DecreaseEffectResist(float effectResist)
    //{ 
    //    EffectResist -= effectResist; 
    //    if (EffectResist < 0) EffectResist = 0;
    //}
    //public override void SetEffectResist(float effectResist)
    //{
    //    EffectResist = effectResist;
    //    EffectResist = Mathf.Clamp(EffectResist, 0, 100);
    //}

    // Movement Speed
    public override void IncreaseMovementSpeed(float moveSpeed)
    { MoveSpeed += moveSpeed * PermanentUpgradeManager.instance.upgradeData.UTLUpgradeRate; }
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
        ReloadSpeed += reloadSpeed * PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate;
        StatusAnimatorChange(HashReloadSpeed, ReloadSpeed / 100f);
    }
    public override void DecreaseReloadSpeed(float reloadSpeed)
    {
        ReloadSpeed -= reloadSpeed;
        if (ReloadSpeed <= 0) ReloadSpeed = Mathf.Epsilon;
        StatusAnimatorChange(HashReloadSpeed, ReloadSpeed / 100f);
    }
    public override void SetReloadSpeed(float reloadSpeed)
    {
        ReloadSpeed = reloadSpeed;
        if (ReloadSpeed <= 0) ReloadSpeed = Mathf.Epsilon;
        StatusAnimatorChange(HashReloadSpeed, ReloadSpeed / 100f);
    }

    // Attack Damage
    public override void IncreaseAttackDamage(float attackDamage)
    { Damage += attackDamage*PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate; }
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
        AttackSpeed += attackSpeed * PermanentUpgradeManager.instance.upgradeData.ATKUpgradeRate;
        StatusAnimatorChange(HashAttackSpeed, AttackSpeed / 100f);
    }
    public override void DecreaseAttackSpeed(float attackSpeed)
    { 
        AttackSpeed -= attackSpeed;
        StatusAnimatorChange(HashAttackSpeed, AttackSpeed / 100f);
    }
    public override void SetAttackSpeed(float attackSpeed)
    {
        AttackSpeed = attackSpeed;
        StatusAnimatorChange(HashAttackSpeed, AttackSpeed / 100f);
    }

    // Damage Alleviation
    //public override void IncreaseDamageAlleviation(float alleviation)
    //{
    //    DamageAlleviation += alleviation; 
    //    if (DamageAlleviation >100) DamageAlleviation = 100;
    //}
    //public override void DecreaseDamageAlleviation(float alleviation)
    //{ DamageAlleviation -= alleviation; }
    //public override void SetAlleviation(float alleviation)
    //{ DamageAlleviation = alleviation; }

    //public override void IncreaseJumpPower(float jumpPower)
    //{ JumpPower += jumpPower; }
    //public override void DecreaseJumpPower(float jumpPower)
    //{
    //    jumpPower -= jumpPower; 
    //    if (JumpPower < 0) JumpPower = 0;
    //}
    //public override void SetJumpPower(float jumpPower)
    //{ 
    //    if (jumpPower < 0) jumpPower = 0;
    //    JumpPower = jumpPower; 
    //}

    //Coins
    public void IncreaseCoin(int coin)
    {   Coins += coin;
        UIManager.instance.dnaIncrease((int)(coin*PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate));
    }
    public void DecreaseCoin(int coin)
    { 
        Coins -= coin;
        UIManager.instance.dnaIncrease(-coin);
    }
    public void SetCoin(int coin)
    { Coins = coin; }

    //Permanent Coins
    public void IncreasePermanentCoin(int packet)
    { 
        PermanentCoins += packet;
        UIManager.instance.packetIncrease(PermanentCoins);
        PermanentUpgradeManager.instance.upgradeData.CurrentDNA = PermanentCoins;
    }
    public void DecreasePermanentCoin(int packet)
    { 
        PermanentCoins -= packet;
        UIManager.instance.packetIncrease(PermanentCoins);
        PermanentUpgradeManager.instance.upgradeData.CurrentDNA = PermanentCoins;
    }
    public void SetPermanentCoin(int coin)
    { 
        PermanentCoins = coin;
        UIManager.instance.PacketReset(PermanentCoins);
        PermanentUpgradeManager.instance.upgradeData.CurrentDNA = PermanentCoins;
    }

    public void IncreaseAcquisitionRate(float rate)
    { CoinAcquisitionRate += rate * PermanentUpgradeManager.instance.upgradeData.CoinAcquisitionRate; }
    public void DecreaseAcquisitionRate(float rate)
    { CoinAcquisitionRate -= rate; }
    public void SetAcquisitionRate(float rate)
    { CoinAcquisitionRate = rate; }

    public void IncreasePermanentAcquisitionRate(float rate)
    { PermanentCoinAcquisitionRate += rate; }
    public void DecreasePermanentAcquisitionRate(float rate)
    { PermanentCoinAcquisitionRate -= rate; }
    public void SetPermanentAcquisitionRate(float rate)
    { PermanentCoinAcquisitionRate = rate; }

    public override float GetHealth() => Health;
    public override float GetMaxHealth() => MaxHealth;
    public float GetStaminaRegen() => StaminaRegen;
  //  public override float GetDefence() => Defence;    
    public override float GetAttackDamage() => Damage;
    public override float GetAttackSpeed() => AttackSpeed / 100f;
    public override float GetCriticalDamage() => CriticalDamage;
    public override float GetCriticalRate() => CriticalRate;
    //public override float GetEffectResist() => EffectResist;
   // public override float GetDamageAlleviation() => DamageAlleviation;
    public override float GetMovementSpeed() => MoveSpeed;
    public float GetMoveSpeedOrigin() => OriginMoveSpeed;
    public override float GetReloadSpeed() => ReloadSpeed / 100f;
    //public override float GetJumpPower() => JumpPower;
    public int GetCoin() => Coins;
    public int GetPermanentCoin() => PermanentCoins;
    public float GetAcquisitionRate() => CoinAcquisitionRate;
    public float GetPermanentAcquisitionRate() => PermanentCoinAcquisitionRate;

    

    private void StatusAnimatorChange(int Id, float value)
    {
        weaponAnimator = character.GetWeaponAnimator();
        characterAnimator.SetFloat(Id, value);
        weaponAnimator.SetFloat(Id, value);
    }


}
