using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InfimaGames.LowPolyShooterPack;
using Unity.Services.Core;
public class WeaponSniper : Weapon
{
    // Start is called before the first frame update
    CharacterBehaviour player;
    private bool zoomEffect;
    protected override void Start()
    {
        base.Start();
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        playerStatus = player.GetComponent<PlayerStatus>();
        zoomEffect = false;
    }
    public override void ZoomEffect(bool val)
    {
        Debug.Log("zoomeffect");
        if (val && !zoomEffect)
        {
            playerStatus.SetAttackDamage(playerStatus.GetAttackDamage() + 50);
            zoomEffect = true;
        }
        else if (!val && zoomEffect)
        {
            playerStatus.SetAttackDamage(playerStatus.GetAttackDamage() - 50);
            zoomEffect = false;
        } 

    }
}
