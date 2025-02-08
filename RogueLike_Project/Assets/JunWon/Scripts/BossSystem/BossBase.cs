using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossBase : MonoBehaviour
{
    #region Serialized Fields
    [Header("General Settings")]
    [SerializeField] protected Transform target;
    [SerializeField] protected Transform body; // Character body (XZ rotation)
    [SerializeField] protected Transform head; // Head or torso (vertical rotation)
    [SerializeField] protected float maxVerticalAngle = 60f; // Maximum vertical angle for head rotation
    [SerializeField] protected float rotateSpeed = 2.0f; // Rotation speed
 


    [Header("Components")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] protected BossStatus bossStatus;
    [SerializeField] protected Rigidbody playerRigidBody;


    [Header("Effects")]
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected GameObject spawnEffect;
    [SerializeField] protected Material startMaterial;
    [SerializeField] protected Material baseMaterial;
    [SerializeField] protected GameObject[] items;
    [SerializeField] protected int[] itemProbability = { 50, 25, 0 };
    [SerializeField] protected float height = 5f;
    [SerializeField] protected int DNADrop = 0;

    [Header("UI")]
    [SerializeField] public EnemyHPBar HPBar;
    [SerializeField] protected GameObject UIDamaged;

    [Header("External Data")]
    [SerializeField] protected EnemyCountData enemyCountData;

    [Header("StateMachine")]
    [SerializeField] protected StateMachine<WormBossPrime> fsm;
    #endregion


    public virtual void TakeDamage(float damage, bool showDamage = true)
    {
        EventManager.Instance.TriggerMonsterDamagedEvent();

        bossStatus.DecreaseHealth(damage);

        if (showDamage) Instantiate(UIDamaged, transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0), Quaternion.identity).GetComponent<UIDamage>().damage = damage;
        // HPBar?.SetRatio(hp, monsterStatus.GetMaxHealth());

    }

}
