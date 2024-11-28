using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimationEventHandler : MonoBehaviour
{

    private MonsterBase monsterBase;
    private Animator animator;
    private StatusBehaviour statusBehaviour;
    

    private float FrozenTime;
    private float ShockTime;

    // Start is called before the first frame update
    void Start()
    {
        monsterBase = GetComponent<MonsterBase>(); 
        animator = GetComponent<Animator>();
        statusBehaviour = GetComponent<StatusBehaviour>();
    }
    void CCbyCondition()
    {
        Debug.Log("checking condition");
        if (statusBehaviour.currentCon == StatusBehaviour.Condition.Frozen) { Frozen(); }
        else if (statusBehaviour.currentCon == StatusBehaviour.Condition.Shocked ) { Shocked(); }
        statusBehaviour.currentCon = StatusBehaviour.Condition.normal;
        statusBehaviour.currentCC = StatusBehaviour.CC.normal;
    }
    void Frozen()
    {
        Debug.Log("Frozed");
        float currentSpeed = statusBehaviour.GetMovementSpeed();
        statusBehaviour.SetMovementSpeed(0);
        animator.speed = 0f;
        monsterBase.enabled = false;
        new WaitForSeconds(10);
        statusBehaviour.SetMovementSpeed(currentSpeed);
        animator.speed = 1f;
        monsterBase.enabled = true;
    }
    void Shocked()
    {
        Debug.Log("Shocked");
        monsterBase.enabled = false;
        animator.speed = 0f;
       // animator.GetComponent<Rigidbody>().AddForce(Vector3.up*10,ForceMode.Impulse);
        new WaitForSeconds(ShockTime);
        animator.speed = 1f;
        monsterBase.enabled = true;
    }
    public void SetFrozenTime(float time) { FrozenTime = time; }
    public void SetShockTime (float time) { ShockTime = time;}
}
