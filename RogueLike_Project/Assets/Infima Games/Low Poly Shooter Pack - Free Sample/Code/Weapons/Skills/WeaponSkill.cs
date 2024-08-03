using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSkill : WeaponSkillManager
{
    float recentSkillUsed;
    [Tooltip("Skill Cool Time")]
    [SerializeField]
    float skillCoolTime;

    int skillCount;

    [Tooltip("Bullet Speed")]
    [SerializeField]
    float speed;

    [Tooltip("Skill Reach")]
    [SerializeField]
    float maxReach;

    [Tooltip("Skill Mask")]
    [SerializeField]
    LayerMask mask;

    [Tooltip("Object to Fire by Skill")]
    [SerializeField]
    GameObject prefabProjectile;
    private WeaponAttachmentManagerBehaviour weapon;
    private MuzzleBehaviour weaponMuzzle;
    private MagazineBehaviour weaponMagazine;

    private Animator weaponAnimator;

    private CharacterBehaviour character;

    private Transform playerCamera;

    private IGameModeService gameModeService;

    Tazer tazer;

    [Tooltip("Skill Audio")]
    [SerializeField]
    AudioClip AudioClipSkill;
    // Start is called before the first frame update
    void Awake()
    {
        recentSkillUsed = Time.time;
        skillCount = 1;
        weapon = GetComponent<WeaponAttachmentManagerBehaviour>();
        weaponMuzzle = weapon.GetEquippedMuzzle();
        weaponMagazine = weapon.GetEquippedMagazine();

        weaponAnimator = GetComponent<Animator>();

        gameModeService = ServiceLocator.Current.Get<IGameModeService>();
        character = gameModeService.GetPlayerCharacter();
        playerCamera = character.GetCameraWorld().transform;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool CanActivateSkill()
    {
        
        float currentSkillUsed = Time.time;
        if (currentSkillUsed - recentSkillUsed > skillCoolTime)
        {
            recentSkillUsed = currentSkillUsed;
            skillCount = 1;
  
        }
        if (skillCount > 0) return true;
        else return false;
    }
    public override void SetSkillCoolTime(float newCoolTime)
    {
        Debug.Log("new coolTime &{newcoolTime}");

        skillCoolTime = newCoolTime;
    }

    public override float GetSkillCoolTime() => skillCoolTime;
    
    public override void ResetSkillCount()
    {
        skillCount = 0;
    }

    public override int GetSkillCount() => skillCount;


    public override AudioClip GetAudioClipSkill() => AudioClipSkill;
    

    public override void FireSkill()
    {
        //We need a muzzle in order to fire this weapon!
        if (weaponMuzzle == null)
            return;

        //Make sure that we have a camera cached, otherwise we don't really have the ability to perform traces.
        if (playerCamera == null)
            return;
        weaponMuzzle.Effect();

        const string stateName = "Skill";
        weaponAnimator.Play(stateName, 0, 0.0f);



       
        Transform muzzleSocket = weaponMuzzle.GetSocket();

        Quaternion rotation = Quaternion.LookRotation(playerCamera.forward * 1000f - muzzleSocket.position);

        if (Physics.Raycast(new Ray(playerCamera.position, playerCamera.forward),out RaycastHit hit, maxReach,mask))
        {
            rotation = Quaternion.LookRotation(hit.point - muzzleSocket.position);
        }

        GameObject projectile = Instantiate(prefabProjectile, muzzleSocket.position, rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * speed;
    }
}
