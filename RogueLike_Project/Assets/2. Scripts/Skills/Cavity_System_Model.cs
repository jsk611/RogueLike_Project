using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavity_System_Model : SkillBehaviour
{
    // Start is called before the first frame update
    CharacterBehaviour character;
    PlayerStatus status;
    float IncreaseCriticalRate = 16f;
    float ConsumeCoinRate = 100f;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        recentSKillUsed = Time.time;
       // StartCoroutine(Critititical());
        
        status.IncreaseCriticalDamage((int)(status.GetCoin()/ConsumeCoinRate*IncreaseCriticalRate));
        status.SetCoin(0);
    }

    IEnumerator Critititical()
    {
        float halfcoin = status.GetCoin()/2;
        float startcoin = status.GetCoin();
        status.IncreaseCriticalDamage(halfcoin);
        while(startcoin>halfcoin)
        {
            status.DecreaseCoin(1);
            yield return null;
        }
    }
}
