using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;
using System;

public class WormBossBodyMovement : MonoBehaviour
{
    [SerializeField] private float turnTimer = 0f;
    [SerializeField] private float turnInterval = 3f;

    [SerializeField] List<Transform> bodyList;
    [SerializeField] Transform chaseTarget;

    private Transform wormHead;
    private WormBossPrime wormBoss;
    private float chaseSpeed;
    
    public enum actionType
    {
        Idle,
        Wandering,
        Flying,
        Digging,
        Rushing
    }
    public actionType currentActionType = actionType.Idle;
    Dictionary<actionType, Action> moveType;

    public actionType CurrentActionType => currentActionType;


    public List<Transform> BodyList => bodyList;

    // Start is called before the first frame update
    void Start()
    {
        wormHead = bodyList[0];
        wormBoss = GetComponent<WormBossPrime>();
        chaseSpeed = GetComponent<MonsterStatus>().GetMovementSpeed();
        
        moveType = new Dictionary<actionType, Action>
        {
            {actionType.Idle, Idle },
            {actionType.Wandering,Wandering},
            {actionType.Flying, Flying},
            {actionType.Digging,Digging },
            {actionType.Rushing,Rushing}
        };
    }

    // Update is called once per frame
    void Update()
    {
        moveType.TryGetValue(currentActionType, out var action);
        action?.Invoke();
    }
    void Idle()
    {

    }

    void Wandering()
    {
        turnTimer += Time.deltaTime;
        if (turnTimer >= turnInterval || Vector3.Distance(wormHead.position,chaseTarget.position)<=1f)
        {
            turnInterval = UnityEngine.Random.Range(1f, 5f);
            turnTimer = 0f;
            chaseTarget.position = new Vector3(UnityEngine.Random.Range(0, 90), UnityEngine.Random.Range(-8,21), UnityEngine.Random.Range(0, 90));
        }
        wormHead.rotation = Quaternion.Lerp(wormHead.rotation, Quaternion.LookRotation(chaseTarget.position - wormHead.position), Time.deltaTime * chaseSpeed);
        wormHead.position += wormHead.forward * Time.deltaTime * chaseSpeed;
        for (int i = 1; i < bodyList.Count; i++)
        {
            if (Vector3.Distance(bodyList[i].position, bodyList[i - 1].position) > 2.3f)
            {
                bodyList[i].rotation = Quaternion.Lerp(bodyList[i].rotation, Quaternion.LookRotation(bodyList[i - 1].position - bodyList[i].position), Time.deltaTime * chaseSpeed);
                bodyList[i].position += bodyList[i].forward * Time.deltaTime * chaseSpeed;
            }
        }
    }
    void Flying()
    {

    }
    void Digging()
    {

    }
    void Rushing()
    {

    }
}
