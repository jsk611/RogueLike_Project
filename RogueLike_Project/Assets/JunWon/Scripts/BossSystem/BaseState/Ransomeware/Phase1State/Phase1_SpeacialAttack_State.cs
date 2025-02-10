using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Phase1_SpeacialAttack_State : State<Ransomware>
{
    private bool isAttackFinished = false;
    public Phase1_SpeacialAttack_State(Ransomware owner) : base(owner) {
        owner.SetSpecialAttackState(this);
    }


    public override void Enter()
    {
        isAttackFinished = false;
        owner.Animator.SetTrigger("DataExplode");
        LockPlayerSkill();
    }

    public override void Update()
    {
     
    }

    void LockPlayerSkill()
    {
        Character player = owner.Player.GetComponent<Character>();
        if (player != null)
        {
            Debug.Log("IsWeaponExchangeLocked");
            player.IsCursorLocked();
            player.LockChangedWeapon();
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }


    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
