using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Phase1_SpeacialAttack_State : State<Ransomware>
{
    float lockTimer = 0.0f;
    float lockCoolDown = 5.0f;
    public Phase1_SpeacialAttack_State(Ransomware owner) : base(owner) { }


    public override void Enter()
    {
        // owner.Animator.SetTrigger("SpecialAttack");
        Debug.Log("[Phase1_SpeacialAttack_State] Enter");
        LockPlayerSkill();
        lockTimer = 0.0f;
    }

    public override void Update()
    {
        lockTimer += Time.deltaTime;
        if (lockTimer > lockCoolDown) {
            lockTimer = 0.0f;
            // owner.Animator.SetTrigger("SpecialAttack");
            Debug.Log("IsWeaponExchangeLocked");
            LockPlayerSkill();
        }
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
}
