using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStatus : StatusBehaviour
{
    [Header("Monster Status")]

    [Tooltip("Creature Health")]
    [SerializeField]
    private float Health;

    [Tooltip("Creature MaxHealth")]
    [SerializeField]
    private float MaxHealth;
    //

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
    private MonsterBase monsterBase;
    
    private EnemyHPBar HPBar;

    private MonsterAnimationEventHandler eventHandler;

    private Animator monsterAnimator;


    private void Start()
    {
        monsterBase = GetComponent<MonsterBase>();
        monsterAnimator = GetComponent<Animator>();
        eventHandler = GetComponent<MonsterAnimationEventHandler>();
        HPBar = monsterBase.HPBar;
    }

    // Current Health
    public override void DecreaseHealth(float damage) {
        Health -= damage; //* (100f - DamageAlleviation / 100.0f) * Mathf.Pow(Mathf.Pow(0.5f,0.005f),Defence);
        
        if (Health <= 0)
        {
            Health = 0;
            //Destroy(gameObject);
            monsterBase.TakeDamage(1000,false,true);
        }
        HPBar.SetRatio(GetHealth(), GetMaxHealth());
    }
    public override void IncreaseHealth(float health)
    {
        Health += health;
        if (Health > MaxHealth) Health = MaxHealth;
        HPBar.SetRatio(GetHealth(), GetMaxHealth());
    }
    public override void SetHealth(float health)
    {
        Health = Mathf.Clamp(health, 0, MaxHealth);
        //if (health <= 0) Destroy(gameObject);
        if(HPBar) HPBar.SetRatio(GetHealth(), GetMaxHealth());
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
        if (MaxHealth < 0) MaxHealth = 0;
        if (Health > MaxHealth) Health = MaxHealth;
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
        if (CriticalRate < 0) CriticalRate = 0;
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
        if (CriticalDamage < 0) CriticalDamage = 0;
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
            return 1;
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

    // Movement Speeds
    public override void IncreaseMovementSpeed(float moveSpeed)
    { MoveSpeed += moveSpeed; }
    public override void DecreaseMovementSpeed(float moveSpeed)
    {
        MoveSpeed -= moveSpeed;
        if (MoveSpeed < 0) MoveSpeed = 0;
    }
    public override void SetMovementSpeed(float moveSpeed)
    {
        if (currentCC == CC.normal)
        {
            MoveSpeed = moveSpeed;
            if (MoveSpeed < 0) MoveSpeed = 0;
        }
    }

    // Reload Speed
    public override void IncreaseReloadSpeed(float reloadSpeed) { ReloadSpeed += reloadSpeed; }
    public override void DecreaseReloadSpeed(float reloadSpeed)
    {
        ReloadSpeed -= reloadSpeed;
        if (ReloadSpeed <= 0) ReloadSpeed = Mathf.Epsilon;
    }
    public override void SetReloadSpeed(float reloadSpeed)
    {
        ReloadSpeed = reloadSpeed;
        if (ReloadSpeed <= 0) ReloadSpeed = Mathf.Epsilon;
    }

    // Attack Damage
    public override void IncreaseAttackDamage(float attackDamage)
    { Damage *= 1 + attackDamage/100; }
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
    }
    public override void DecreaseAttackSpeed(float attackSpeed)
    {
        AttackSpeed -= attackSpeed;
    }
    public override void SetAttackSpeed(float attackSpeed)
    {
        AttackSpeed = attackSpeed;
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

    


    public override float GetHealth() => Health;
    public override float GetMaxHealth() => MaxHealth;

    //  public override float GetDefence() => Defence;
    public override float GetAttackDamage() => Damage;
    public override float GetAttackSpeed() => AttackSpeed / 100f;
    public override float GetCriticalDamage() => CriticalDamage;
    public override float GetCriticalRate() => CriticalRate;
    //public override float GetEffectResist() => EffectResist;
    // public override float GetDamageAlleviation() => DamageAlleviation;
    public override float GetMovementSpeed() => MoveSpeed;
    public override float GetReloadSpeed() => ReloadSpeed / 100f;
    //public override float GetJumpPower() => JumpPower;

   

    public override IEnumerator Shocked(float damage, float duration, float probability,float interval, float shockTime)
    {
        eventHandler.SetShockTime(duration/10);
        float startTime = 0;
        float latest = 0;
 
        currentCon = Condition.Shocked;
        while (startTime < duration)
        {
            if (latest >= interval && Random.Range(1f, 100f) <= probability)
            {
                latest = 0;
                monsterBase.TakeDamage(damage);
            }
            latest += Time.deltaTime;
            startTime += Time.deltaTime;
            yield return null;
        }
        currentCon = Condition.normal;
        currentCC = CC.normal;
    }
    //public override IEnumerator Poisoned(float effect, float duration, float interval)
    //{
    //    if (currentCon == Condition.Poisoned) yield break;
    //    currentCon = Condition.Poisoned;
    //    float startTime = 0;
    //    while (startTime < duration)
    //    {
    //        DecreaseHealth(effect);
    //        yield return new WaitForSeconds(interval);
    //        startTime += interval;
    //    }
    //    currentCon = Condition.normal;
    //}
    public override IEnumerator Frozen(float damage,float effect,float duration)
    {
        Debug.Log("Get Frozen");
        if (currentCon == Condition.Frozen) yield break;
        currentCon = Condition.Frozen;
        float originSpeed = GetMovementSpeed();
        if (effect >= 100)
        {
            eventHandler.SetFrozenTime(duration);
            monsterBase.TakeDamage(0,false);
            yield return new WaitForSeconds(duration);
            monsterBase.TakeDamage(damage);
        }
        else
        {
            DecreaseMovementSpeed(originSpeed * (effect / 100));
            yield return new WaitForSeconds(duration);
            monsterBase.TakeDamage(damage);
            SetMovementSpeed(originSpeed);
        }

        currentCon = Condition.normal;
        currentCC = CC.normal;
    }
    public override IEnumerator Blazed(float damage, float probability, float duration, float interval)
    {
        //   if (currentCon == Condition.Blazed) yield break;
        if (Random.Range(0f, 100f) > probability) yield break;
        currentCon = Condition.Blazed;
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            Debug.Log("Get Blazed");
            DecreaseHealth(damage);
            yield return new WaitForSeconds(interval);
        }
        currentCon = Condition.normal;
    }

}

