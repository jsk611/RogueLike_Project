using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState_Troy : State<Troy>
{
    public DieState_Troy(Troy owner) : base(owner) { }

    PlayerStatus player;
    public override void Enter()
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
        GameObject.Instantiate(owner.BOMBEFFECT, owner.transform.position,Quaternion.Euler(Vector3.left*90));
        Collider[] targets = Physics.OverlapSphere(owner.transform.position, 7f, LayerMask.GetMask("Character"));
        if (targets.Length > 0) owner.Player.GetComponent<PlayerStatus>().DecreaseHealth(owner.BossStatus.GetAttackDamage());
        foreach (StatusBehaviour enemies in owner.SUMMONEDMONSTERS)
        {
            if (enemies != null)
            {
                if(enemies.TryGetComponent<MonsterBase>(out MonsterBase enem)) enem.TakeDamage(9999, false, true);
                if(enemies.TryGetComponent<BossBase>(out BossBase boss)) boss.TakeDamage(9999, false);
            }
        }
        GameObject.Destroy(owner.gameObject);
    }
    public override void Update()
    {
    }
    public override void Exit()
    {
    }
}
