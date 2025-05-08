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


        endPos = Player.position;
        curPos = owner.transform.position;
        midPos = (endPos+curPos)/2+Vector3.up*40;

        glideTime = 0f;

        owner.transform.position = Player.position + Vector3.up * 30;
        owner.NmAgent.isStopped = true;
    }
    public override void Update()
    {
        float elapsedTIme = glideTime / aerialDuration;
        if (elapsedTIme >= 1f || Vector3.Distance(Player.position, owner.transform.position) <= 2f)
        {
            isAttackFinished = true;
            RaycastHit hit;
            int wallLayerMask = LayerMask.GetMask("Wall"); // "Wall" 레이어 마스크 생성
            if (Physics.Raycast(owner.transform.position, Vector3.down, out hit, 4f, wallLayerMask))
            {
                Tile tile = hit.transform.GetComponent<Tile>();
                if (tile != null)
                {
                    int z = (int)tile.transform.position.x / 2;
                    int x = (int)tile.transform.position.z / 2;
                    Debug.Log(hit.transform.name);
                    owner.CoroutineRunner(tileManager.CreateShockwave(z, x, 6, 3, 0.05f));
                    Collider[] boom = Physics.OverlapSphere(owner.transform.position, 14, LayerMask.GetMask("Character"));
                    if (boom.Length > 0)
                    {
                        playerStatus.DecreaseHealth(bossStatus.GetAttackDamage());
                        if (playerStatus.currentCC != StatusBehaviour.CC.entangled)
                        {
                            float speed = playerStatus.GetMovementSpeed() / 3;
                            playerStatus.CoroutineEngine(playerStatus.SlowCoroutine(speed, 3));
                        }
                    }
                
                }
            }
        }
        else
        {
            owner.transform.rotation = Quaternion.LookRotation(Player.position - owner.transform.position);
            owner.transform.position = Vector3.Lerp(
                Vector3.Lerp(curPos,midPos,elapsedTIme),
                Vector3.Lerp(midPos,endPos,elapsedTIme),
                elapsedTIme
            );
        }
        glideTime += Time.deltaTime;
    }
    public override void Exit() {
        isAttackFinished = false;
        owner.AbilityManager.SetMaxCoolTime(AerialAttack);
        owner.NmAgent.isStopped = false;
    }
}
