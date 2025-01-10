// Copyright 2021, Infima Games. All Rights Reserved.

using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class WeaponGrenade: WeaponBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Firing")]

        [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
        [SerializeField]
        private bool automatic;

        [Tooltip("How fast the projectiles are.")]
        [SerializeField]
        private float projectileImpulse = 400.0f;

        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField]
        private float roundsPerSeconds = 200;

        [Tooltip("Mask of things recognized when firing.")]
        [SerializeField]
        private LayerMask mask;

        [Tooltip("Maximum distance at which this weapon can fire accurately. Shots beyond this distance will not use linetracing for accuracy.")]
        [SerializeField]
        private float maximumDistance = 500.0f;

        [Header("Animation")]

        [Tooltip("Transform that represents the weapon's ejection port, meaning the part of the weapon that casings shoot from.")]
        [SerializeField]
        private Transform socketEjection;

        [Header("Resources")]

        [Tooltip("Casing Prefab.")]
        [SerializeField]
        private GameObject prefabCasing;

        [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
        [SerializeField]
        private GameObject prefabProjectile;

        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField]
        public RuntimeAnimatorController controller;

        [Tooltip("Weapon Body Texture.")]
        [SerializeField]
        private Sprite spriteBody;

        [Header("Audio Clips Holster")]

        [Tooltip("Holster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipHolster;

        [Tooltip("Unholster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipUnholster;

        [Header("Audio Clips Reloads")]

        [Tooltip("Reload Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReload;

        [Tooltip("Reload Empty Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadEmpty;

        [Header("Audio Clips Other")]

        [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
        [SerializeField]
        private AudioClip audioClipFireEmpty;

        [Tooltip("Exchange weapon when ammunition is zero")]
        [SerializeField]
        private WeaponBehaviour exchangeWeapon;

        #endregion

        #region FIELDS

        /// <summary>
        /// Weapon Animator.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Attachment Manager.
        /// </summary>
        private WeaponAttachmentManagerBehaviour attachmentManager;

        /// <summary>
        /// Amount of ammunition left.
        /// </summary>
        private int ammunitionCurrent;

        #region Attachment Behaviours

        /// <summary>
        /// Equipped Magazine Reference.
        /// </summary>
        private MagazineBehaviour magazineBehaviour;
        /// <summary>
        /// Equipped Muzzle Reference.
        /// </summary>
        private MuzzleBehaviour muzzleBehaviour;

        #endregion

        /// <summary>
        /// The GameModeService used in this game!
        /// </summary>
        private IGameModeService gameModeService;
        /// <summary>
        /// The main player character behaviour component.
        /// </summary>
        private CharacterBehaviour characterBehaviour;

        /// <summary>
        /// The player character's camera.
        /// </summary>
        private Transform playerCamera;

        /// <summary>
        /// The player character's status
        /// </summary>
        private PlayerStatus playerStatus;

        private static readonly int HashReloadSpeed = Animator.StringToHash("Reload Speed");
        private static readonly int HashAtackSpeed = Animator.StringToHash("Fire Speed");



        private LineRenderer lineRenderer;
        private Coroutine drawLine;
        private Transform upperBody;

        #endregion

        #region UNITY

        protected override void Awake()
        {
            //Get Animator.
            animator = GetComponent<Animator>();
            //Get Attachment Manager.
            attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();

            //Cache the game mode service. We only need this right here, but we'll cache it in case we ever need it again.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            //Cache the player character.
            characterBehaviour = gameModeService.GetPlayerCharacter();
            //Cache the world camera. We use this in line traces.
            //Cache the character status
            playerStatus = characterBehaviour.GetComponent<PlayerStatus>();

            lineRenderer = GetComponent<LineRenderer>();   

            upperBody = gameModeService.GetPlayerCharacter().GetComponentInChildren<CameraLook>().transform;
        }
        protected override void Start()
        {
            #region Cache Attachment References

            //Get Magazine.
            magazineBehaviour = attachmentManager.GetEquippedMagazine();
            //Get Muzzle.
            muzzleBehaviour = attachmentManager.GetEquippedMuzzle();

            #endregion

            //Max Out Ammo.
            ammunitionCurrent = magazineBehaviour.GetAmmunitionTotal();
        }
        private void OnEnable()
        {
            animator.SetFloat(HashReloadSpeed, playerStatus.GetReloadSpeed());
            animator.SetFloat(HashAtackSpeed, playerStatus.GetAttackSpeed());
        }
        #endregion

        #region GETTERS

        public override Animator GetAnimator() => animator;

        public override Sprite GetSpriteBody() => spriteBody;

        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;

        public override AudioClip GetAudioClipReload() => audioClipReload;
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;

        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;

        public override AudioClip GetAudioClipFire() => muzzleBehaviour.GetAudioClipFire();



        public override int GetAmmunitionCurrent() => ammunitionCurrent;

        public override int GetAmmunitionTotal() => magazineBehaviour.GetAmmunitionTotal();

        public override bool IsAutomatic() => automatic;
        public override float GetRateOfFire() => roundsPerSeconds * playerStatus.GetAttackSpeed();

        public override bool IsFull() => ammunitionCurrent == magazineBehaviour.GetAmmunitionTotal();
        public override bool HasAmmunition() => ammunitionCurrent > 0;

        public override RuntimeAnimatorController GetAnimatorController() => controller;
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;

        #endregion

        #region METHODS

        public override void Reload()
        {
            ;
        }
        public override void Fire(float spreadMultiplier = 1.0f)
        {
            projectileImpulse = 20;
            StartCoroutine(standby());
        }

        public override void FillAmmunition(int amount)
        {
            //Update the value by a certain amount.
            ammunitionCurrent = amount != 0 ? Mathf.Clamp(ammunitionCurrent + amount,
                0, GetAmmunitionTotal()) : magazineBehaviour.GetAmmunitionTotal();

        }

        public override void EjectCasing()
        {
            //Spawn casing prefab at spawn point.
            if (prefabCasing != null && socketEjection != null)
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        public override void Throw()
        {
            ServiceLocator.Current.Get<IAudioManagerService>().PlayOneShotDelayed(GetAudioClipFireEmpty(),new AudioSettings(1,0,true),0);
            //getcamera
            playerCamera = characterBehaviour.GetCameraWorld().transform;
            //We need a muzzle in order to fire this weapon!
            if (muzzleBehaviour == null)
                return;

            //Make sure that we have a camera cached, otherwise we don't really have the ability to perform traces.
            if (playerCamera == null)
                return;

            //Get Muzzle Socket. This is the point we fire from.
            Transform muzzleSocket = muzzleBehaviour.GetSocket();

            //Play the firing animation.
            const string stateName = "Fire";
   //         animator.Play(stateName, 0, 0.0f);
            //Reduce ammunition! We just shot, so we need to get rid of one!
            ammunitionCurrent = Mathf.Clamp(ammunitionCurrent - 1, 0, magazineBehaviour.GetAmmunitionTotal());

            //Play all muzzle effects.


            //Determine the rotation that we want to shoot our projectile in.
            Quaternion rotation = Quaternion.LookRotation(playerCamera.forward * 10000000.0f - muzzleSocket.position);

            //If there's something blocking, then we can aim directly at that thing, which will result in more accurate shooting.
            if (Physics.Raycast(new Ray(playerCamera.position, playerCamera.forward),
                out RaycastHit hit, maximumDistance, mask))
                rotation = Quaternion.LookRotation(hit.point - muzzleSocket.position);

            //Spawn projectile from the projectile spawn point.
            GameObject projectile = Instantiate(prefabProjectile, muzzleSocket.position, rotation);

            //Add velocity to the projectile.
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileImpulse;

            muzzleBehaviour.Effect();

            StopCoroutine(drawLine);
            lineRenderer.enabled = false;
        }

        IEnumerator standby()
        {
            //        while(characterBehaviour.GetPlayerAnimator().speed > 0) yield return null;
            lineRenderer.enabled = true;

            drawLine = StartCoroutine(drawTrace());
            while (characterBehaviour.GetHoldingFire())
            {
                projectileImpulse += Time.deltaTime*30;
                projectileImpulse = Mathf.Clamp(projectileImpulse, 0, 60);
                //   rotation = Quaternion.LookRotation(playerCamera.forward * 10000000.0f - muzzleBehaviour.GetSocket().position);
                yield return null;
            }
            while(characterBehaviour.GetPlayerAnimator().speed > 0) yield return null;
            characterBehaviour.GetPlayerAnimator().speed = 1.0f;


         
        }
        private IEnumerator drawTrace()
        {
            while (true)
            {
                float G = -Physics.gravity.y;
                float sin = Mathf.Sin(-upperBody.eulerAngles.x * Mathf.Deg2Rad);
                float cos = Mathf.Cos(-upperBody.eulerAngles.x * Mathf.Deg2Rad);
                lineRenderer.SetPosition(0, gameObject.transform.position);
                float t = (projectileImpulse * sin + Mathf.Pow((Mathf.Pow(projectileImpulse * sin, 2) + 2 * G * gameObject.transform.position.y), 0.5f)) / G;
                float temp = 0;

                for (int i = 1; i < lineRenderer.positionCount; i++)
                {
                    temp += t / lineRenderer.positionCount;
                    float x = CalX(temp, projectileImpulse, cos);
                    float z = CalZ(temp, projectileImpulse, cos);

                    lineRenderer.SetPosition(i, new Vector3(x * Mathf.Sin(Mathf.Abs(characterBehaviour.transform.eulerAngles.y) * Mathf.Deg2Rad) + transform.position.x, CalY(temp, projectileImpulse, sin, G), z * Mathf.Cos(Mathf.Abs(characterBehaviour.transform.eulerAngles.y) * Mathf.Deg2Rad) + gameObject.transform.position.z));//CalZ(temp, projectileImpulse, cos)*Mathf.Cos(characterBehaviour.transform.localEulerAngles.y)));
                }
                yield return null;
            }
        }

        private float CalX(float t,float v,float cos)
        { return v * cos * t; }
        private float CalY(float t,float v,float sin,float g)
        { return v * sin * t - 0.5f * g * t * t + gameObject.transform.position.y; }
        private float CalZ(float t, float v, float cos)
        { return v*cos*t; }
        #endregion
    }
}
