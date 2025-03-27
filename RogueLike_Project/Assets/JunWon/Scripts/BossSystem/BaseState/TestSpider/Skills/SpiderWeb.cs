using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    public float sloowTime = 3f;
    PlayerStatus player;
    private void OnTriggerEnter(Collider other)
    {
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>(); 
        if (other.gameObject == other.gameObject)
        {
            StartCoroutine(TargetSlow());
        }
    }
    IEnumerator TargetSlow()
    {
        float speed = player.GetMovementSpeed() / 2;
        player.DecreaseMovementSpeed(speed);
        yield return new WaitForSeconds(sloowTime);
        player.IncreaseMovementSpeed(speed);
    }
}
