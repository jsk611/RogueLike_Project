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

    //속박
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
        Iced,
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
   
    public virtual IEnumerator Shocked(float duration,float interval,float shockTime)
    {
        float startTime = Time.time;
        float latest = 0;
        float currentSpeed;
        while (Time.time-startTime<duration)
        {
            if (latest >= interval)
            {
              
                latest = 0;
                currentSpeed = GetMovementSpeed();
                SetMovementSpeed(0);
                currentCC = CC.entangled;
                yield return new WaitForSeconds(shockTime);
                currentCC = CC.normal;
                SetMovementSpeed(currentSpeed);
              
            }
            latest += Time.deltaTime;
        }
    }
    public virtual IEnumerator Blazed(float effect, float duration,float interval)
    {
        if (currentCon == Condition.Blazed) yield break;
        currentCon = Condition.Blazed;
        float startTime = Time.time;
        while(Time.time-startTime < duration)
        {
            DecreaseHealth(effect);
            yield return new WaitForSeconds(interval);
        }
        currentCon = Condition.normal;
    }
    public virtual IEnumerator Frozen(float duration)
    {
        if (currentCon == Condition.Frozen) yield break;
        currentCon = Condition.Frozen;
        SetMovementSpeed(0);
        currentCC = CC.entangled;
        float currentSpeed = GetMovementSpeed();
        
        yield return new WaitForSeconds(duration);
        
        currentCC = CC.normal;
        SetMovementSpeed(currentSpeed);
        currentCon = Condition.normal;
    }
    public virtual IEnumerator Iced(float effect,float duration,float interval)
    {
        //예외적으로 interval을 둔화율로 판단하겠음
        float currentAtk = GetAttackSpeed();
        float currentMv = GetMovementSpeed();
        DecreaseAttackSpeed(effect);
        DecreaseMovementSpeed(interval);
        yield return new WaitForSeconds(duration);
        SetAttackSpeed(currentAtk);
        SetMovementSpeed(currentMv);
    }

    public virtual IEnumerator Poisoned(float effect, float duration,float interval)
    {
        if (currentCon == Condition.Poisoned) yield break;
        currentCon = Condition.Poisoned;
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            DecreaseHealth(effect);
            yield return new WaitForSeconds(interval);
        }
        currentCon = Condition.normal;
    }

    public virtual void ConditionOverload(Condition con,float effect=1, float duration = 1, float interval = 1,float shockTime = 0.5f)
    {
        currentCon = con;
        switch(currentCon)
        {
            case Condition.Poisoned:
                StartCoroutine(Poisoned(effect, duration,interval));
                break;
            case Condition.Blazed:
                StartCoroutine(Blazed(effect,duration,interval));  
                break;
            case Condition.Frozen:
                StartCoroutine(Frozen(duration));
                break;
            case Condition.Shocked:
                StartCoroutine(Shocked(duration,interval,shockTime));
                break;
            case Condition.Iced:
                StartCoroutine(Iced(effect,duration,interval));
                break;
        }
        return;
    }

}
