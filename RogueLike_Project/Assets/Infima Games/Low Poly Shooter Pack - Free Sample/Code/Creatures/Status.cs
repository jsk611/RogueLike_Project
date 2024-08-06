using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : StatusBehaviour
{
    [Tooltip("Creature Health")]
    [SerializeField]
    private float Health;

    [Tooltip("Creature Defence")]
    [SerializeField]
    private float Defence;

    [Tooltip("Critical Rate")]
    [SerializeField]
    private float CriticalRate;

    [Tooltip("Critical Damage")]
    [SerializeField]
    private float CriticalDamage;

    [Tooltip("Movement Speed")]
    [SerializeField]
    private float MoveSpeed;

    [Tooltip("Jump Power")]
    [SerializeField]
    private float JumpPower;


    // Start is called before the first frame update
    public override float GetHealth() => Health;
    public override float GetDefence() => Defence;
    public override float GetCriticalRate() => CriticalRate;
    public override float GetCriticalDamage() => CriticalDamage;
    public override float GetMoveSpeed() => MoveSpeed;
    public override float GetJumpPower() => JumpPower;



    public override void SetHealth(float Hp) {  Health = Hp; }
    public override void SetDefence(float Def) { Defence = Def; }
    public override void SetCriticalRate(float CriRate) { CriticalRate = CriRate; }
    public override void SetCriticalDamage(float CriDam) { CriticalDamage = CriDam; }
    public override void SetMoveSpeed(float Speed) { MoveSpeed= Speed; }
    public override void SetJumpPower(float Power) { JumpPower = Power; }




    /// <summary>
    /// Increase Health by parameter Hp (negative parameter by decrease).
    /// </summary>
    /// <param name="Hp"></param>
    public override void IncreaseHealth(float Hp){ Health += Hp; }
    public override void DecreaseHealth(float Hp) { Health -= Hp; }


    public override void IncreaseDefence(float Def) { Defence += Def; }
    public override void DecreaseDefence(float Def) { Defence -= Def; }

    public override void IncreaseCriticalRate(float CriRate) { CriticalRate += CriRate; }
    public override void DecreaseCriticalRate(float CriRate) { CriticalRate -= CriRate; }

    public override void IncreaseCriticalDamage(float CriDam) { CriticalDamage += CriDam; }
    public override void DecreaseCriticalDamage(float CriDam) { CriticalDamage -= CriDam; }

    public override void IncreaseMoveSpeed(float Speed) { MoveSpeed += Speed; }
    public override void DecreaseMoveSpeed(float Speed) { MoveSpeed -= Speed; }

    public override void IncreaseJumpPower(float Power) { JumpPower += Power; }
    public override void DecreaseJumpPower(float Power) { JumpPower -= Power; }


    /// <summary>
    /// Return true if the character die.
    /// </summary>
    /// <returns></returns>
    public override bool IsDie()
    {
        if (Health <= 0) return true;
        else return false;
    }
}
