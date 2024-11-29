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
        if (statusBehaviour.currentCon == StatusBehaviour.Condition.Frozen) { StartCoroutine(Frozen()); }
        else if (statusBehaviour.currentCon == StatusBehaviour.Condition.Shocked ) { StartCoroutine(Shocked()); }

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
            statusBehaviour.currentCon = StatusBehaviour.Condition.normal;
            statusBehaviour.currentCC = StatusBehaviour.CC.normal;
            monsterBase.UpdateStateFromAnimationEvent();
        }
    }
    IEnumerator Shocked()
    {
        Debug.Log("Shocked");
      //  monsterBase.enabled = false;
  
       // animator.GetComponent<Rigidbody>().AddForce(Vector3.up*10,ForceMode.Impulse);
        yield return new WaitForSeconds(ShockTime);
    
      //  monsterBase.enabled = true;
        statusBehaviour.currentCon = StatusBehaviour.Condition.normal;
        statusBehaviour.currentCC = StatusBehaviour.CC.normal;
        monsterBase.UpdateStateFromAnimationEvent();
    }
    public void SetFrozenTime(float time) { FrozenTime = time; }
    public void SetShockTime (float time) { ShockTime = time;}
}
