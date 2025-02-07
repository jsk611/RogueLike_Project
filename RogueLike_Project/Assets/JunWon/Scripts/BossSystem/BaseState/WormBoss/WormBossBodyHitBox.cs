using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBossBodyHitBox : MonoBehaviour
{
    [SerializeField] private WormBossPrime wormBoss;
    PlayerStatus playerStatus;

    private void Start()
    {
        playerStatus = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Worm hit by "+other.gameObject.name );
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            float bulletDamage = other.gameObject.GetComponent<Projectile>().bulletDamage;
            wormBoss.TakeDamage(bulletDamage*playerStatus.GetAttackDamage()/100*playerStatus.CalculateCriticalHit());
        }
    }
}
