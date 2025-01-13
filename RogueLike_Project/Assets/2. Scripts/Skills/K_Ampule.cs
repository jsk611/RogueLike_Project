using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K_Ampule : SkillBehaviour
{
    // Start is called before the first frame update
    PlayerStatus status;
    CharacterBehaviour character;

    float ExtraHealth = 100;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        StartCoroutine(AmpuleActivation());
    }

    IEnumerator AmpuleActivation()
    {
        status.IncreaseMaxHealth(ExtraHealth);
        float StartHealth = status.GetHealth();
        while(StartHealth < status.GetMaxHealth())
        {
            status.IncreaseHealth(1);
            yield return null;
        }
    }
}
