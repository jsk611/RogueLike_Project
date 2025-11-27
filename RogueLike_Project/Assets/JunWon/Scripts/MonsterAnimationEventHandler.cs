using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimationEventHandler : MonoBehaviour
{

    private MonsterBase monsterBase;
    private Animator animator;
    private StatusBehaviour statusBehaviour;

    private PlayerStatus player;
    private PlayMonsterSound sound;
    

    private float FrozenTime;
    private float ShockTime;

    // Start is called before the first frame update
    void Start()
    {
        monsterBase = GetComponent<MonsterBase>(); 
        animator = GetComponent<Animator>();
        statusBehaviour = GetComponent<StatusBehaviour>();
        sound = GetComponent<PlayMonsterSound>();

        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
    }

    void MeleeAttack()
    {
        //  Debug.Log("try melee attack "+Vector3.Distance(transform.position,player.transform.position));

        if (Vector3.Distance(transform.position, player.transform.position) <= monsterBase.GetRange())
        {
            player.DecreaseHealth(statusBehaviour.GetAttackDamage());
        }
    }
    void CCbyCondition()
    {
        if (statusBehaviour.currentCon == StatusBehaviour.Condition.Frozen) { StartCoroutine(Frozen()); }
        else if (statusBehaviour.currentCon == StatusBehaviour.Condition.Shocked ) { StartCoroutine(Shocked()); }

    }

    void DeathSignalConfirmed()
    {

    }
 

    IEnumerator Frozen()
    {
        {
            Debug.Log("Frozed");
            float currentSpeed = statusBehaviour.GetMovementSpeed();
            statusBehaviour.SetMovementSpeed(0);
        //    monsterBase.enabled = false;
            animator.speed = 0f;

            yield return new WaitForSeconds(FrozenTime);
            statusBehaviour.SetMovementSpeed(currentSpeed);
            animator.speed = 1f;
        //    monsterBase.enabled = true;

            monsterBase.UpdateStateFromAnimationEvent();
        }
    }
    IEnumerator Shocked()
    {
        Debug.Log("Shocked");
        //  monsterBase.enabled = false;
        animator.speed = 0f;
        float currentSpeed = statusBehaviour.GetMovementSpeed();
        statusBehaviour.SetMovementSpeed(0);
        // animator.GetComponent<Rigidbody>().AddForce(Vector3.up*10,ForceMode.Impulse);
        yield return new WaitForSeconds(ShockTime);
        animator.speed = 1f;
        statusBehaviour.SetMovementSpeed(currentSpeed);
        //  monsterBase.enabled = true;

        monsterBase.UpdateStateFromAnimationEvent();
    }
    public void SetFrozenTime(float time) { FrozenTime = time; }
    public void SetShockTime (float time) { ShockTime = time;}
}
