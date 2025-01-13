using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserk : SkillBehaviour
{
    // Start is called before the first frame update
    PlayerStatus status;
    CharacterBehaviour character;
    private void Start()
    {
        character = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        status = character.GetComponent<PlayerStatus>();
    }
    public override void SkillActivation()
    {
        if (!CanActivateSkill()) return;
        StartCoroutine(BloodPact());
    }

    IEnumerator BloodPact()
    {
        float halflife = status.GetHealth() / 2;
        while(status.GetHealth() > halflife)
        {
            status.DecreaseHealth(1);
            yield return null;
        }
    }
}
