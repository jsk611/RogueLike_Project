using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static WormBossBodyMovement;

public class Phase1_AirAssault : State<SpiderPrime>
{
    public Phase1_AirAssault(SpiderPrime owner) : base(owner) { }
    private string AerialAttack = "SpiderAerialAttack";
    private float CrashRange = 14f;
    private float JumpHeight = 40f;
    private bool LegLocked = false;

    TileManager tileManager;
    Transform Player;
    PlayerStatus playerStatus;
    BossStatus bossStatus;

    Vector3 endPos;
    Vector3 curPos;
    Vector3 midPos;
    float glideTime = 0f;
    float aerialDuration = 1f;

    public bool isAttackFinished = false;
    // Start is called before the first frame update
    public override void Enter()
    {
        if (tileManager == null)
        {
            tileManager = GameObject.FindAnyObjectByType<TileManager>();
            Player = owner.Player;
            playerStatus = Player.GetComponent<PlayerStatus>(); 
            bossStatus = owner.BossStatus;
        }
        CrashRange = owner.isBoss ? 14f : 4f;
        JumpHeight = owner.isBoss ? 40f : 10f;
        LegLocked = false;

        endPos = Player.position;
        curPos = owner.transform.position;
        midPos = (endPos+curPos)/2+Vector3.up*JumpHeight;

        glideTime = 0f;

        owner.transform.position = Player.position + Vector3.up * 30;
        owner.NmAgent.isStopped = true;
    }
    public override void Update()
    {
        float elapsedTIme = glideTime / aerialDuration;
        if (elapsedTIme >= 1f)// && Vector3.Distance(Player.position, owner.transform.position) <= 2f)
        {
            isAttackFinished = true;
            RaycastHit hit;
            int wallLayerMask = LayerMask.GetMask("Wall"); // "Wall" ���̾� ����ũ ����
            if (Physics.Raycast(owner.transform.position, Vector3.down, out hit, 4f, wallLayerMask))
            {
                Tile tile = hit.transform.GetComponent<Tile>();
                if (tile != null)
                {
                    int z = (int)tile.transform.position.x / 2;
                    int x = (int)tile.transform.position.z / 2;
                    if (owner.isBoss) owner.CoroutineRunner(tileManager.CreateShockwave(z, x, 6, 3, 0.05f));
                    Collider[] boom = Physics.OverlapSphere(owner.transform.position, CrashRange, LayerMask.GetMask("Character"));
                    if (boom.Length > 0)
                    {
                        playerStatus.DecreaseHealth(bossStatus.GetAttackDamage());
                        if (owner.isBoss && playerStatus.currentCC != StatusBehaviour.CC.entangled)
                        {
                            float speed = playerStatus.GetMovementSpeed() / 3;
                            playerStatus.CoroutineEngine(playerStatus.SlowCoroutine(speed, 3));
                        }
                    }
                }
            }
        }
        else if (elapsedTIme >= 0.2f && !LegLocked)
        {
            owner.LegIKManager.LegStop();
            LegLocked = true;
        }
        else
        {
            owner.transform.rotation = Quaternion.LookRotation(Player.position - owner.transform.position);
            owner.transform.position = Vector3.Lerp(
                Vector3.Lerp(curPos, midPos, elapsedTIme),
                Vector3.Lerp(midPos, endPos, elapsedTIme),
                elapsedTIme
            );
        }
        glideTime += Time.deltaTime;
    }
    public override void Exit() {
        isAttackFinished = false;
        owner.LegIKManager.LegMove();
        owner.LegIKManager.LegReset();
        owner.AbilityManager.SetMaxCoolTime(AerialAttack);
        owner.NmAgent.isStopped = false;
    }
}
