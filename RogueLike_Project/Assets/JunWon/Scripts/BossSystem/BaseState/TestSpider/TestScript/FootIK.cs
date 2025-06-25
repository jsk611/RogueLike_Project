
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    SpiderPrime spiderPrime;
    BossStatus spiderStatus;
    LegIKManager legIKManager;
    float bodySpeed;
    
    private Vector3 fixedFootpos;
    private Quaternion fixedFootrot;

    public Transform nextFoot;
    private Vector3 nextFootpos;
    private Vector3 nextHintpos;

    float stepInterval;

    [SerializeField] Transform LegHint;
    float hintOffSet;

    [SerializeField] FootIK oppositeLeg;
    bool isMoving = false;
    public bool moveLock = false;
    public bool GetLegMoving => isMoving;

    public enum FootState{
        Default,
        Die,
        End
    }
    public FootState state;
    // Start is called before the first frame update
    void Start()
    {
        fixedFootpos = transform.position;
        fixedFootrot = transform.rotation;
        spiderPrime = GetComponentInParent<SpiderPrime>();
        spiderStatus = spiderPrime.GetComponent<BossStatus>();
        bodySpeed = spiderPrime.BossStatus.GetMovementSpeed();
        hintOffSet = Vector3.Distance(LegHint.position,fixedFootpos);// LegHint.position - fixedFootpos;
        legIKManager = spiderPrime.LegIKManager;

        nextFootpos = transform.position;
        state = FootState.Default;
        stepInterval = spiderStatus.GetMovementSpeed() / 4f;
    }
    private void Update()
    {
        bodySpeed = spiderPrime.BossStatus.GetMovementSpeed();
        stepInterval = spiderStatus.GetMovementSpeed() / 4f;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (moveLock) return;

        else if (state == FootState.Default)
        {
            UpdateNextFootpos(out RaycastHit hit);
            transform.position = fixedFootpos;
            if (Vector3.Distance(fixedFootpos, nextFootpos) > stepInterval && !isMoving && oppositeLeg.GetLegMoving == false)
                StartCoroutine(LegMove(fixedFootpos));
            if (Quaternion.Angle(fixedFootrot, nextFoot.rotation) >= 5f && !isMoving && oppositeLeg.GetLegMoving == false)
                StartCoroutine(LegMove(fixedFootpos));
        }
        else if (state == FootState.Die)
        {
            StartCoroutine(LegDie());
            state = FootState.End;
        }
    }
    public void LegControl(bool val)
    {
        moveLock = val;
    }

    void UpdateNextFootpos(out RaycastHit hit)
    {
        Vector3 MoveDirection = spiderPrime.transform.forward;
        if(Physics.Raycast(nextFoot.position + MoveDirection / 2, -nextFoot.up, out hit, 30f, LayerMask.GetMask("Wall"))) nextFootpos = hit.point;
      //  nextFootpos = nextFoot.position - Vector3.up * 30;
        
        // transform.rotation = fixedFootrot;
        if (Vector3.Angle(hit.normal, Vector3.ProjectOnPlane(hit.normal, Vector3.up)) >= 2)
        {
            Vector3 legHintDirection = hit.normal;

            nextHintpos = nextFootpos + legHintDirection * hintOffSet;
        }
        else
        {
            nextHintpos = nextFootpos + Vector3.up * hintOffSet;
        }
    }
    IEnumerator LegMove(Vector3 curFootpos)
    {
        isMoving = true;

        float elapsedTime = 0f;
        float moveDuration = stepInterval/bodySpeed/1.5f;
        Vector3 centerFootpos = (curFootpos+nextFootpos) / 2 +Vector3.up*legIKManager.StepInterval;
        Vector3 currentHintpos = LegHint.position;
        while (elapsedTime/moveDuration <1f)
        {
            Vector3 newFootpos = Vector3.Lerp(
                Vector3.Lerp(curFootpos, centerFootpos, elapsedTime / moveDuration),
                Vector3.Lerp(centerFootpos, nextFootpos, elapsedTime / moveDuration),
                elapsedTime / moveDuration
            );
            Vector3 newHintpos = Vector3.Lerp(
                currentHintpos, nextHintpos, elapsedTime / moveDuration
            );
            Quaternion newFootrot = Quaternion.Lerp(transform.rotation, nextFoot.rotation, elapsedTime / moveDuration);
           
            fixedFootpos = newFootpos;
            fixedFootrot = newFootrot;

            LegHint.position = newHintpos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isMoving = false;
    }
    IEnumerator LegDie()
    {
    //    yield return new WaitForSeconds(1);
        LegHint.position = new Vector3(LegHint.position.x, -LegHint.position.y, LegHint.position.z);
        float deathTime = 3f;
        float dieTime = 0f;
        float elapsedTime = dieTime / deathTime;

        Vector3 endFoot = nextFoot.localPosition/ 3;
        while (elapsedTime <= 1f)
        {
            elapsedTime = dieTime / deathTime;
            nextFoot.localPosition = Vector3.Lerp(nextFoot.localPosition, endFoot, elapsedTime);

            transform.localPosition = Vector3.Lerp(transform.localPosition,nextFoot.localPosition,elapsedTime);
            dieTime += Time.deltaTime;
            yield return null;
        }
       // this.AddComponent<Rigidbody>();
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(fixedFootpos, Vector3.one*0.1f);
        Gizmos.color = Color.green;
        if(nextFootpos != null)
            Gizmos.DrawWireCube(nextFootpos, Vector3.one * 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(nextHintpos,  0.1f);
    }
}
