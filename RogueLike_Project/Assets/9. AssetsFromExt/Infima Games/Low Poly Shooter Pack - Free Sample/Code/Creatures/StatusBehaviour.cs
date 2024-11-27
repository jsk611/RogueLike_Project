using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditorInternal;
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



    //public abstract void IncreaseEffectResist(float effectResist);
    //public abstract void DecreaseEffectResist(float effectResist);
    //public abstract void SetEffectResist(float effectResist);
    //public abstract float GetEffectResist();


    //public abstract void IncreaseDefence(float defence);
    //public abstract void DecreaseDefence(float defence);
    //public abstract void SetDefence(float defence);
    //public abstract float GetDefence();

    //public abstract void IncreaseDamageAlleviation(float alleviation);
    //public abstract void DecreaseDamageAlleviation(float alleviation);
    //public abstract void SetAlleviation(float alleviation);
    //public abstract float GetDamageAlleviation();

    public abstract void IncreaseCriticalRate(float criticalRate);
    public abstract void DecreaseCriticalRate(float criticalRate);
    public abstract void SetCriticalRate(float criticalRate);
    public abstract float GetCriticalRate();

    public abstract void IncreaseCriticalDamage(float criticalDamage);
    public abstract void DecreaseCriticalDamage(float criticalDamage);
    public abstract void SetCriticalDamage(float criticalDamage);
    public abstract float GetCriticalDamage();

    public abstract float CalculateCriticalHit();

    public abstract void IncreaseMovementSpeed(float moveSpeed);
    public abstract void DecreaseMovementSpeed(float moveSpeed);
    public abstract void SetMovementSpeed(float moveSpeed);
    public abstract float GetMovementSpeed();

    public abstract void IncreaseReloadSpeed(float reloadSpeed);
    public abstract void DecreaseReloadSpeed(float reloadSpeed);
    public abstract void SetReloadSpeed(float reloadSpeed);
    public abstract float GetReloadSpeed();

    public abstract void IncreaseAttackDamage(float attackDamage);
    public abstract void DecreaseAttackDamage(float attackDamage);
    public abstract void SetAttackDamage(float attackDamage);
    public abstract float GetAttackDamage();

    public abstract void IncreaseAttackSpeed(float attackSpeed);
    public abstract void DecreaseAttackSpeed(float attackSpeed);
    public abstract void SetAttackSpeed(float attackSpeed);
    public abstract float GetAttackSpeed();

    //public abstract void IncreaseJumpPower(float jumpPower);
    //public abstract void DecreaseJumpPower(float jumpPower);
    //public abstract void SetJumpPower(float jumpPower);
    //public abstract float GetJumpPower();

    //¼Ó¹Ú
    public enum CC
    {
        normal,
        entangled,
        frozen,
    }

    public enum Condition{
        normal,
        Blazed,
        Shocked,
        Frozen,
        Poisoned
    }
    public CC currentCC;
    public Condition currentCon;

    public IEnumerator Slow(float effect, float duration)
    {
        Debug.Log("slow");
        DecreaseMovementSpeed(effect);
        yield return new WaitForSeconds(duration);
        IncreaseMovementSpeed(effect);
    }
    
    public IEnumerator Accelerate(float effect,float duration)
    {
        IncreaseMovementSpeed(effect);
        yield return new WaitForSeconds(duration);
        DecreaseMovementSpeed(effect);
    }
   
    public IEnumerator Shocked(float effect, float duration,float interval,float shockTime)
    {
        float time = 0;
        float currentSpeed;
        while (time <= duration)
        {
            
            currentSpeed = GetMovementSpeed();
            SetMovementSpeed(0);
            currentCC = CC.entangled;
            yield return new WaitForSeconds(shockTime);
            currentCC = CC.normal;
            SetMovementSpeed(currentSpeed);
            time += interval;
        }
    }
    public IEnumerator Blazed(float effect, float duration,float interval)
    {
        currentCon = Condition.Blazed;
        float startTime = Time.time;
        while(Time.time-startTime < duration)
        {
            DecreaseHealth(effect);
            yield return new WaitForSeconds(interval);
        }
        currentCon = Condition.normal;
    }
    public IEnumerator Frozen(float effect, float duration)
    {
        currentCon = Condition.Frozen;
        currentCC = CC.entangled;
        float currentSpeed = GetMovementSpeed();
        SetMovementSpeed(0);
        yield return new WaitForSeconds(duration);
        SetMovementSpeed(currentSpeed);
        currentCC = CC.normal;
        currentCon = Condition.normal;
    }

    public IEnumerator Poisoned(float effect, float duration,float interval)
    {
        currentCon = Condition.Poisoned;
        float startTime = Time.time;   
        while(Time.time - startTime < duration)
        {
            DecreaseHealth(effect);
            yield return new WaitForSeconds(interval);
        }
        currentCon = Condition.normal;
    }

    public void ConditionOverload(Condition con,float effect, float duration, float interval,float shockTime = 0.5f)
    {
        currentCon = con;
        switch(currentCon)
        {
            case Condition.Poisoned:
                StartCoroutine(Frozen(effect, duration));
                break;
            case Condition.Blazed:
                StartCoroutine(Blazed(effect,duration,interval));  
                break;
            case Condition.Frozen:
                StartCoroutine(Frozen(effect, duration));
                break;
            case Condition.Shocked:
                StartCoroutine(Shocked(effect,duration,interval,shockTime));
                break;
        }
        return;
    }

}
