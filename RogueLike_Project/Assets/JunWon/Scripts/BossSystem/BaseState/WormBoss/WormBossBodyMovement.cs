using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;
using System;
using InfimaGames.LowPolyShooterPack;

public class WormBossBodyMovement : MonoBehaviour
{
    private float turnTimer = 0f;
    private float turnInterval = 3f;
    private float moveTimer = 0f;

    [SerializeField] List<Transform> bodyList;
    [SerializeField] Transform chaseTarget;

    private Transform wormHead;
    private WormBossPrime wormBoss;
    private BossStatus bossStatus;
    Transform target;
    private float chaseSpeed;
    Quaternion moveDirection;

    TileManager tileManager;

    public enum actionType
    {
        Idle,
        Wandering,
        Flying,
        Digging,
        Rushing,
        Inertia
    }
    public actionType currentActionType = actionType.Idle;
    Dictionary<actionType, Action> moveType;

    public actionType CurrentActionType => currentActionType;


    public List<Transform> BodyList => bodyList;
    public Transform WormHead => wormHead;
    public Transform ChaseTarget => chaseTarget;
    public Quaternion MoveDirection { get { return moveDirection; } set { moveDirection = value; } }
    // Start is called before the first frame update
    void Start()
    {
        wormHead = bodyList[0];
        wormBoss = GetComponent<WormBossPrime>();
        bossStatus = GetComponent<BossStatus>();
        target = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
        chaseSpeed = GetComponent<BossStatus>().GetMovementSpeed();
        tileManager = FindAnyObjectByType<TileManager>();
        
        moveType = new Dictionary<actionType, Action>
        {
            {actionType.Idle, Idle },
            {actionType.Wandering,Wandering},
            {actionType.Flying, Flying},
            {actionType.Digging,Digging },
            {actionType.Rushing,Rushing},
            {actionType.Inertia,AfterRush }
        };
    }

    // Update is called once per frame
    void Update()
    {
        moveType.TryGetValue(currentActionType, out var action);
        action?.Invoke();
        WormMove();
    }
    public void ChangeState(actionType actionType, float speed)
    {
        currentActionType = actionType;
        //moveTimer = 0f;
        chaseSpeed = speed;
    }
    void WormMove()
    {
        wormHead.rotation = Quaternion.Lerp(wormHead.rotation, moveDirection, Time.deltaTime * chaseSpeed);
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
    void Idle()
    {

    }

    void Wandering()
    {
        turnTimer += Time.deltaTime;
        if (turnTimer >= turnInterval || Vector3.Distance(wormHead.position,chaseTarget.position)<=1f)
        {
            turnInterval = UnityEngine.Random.Range(1f, 4f);
            turnTimer = 0f;
            chaseTarget.position = new Vector3(UnityEngine.Random.Range(0, 90), UnityEngine.Random.Range(-8,21), UnityEngine.Random.Range(0, 90));
        }
        moveDirection = Quaternion.LookRotation(chaseTarget.position - wormHead.position);
    }
    void Flying()
    {
        moveTimer += Time.deltaTime;
        chaseTarget.position = wormHead.position + Vector3.up;
        moveDirection = Quaternion.LookRotation(chaseTarget.position - wormHead.position);

        RaycastHit hit;
        int wallLayerMask = LayerMask.GetMask("Wall"); // "Wall" 레이어 마스크 생성
        if (Physics.Raycast(wormHead.position, wormHead.forward, out hit, 4f, wallLayerMask) && currentActionType != actionType.Inertia)
        {
            Tile tile = hit.transform.GetComponent<Tile>();
            if (tile != null)
            {
                int z = (int)wormHead.position.x / 2;
                int x = (int)wormHead.position.z / 2;
                Debug.Log(hit.transform.name);
                StartCoroutine(tileManager.CreateShockwave(z, x, 6, 4));
                Collider[] boom = Physics.OverlapSphere(wormHead.position, 12, LayerMask.GetMask("Character"));
                if (boom.Length > 0)
                {
                    target.GetComponent<PlayerStatus>().DecreaseHealth(bossStatus.GetAttackDamage());
                    StartCoroutine(target.GetComponent<PlayerControl>().AirBorne(target.position - wormHead.position));
                }
                currentActionType = actionType.Inertia;
                //  StartCoroutine(tile.CreateShockwave());
            }
        }
    }
    void Digging()
    {
        chaseTarget.position = target.position - new Vector3(0, 30, 0);
        moveDirection = Quaternion.LookRotation(chaseTarget.position - wormHead.position);
    }
    void Rushing()
    {
        chaseSpeed = wormBoss.BossStatus.GetMovementSpeed() * 2;
        RaycastHit hit;
        int wallLayerMask = LayerMask.GetMask("Wall"); // "Wall" 레이어 마스크 생성
        if (Physics.Raycast(wormHead.position, wormHead.forward, out hit, 4f, wallLayerMask) && currentActionType != actionType.Inertia)
        {
            Tile tile = hit.transform.GetComponent<Tile>();
            if (tile != null)
            {
                int z = (int)wormHead.position.x / 2;
                int x = (int)wormHead.position.z / 2;
                Debug.Log(hit.transform.name);
                StartCoroutine(tileManager.CreateShockwave(z, x, 5, 4));
                Collider[] boom = Physics.OverlapSphere(wormHead.position, 8, LayerMask.GetMask("Character"));
                if (boom.Length > 0)
                {
                    target.GetComponent<PlayerStatus>().DecreaseHealth(bossStatus.GetAttackDamage());
                    StartCoroutine(target.GetComponent<PlayerControl>().AirBorne(target.position - wormHead.position));
                }
                currentActionType = actionType.Inertia;
                //  StartCoroutine(tile.CreateShockwave());
            }
        }

    }
    void AfterRush()
    {
        //dummy
    }
    
}
