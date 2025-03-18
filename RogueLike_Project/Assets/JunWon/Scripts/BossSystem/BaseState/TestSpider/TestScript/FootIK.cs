using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    BodyMove body;
    float bodySpeed;
    
    private Vector3 fixedFootpos;
    private Quaternion fixedFootrot;

    public Transform nextFoot;
    private Vector3 nextFootpos;
    private Vector3 nextHintpos;

    float stepInterval;

    [SerializeField] Transform LegHint;
    float hintOffSet;

    public LegIKManager.LEGS legType;
    public LegIKManager.LEGS oppositeLegType;
    bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        stepInterval = 1;// LegIKManager.instance.StepInterval;
        fixedFootpos = transform.position;
        fixedFootrot = transform.rotation;
        body = GetComponentInParent<BodyMove>();
        bodySpeed = body.SPEED;
        hintOffSet = Vector3.Distance(LegHint.position,fixedFootpos);// LegHint.position - fixedFootpos;
  
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Physics.Raycast(nextFoot.position+body.MoveDirection/3, -nextFoot.up,out RaycastHit hit,30f,LayerMask.GetMask("Wall"));
        nextFootpos = hit.point;

        transform.position = fixedFootpos;
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



        if (Vector3.Distance(fixedFootpos, nextFootpos) > stepInterval && !isMoving && LegIKManager.instance.legState[oppositeLegType] == false)
        {
            StartCoroutine(LegMove(fixedFootpos));
        }
        if (Quaternion.Angle(fixedFootrot, nextFoot.rotation) >= 5f && !isMoving && LegIKManager.instance.legState[oppositeLegType] == false)
        {
            Debug.Log("rotate");
            StartCoroutine(LegMove(fixedFootpos));
        }
    }

    IEnumerator LegMove(Vector3 curFootpos)
    {
        isMoving = true;
        LegIKManager.instance.UpdateLEGS(legType, isMoving);
        float elapsedTime = 0f;
        float moveDuration = stepInterval/bodySpeed/2;
        Vector3 centerFootpos = (curFootpos+nextFootpos) / 2+Vector3.up*LegIKManager.instance.StepInterval/1.5f;
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
        LegIKManager.instance.UpdateLEGS(legType, isMoving);
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
