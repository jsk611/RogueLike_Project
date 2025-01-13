using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Havel_The_Rock : SkillBehaviour
{
    // Start is called before the first frame update
    CharacterBehaviour character;
    PlayerStatus status;
    PlayerControl control;

    float HealthShield = 50;

    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
        control = character.GetComponent<PlayerControl>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
    }

    IEnumerator KnightofHavel()
    {
        control.dashOver = true;
        while (status.GetHealth() >= HealthShield) 
        {
            yield return null;
        }
        status.SetHealth(HealthShield);
    }
}
