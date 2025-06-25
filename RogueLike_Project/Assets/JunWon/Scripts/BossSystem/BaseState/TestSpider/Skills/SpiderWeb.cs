using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    public float slowTime = 1f;
    PlayerStatus player;
    private void OnTriggerEnter(Collider other)
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>(); 
        if (player.currentCC != StatusBehaviour.CC.entangled && other.gameObject == player.gameObject)
        {
            float speed = player.GetMovementSpeed() / 3;
            player.CoroutineEngine(player.SlowCoroutine(speed,slowTime));
        }
    }

}
