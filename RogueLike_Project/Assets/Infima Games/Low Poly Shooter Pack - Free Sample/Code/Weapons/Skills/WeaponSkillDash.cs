using InfimaGames.LowPolyShooterPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSkillDash : WeaponSkillManager
{
    CharacterBehaviour character;
    PlayerControl playerControl;
    CharacterController characterController;


    [Tooltip("Skill Mask")]
    [SerializeField]
    LayerMask mask;

    private WeaponAttachmentManagerBehaviour weapon;
    private MuzzleBehaviour weaponMuzzle;
    private MagazineBehaviour weaponMagazine;

    private Animator weaponAnimator;

    private Transform playerCamera;

    private IGameModeService gameModeService;
    
    Vector3 movement = Vector3.zero;
    float usedTIme;

    bool jumped = false;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        characterController = character.GetComponent<CharacterController>();
        playerControl = character.GetComponent<PlayerControl>();
    }


    public override void FireSkill()
    {
        movement.y = 24;
        usedTIme = 0;
        if (!jumped)
        {
            Debug.Log("jump");
            StartCoroutine(moving());
        }
        else if (jumped)
        {
            if (playerControl.CheckGrounded())
            {
                Debug.Log(playerControl.isGrounded);
                jumped = false;
            }
            else
            {
                playerControl.Vertical.y = -58f;
                jumped = false;
            }
        }
    }
    IEnumerator fall()
    {
        if (Input.GetKey(KeyCode.E))
        {
            characterController.Move(movement);
        }
        yield return null;
    }

    IEnumerator moving()
    {
        playerControl.Vertical.y = 24;
        jumped = true;
        IncreaseSkillCount();
        while (usedTIme < 2)
        {
            usedTIme += Time.deltaTime;
            Debug.Log("stomp");
            yield return null;
        }
        
    }
}
