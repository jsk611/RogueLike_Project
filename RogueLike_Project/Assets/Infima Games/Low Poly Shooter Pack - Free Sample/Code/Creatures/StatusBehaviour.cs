using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public abstract float GetHealth();
    public abstract void SetHealth(float Hp);
    public abstract void IncreaseHealth(float Hp);
    public abstract void DecreaseHealth(float Hp);
    public abstract bool IsDie();

    public abstract float GetDefence();
    public abstract void SetDefence(float Def);
    public abstract void IncreaseDefence(float Def);
    public abstract void DecreaseDefence(float Def);


    public abstract float GetCriticalRate();
    public abstract void SetCriticalRate(float CriRate);
    public abstract void IncreaseCriticalRate(float CriRate);
    public abstract void DecreaseCriticalRate(float CriRate);

    public abstract float GetCriticalDamage();
    public abstract void SetCriticalDamage(float CriDam);
    public abstract void IncreaseCriticalDamage(float CriDam);
    public abstract void DecreaseCriticalDamage(float CriDam);

    public abstract float GetMoveSpeed();
    public abstract void SetMoveSpeed(float Speed);
    public abstract void IncreaseMoveSpeed(float Speed);
    public abstract void DecreaseMoveSpeed(float Speed);

    public abstract float GetJumpPower();
    public abstract void SetJumpPower(float Power);
    public abstract void IncreaseJumpPower(float Power);
    public abstract void DecreaseJumpPower(float Power);
}
