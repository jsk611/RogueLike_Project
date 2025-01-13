using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavity_System_Model : SkillBehaviour
{
    // Start is called before the first frame update
    CharacterBehaviour character;
    PlayerStatus status;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        StartCoroutine(Critititical());
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
