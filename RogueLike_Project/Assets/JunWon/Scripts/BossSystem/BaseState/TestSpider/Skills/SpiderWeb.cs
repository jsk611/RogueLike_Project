using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    public float slowTime = 3f;
    PlayerStatus player;
    private void OnTriggerEnter(Collider other)
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>(); 
        if (player.currentCC != StatusBehaviour.CC.entangled && other.gameObject == player.gameObject)
        {
            float speed = player.GetMovementSpeed() / 2;
            StartCoroutine(player.Slow(speed,slowTime));
        }
    }

}
