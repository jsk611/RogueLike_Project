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
        StartCoroutine(moving());
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
        while (usedTIme < 2)
        {
            usedTIme += Time.deltaTime;
            if (Input.GetKey(KeyCode.Q)) Debug.Log("stomp");
            yield return null;
        }
    }
}
